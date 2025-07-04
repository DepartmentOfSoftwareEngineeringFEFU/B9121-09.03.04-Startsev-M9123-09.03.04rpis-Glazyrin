﻿using System.Collections.Generic;

namespace Library.Analyzer.Forest
{
    public class AndForestNode : IAndForestNode
    {
        public IReadOnlyList<IForestNode> Children { get { return _children; } }


        private readonly List<IForestNode> _children;

        public AndForestNode()
        {
            _children = new List<IForestNode>();
        }

        public void AddChild(IForestNode orNode)
        {
            _children.Add(orNode);
        }

        public void Accept(IForestNodeVisitor visitor)
        {
            throw new System.NotImplementedException();
        }
    }
}