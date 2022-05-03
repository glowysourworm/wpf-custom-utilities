using System.Collections.Generic;

using WpfCustomUtilities.SimpleCollections.Collection.Interface;
using WpfCustomUtilities.SimpleCollections.Graph.Interface;

namespace WpfCustomUtilities.SimpleCollections.Graph
{
    public class Graph<TNode> : IGraph<TNode, GraphEdge<TNode>> where TNode : class, INotifyDictionaryKey, IGraphNode
    {
        // Primary graph collection
        GraphEdgeCollection<TNode, GraphEdge<TNode>> _collection;

        public IEnumerable<TNode> Nodes
        {
            get
            {
                return _collection.GetNodes();
            }
        }

        public IEnumerable<GraphEdge<TNode>> Edges
        {
            get { return _collection.GetEdges(); }
        }

        public Graph(IEnumerable<TNode> nodes, IEnumerable<GraphEdge<TNode>> edges)
        {
            _collection = new GraphEdgeCollection<TNode, GraphEdge<TNode>>(nodes, edges);
        }

        public void AddEdge(GraphEdge<TNode> edge)
        {
            // Detects duplicate edges
            _collection.Add(edge);
        }

        /// <summary>
        /// Removes an existing edge in favor of a new one
        /// </summary>
        public void Modify(GraphEdge<TNode> existingEdge, GraphEdge<TNode> newEdge)
        {
            _collection.Remove(existingEdge);
            _collection.Add(newEdge);
        }

        public IEnumerable<GraphEdge<TNode>> GetAdjacentEdges(TNode node)
        {
            return _collection.GetAdjacentEdges(node);
        }

        public GraphEdge<TNode> FindEdge(TNode node1, TNode node2)
        {
            return _collection.Find(node1, node2);
        }
    }
}
