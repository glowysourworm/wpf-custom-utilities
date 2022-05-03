using System.Collections.Generic;

using WpfCustomUtilities.SimpleCollections.Graph.Interface;

namespace WpfCustomUtilities.SimpleCollections.Graph
{
    /// <summary>
    /// Component for planning a route between graph vertices
    /// </summary>
    public class GraphTraversal<TNode, TEdge> where TNode : IGraphNode
                                              where TEdge : IGraphEdge<TNode>
    {
        public TNode Source { get; private set; }
        public TNode Destination { get; private set; }
        public IEnumerable<TEdge> Route { get; private set; }

        public GraphTraversal(TNode source, TNode destination, IEnumerable<TEdge> route)
        {
            this.Source = source;
            this.Destination = destination;
            this.Route = route;
        }
    }
}
