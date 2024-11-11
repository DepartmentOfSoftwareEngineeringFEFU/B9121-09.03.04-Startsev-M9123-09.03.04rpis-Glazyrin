namespace Library.Analyzer.Forest
{
    public interface IForestNode : IForestNodeVisitable
    {

        //источник
        int Origin { get; }

        //
        int Location { get; }

        ForestNodeType NodeType { get; }
    }
}