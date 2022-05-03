namespace WpfCustomUtilities.SimpleCollections.Graph.Interface
{
    /// <summary>
    /// Interface for working with graph algorithms based on abstract graphs of connected nodes
    /// </summary>
    public interface IGraphEdge<T> where T : IGraphNode
    {
        T Node { get; }
        T AdjacentNode { get; }

        double Weight { get; }
    }
}
