using Library.Analyzer.Automata;
using Library.Analyzer.Collections;
using Library.Analyzer.Forest;
using Library.Analyzer.Grammars;
using Library.General.Workspace;
using Library.InterfaceConnection.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Parameter = Library.General.Workspace.Parameter;

namespace Library.General.NameTable
{
    public class NameSearchForestNodeVisitor : IForestNodeVisitor
    {
        private HashSet<IForestNode> _visited;
        private TextBoxWriter _writer;
        private ISymbolForestNode _name;
        private UniqueList<ITokenForestNode> _nameValue;
        private NameElementType _nameType;
        public ModuleNameTable _nameTable;
        public List<Parameter> _parameters;

        public NameSearchForestNodeVisitor(TextBoxWriter streamWriter)
        {
            _visited = new HashSet<IForestNode>();
            _writer = streamWriter;
            _nameTable = new ModuleNameTable("Name table");
            _parameters = new List<Parameter>();

        }

        public void Visit(ITokenForestNode tokenNode)
        {
                _visited.Add(tokenNode);
            return;
        }
        
        public void Visit(IAndForestNode andNode)
        {

            for (var i = 0; i < andNode.Children.Count; i++)
            {
                var child = andNode.Children[i];
                child.Accept(this);
            }
        }

        public void Visit(IIntermediateForestNode node)
        {

            if (!_visited.Add(node))
                return;
       

            for (var i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                Visit(child);
            }
        }

        public void Visit(ISymbolForestNode node)
        {
            if (!_visited.Add(node))
                return;

            if (node.Symbol is INonTerminal nonTerminal)
            {
                HandleNode(node, nonTerminal);
            }

            ProcessChildren(node);
        }

        private void HandleNode(ISymbolForestNode node, INonTerminal nonTerminal)
        {
            if (nonTerminal.Value.Equals("Main_Name") || nonTerminal.Value.Equals("Additional_Name"))
            {
                HandleMainNameNode(node, nonTerminal);
            }
            else if (nonTerminal.Value.Equals("Limit") || nonTerminal.Value.Equals("Param") || nonTerminal.Value.Equals("Constructs"))
            {
                HandleParameterNode(node);
            }
        }

        private void HandleMainNameNode(ISymbolForestNode node, INonTerminal nonTerminal)
        {
            Print();

            _name = node;
            _nameValue = new UniqueList<ITokenForestNode>();
            _nameType = nonTerminal.Value.Equals("Main_Name") ? NameElementType.MainName : NameElementType.AdditionalName;
        }

        private void HandleParameterNode(ISymbolForestNode node)
        {
            Print();

            var childNodes = GetChildNodes(node);
            ExtractParamsAndConstruct(childNodes);

        }

        private List<IForestNode> GetChildNodes(ISymbolForestNode node)
        {
            var childNodes = new List<IForestNode>();

            foreach (var andNode in node.Children)
            {
                foreach (var child in andNode.Children)
                {
                    if (child is ITokenForestNode tokenNode)
                    {
                        childNodes.Add(tokenNode);
                    }
                    else if (child is IIntermediateForestNode intermediateNode)
                    {
                        var flattened = GetFlattenedList(intermediateNode);
                        childNodes.AddRange(flattened.OfType<ITokenForestNode>());
                    }
                }
            }

            return childNodes;
        }

        private void ProcessChildren(ISymbolForestNode node)
        {
            foreach (var andNode in node.Children)
            {
                foreach (var child in andNode.Children)
                {
                    CheckChild(child);
                }
            }

            foreach (var child in node.Children)
            {
                Visit(child);
            }
        }

        public void Visit(ITerminalForestNode node)
        {
            CheckChild(node);
        }

        private void CheckChild(IForestNode node, int index = -1)
        {
            if (_name != null)
            {
                switch (node.NodeType)
                {
                    case ForestNodeType.Intermediate:
                        var intermediate = node as IIntermediateForestNode;
                        if (intermediate.Children.Count > 1)
                            throw new Exception("Intermediate node has more children than expected. ");
                        var flatList = GetFlattenedList(intermediate);
                        for (var i = 0; i < flatList.Count; i++)
                        {
                            CheckChild(flatList[i], flatList.Count);
                        }
                        break;

                    case ForestNodeType.Symbol:
                        break;

                    case ForestNodeType.Token:
                        var tokenForestNode = node as ITokenForestNode;
                        _nameValue.Add(tokenForestNode);
                        break;
                }
            }
        }

        private static IList<IForestNode> GetFlattenedList(IIntermediateForestNode intermediate)
        {
            var children = new List<IForestNode>();
            for (var a = 0; a < intermediate.Children.Count; a++)
            {
                var andNode = intermediate.Children[a];
                for (var c = 0; c < andNode.Children.Count; c++)
                {
                    var child = andNode.Children[c];
                    switch (child.NodeType)
                    {
                        case ForestNodeType.Intermediate:
                            var childList = GetFlattenedList(child as IIntermediateForestNode);
                            children.AddRange(childList);
                            break;
                        default:
                            children.Add(child);
                            break;
                    }
                }
            }
            return children;
        }

        public void ExtractParamsAndConstruct(List<IForestNode> tokens)
        {      

            if (tokens.Count == 3 || tokens.Count == 5)
            {
                ITokenForestNode tokenId = (ITokenForestNode)tokens[1];
                string idValue = tokenId.Token.Capture.ToString();

                int? levelValue = null;
                if (tokens.Count == 5)
                {
                    ITokenForestNode tokenLevel = (ITokenForestNode)tokens[3];
                    if (int.TryParse(tokenLevel.Token.Capture.ToString(), out int parsedLevel))
                    {
                        levelValue = parsedLevel;
                    }
                    else
                    {
                        throw new InvalidOperationException("Невозможно преобразовать tokenLevel в число.");
                        //выкидавать throw в окно под редактором кода если уровень это не число, или он больше чем уровень модуля
                    }
                }

                _parameters.Add(levelValue.HasValue
                    ? new Parameter(idValue, levelValue.Value)
                    : new Parameter(idValue));
            }

            _name = null;
            _nameValue = null;

        }

        public void Print()
        {

            if (_name == null || _nameValue == null)
                return;
            SortList();
            List<PrefixCouple> prefixes = PrefixParse();
            List<ITokenForestNode> value = ValueParse();

            _nameTable.AddName(_nameType, prefixes,
                IdNameParse().Token.Capture.ToString(), value);
            ClearNameInfo();

        }

        private void ClearNameInfo()
        {
            _name = null;
            _nameValue = null;
        }

        private ITokenForestNode IdNameParse()
        {
            ITokenForestNode prev = default(ITokenForestNode);
            bool save = false;

            foreach (var element in _nameValue)
            {
                if (element.Token.Capture.ToString().Equals("Sort"))
                    save = true;
                if (save)
                {
                    if (element.Token.TokenType.Id.Equals("ID"))
                        return element;
                }
                else
                {
                    if (element.Token.TokenType.Id.Equals("ID"))
                        prev = element;
                    if (element.Token.TokenType.Id.Equals("="))
                        return prev;
                }
            }
            return null;
        }

        private List<PrefixCouple> PrefixParse()
        {
            var prefixes = new List<PrefixCouple>();
            ITokenForestNode from = null;
            ITokenForestNode to = null;
            bool save = false;
            foreach (var element in _nameValue)
            {
                if (element.Token.Capture.ToString().Equals("Sort"))
                    break;
                if (element.Token.TokenType.Id.Equals("ID") && !save)
                        from = element;
                if (element.Token.TokenType.Id.Equals("IN") 
                    && !(from is null))
                {
                    save = true;
                }
                if (element.Token.TokenType.Id.Equals("ID") && save)
                {
                    to = element;
                    prefixes.Add(new PrefixCouple(from, to));
                    save = false;
                }
            }
            return prefixes;
        }

        //private List<ITokenForestNode> ParseParamsAndConstructValue()
        //{

        //}

        private List<ITokenForestNode> ValueParse()
        {
            var value = new List<ITokenForestNode>();
            var save = false;
            foreach (var element in _nameValue)
            {
                if (element.Token.Capture.ToString().Equals(":") ||
                    element.Token.Capture.ToString().Equals("="))
                {
                    save = true;
                    continue;
                }
                if (save)
                {
                    if (element.Token.Capture.ToString().Equals(";"))
                        break;
                    else
                        value.Add(element);
                }
            }
            return value;
        }

        class ListComparer : IComparer<ITokenForestNode>
        {
            public int Compare(ITokenForestNode p1, ITokenForestNode p2)
            {
                if (p1 is null || p2 is null)
                    throw new ArgumentException("Некорректное значение параметра");
                return p1.Origin - p2.Origin;
            }
        }

        private void SortList()
        {
            var array = _nameValue.ToArray();
            Array.Sort(array, new ListComparer());
            _nameValue = new UniqueList<ITokenForestNode>(array);
        }

        private static string GetSymbolNodeString(ISymbolForestNode node)
        {
            return $"({node.Symbol}, {node.Origin}, {node.Location})";
        }

        private static string GetIntermediateNodeString(IIntermediateForestNode node)
        {
            return $"({node.DottedRule}, {node.Origin}, {node.Location})";
        }

        private static string GetTokenNodeString(ITokenForestNode node)
        {
            return $"('{node.Token.Capture}', {node.Origin}, {node.Location})";
        }

    }
}
