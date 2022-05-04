using System;

using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.SimpleCollections.Graph.Interface;

namespace WpfCustomUtilities.SimpleCollections.Graph
{
    [Serializable]
    public class GraphEdge<TNode> : IGraphEdge<TNode> where TNode : IGraphNode
    {
        public TNode Node { get; private set; }
        public TNode AdjacentNode { get; private set; }

        public double Weight { get; private set; }

        public GraphEdge(TNode node, TNode adjacentNode, GraphDistanceDelegate distanceDelegate)
        {
            this.Node = node;
            this.AdjacentNode = adjacentNode;
            this.Weight = distanceDelegate(node, adjacentNode);
        }

        public bool IsEquivalent(GraphEdge<TNode> edge)
        {
            return (this.Node.Equals(edge.Node) &&
                    this.AdjacentNode.Equals(edge.AdjacentNode)) ||
                   (this.Node.Equals(edge.AdjacentNode) &&
                    this.AdjacentNode.Equals(edge.Node));
        }

        /// <summary>
        /// Returns true if the edge is equivalent to the edge specified by two vertices IN EITHER DIRECTION
        /// </summary>
        public bool IsEquivalent(TNode vertex1, TNode vertex2)
        {
            return (this.Node.Equals(vertex1) &&
                    this.AdjacentNode.Equals(vertex2)) ||
                   (this.Node.Equals(vertex2) &&
                    this.AdjacentNode.Equals(vertex1));
        }
        public static bool operator ==(GraphEdge<TNode> graphEdge1, GraphEdge<TNode> graphEdge2)
        {
            if (ReferenceEquals(graphEdge1, null))
                return ReferenceEquals(graphEdge2, null);

            else if (ReferenceEquals(graphEdge2, null))
                return false;

            else
                return graphEdge1.Equals(graphEdge2);
        }
        public static bool operator !=(GraphEdge<TNode> graphEdge1, GraphEdge<TNode> graphEdge2)
        {
            return !(graphEdge1 == graphEdge2);
        }
        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.Node, this.AdjacentNode);
        }

        public override string ToString()
        {
            return "(" + this.Node.ToString() + ") -> (" + this.AdjacentNode.ToString() + ")";
        }
    }
}
