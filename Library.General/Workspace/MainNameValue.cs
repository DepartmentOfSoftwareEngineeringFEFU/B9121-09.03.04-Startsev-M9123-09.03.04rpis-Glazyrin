using Library.Analyzer.Automata;
using Library.Analyzer.Forest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.General.Workspace
{
    public class MainNameValue : NameValue
    {
        public MainNameValue(List<ITokenForestNode> dfaLexemes) 
            : base(dfaLexemes)
        {
        }

        public UndefinedType GetUndefinedType()
        {
            if (Value.Any(node => node.Token.TokenType.Id == "LOGIC_RELATION_IMPLICATION"))
                return UndefinedType.Function;

            if (Value.Any(node => node.Token.TokenType.Id == "OR" ||
                                 node.Token.TokenType.Id == "REAL_S" ||
                                 node.Token.TokenType.Id == "INT_S"))
                return UndefinedType.Undefined_Sets;

            if (Value.Any(node => node.Token.TokenType.Id == "STRING_S"))
                return UndefinedType.Set_String;

            return UndefinedType.None;
        }

        public override string ToString()
        {
            return string.Join(" ", Value.Select(node => node.Token.Capture.ToString()));
        }
    }
}
