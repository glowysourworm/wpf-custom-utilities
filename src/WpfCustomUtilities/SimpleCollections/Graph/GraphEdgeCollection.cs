using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Interface;
using WpfCustomUtilities.SimpleCollections.Collection;
using WpfCustomUtilities.SimpleCollections.Collection.Interface;
using WpfCustomUtilities.SimpleCollections.Graph.Interface;

namespace WpfCustomUtilities.SimpleCollections.Graph
{
    /// <summary>
    /// This should be the primary collection for creating an IGraph. Gives fast lookup of edges and nodes. Also, supports
    /// directed (or) un-directed graph lookup and duplicate edge validation.
    /// </summary>
    [Serializable]
    public class GraphEdgeCollection<TNode, TEdge> : IRecursiveSerializable where TNode : IGraphNode, INotifyDictionaryKey
                                                                            where TEdge : IGraphEdge<TNode>
    {
        // Edge hash table
        SimpleDictionary<string, TEdge> _edges;

        // Node hash table
        SimpleDictionary<int, TNode> _nodes;

        // Node adjacent edges
        SimpleDictionary<int, SimpleDictionary<string, TEdge>> _nodeEdges;

        public int Count { get { return _edges.Count; } }
        public int NodeCount { get { return _nodeEdges.Keys.Count; } }

        public void Add(TEdge edge)
        {
            if (edge.Node.Equals(edge.AdjacentNode))
                throw new Exception("Trying to add self-referencing edge to a graph:  GraphEdgeCollection.Add");

            var edgeHashCode = CreateDirectionalHashCode(edge.Node, edge.AdjacentNode);
            var edgeReverseHashCode = CreateDirectionalHashCode(edge.Node, edge.AdjacentNode);

            if (_edges.ContainsKey(edgeHashCode) ||
                _edges.ContainsKey(edgeReverseHashCode))
                throw new Exception("Trying to insert duplicate edge key:  GraphEdgeCollection.cs");

            // Hook
            HookListeners(edge.Node);
            HookListeners(edge.AdjacentNode);

            _edges.Add(edgeHashCode, edge);

            // Nodes
            if (!_nodes.ContainsKey(edge.Node.GetHashCode()))
                _nodes.Add(edge.Node.GetHashCode(), edge.Node);

            if (!_nodes.ContainsKey(edge.AdjacentNode.GetHashCode()))
                _nodes.Add(edge.AdjacentNode.GetHashCode(), edge.AdjacentNode);

            // Forward entry
            if (_nodeEdges.ContainsKey(edge.Node.GetHashCode()))
                _nodeEdges[edge.Node.GetHashCode()].Add(edgeHashCode, edge);

            else
                _nodeEdges.Add(edge.Node.GetHashCode(), new SimpleDictionary<string, TEdge>() { { edgeHashCode, edge } });

            // Reverse entry - will not fail spatial index
            if (_nodeEdges.ContainsKey(edge.AdjacentNode.GetHashCode()))
                _nodeEdges[edge.AdjacentNode.GetHashCode()].Add(edgeReverseHashCode, edge);

            else
                _nodeEdges.Add(edge.AdjacentNode.GetHashCode(), new SimpleDictionary<string, TEdge>() { { edgeReverseHashCode, edge } });
        }

        public void AddNode(TNode node)
        {
            if (!_nodes.ContainsKey(node.GetHashCode()))
                _nodes.Add(node.GetHashCode(), node);

            if (!_nodeEdges.ContainsKey(node.GetHashCode()))
                _nodeEdges.Add(node.GetHashCode(), new SimpleDictionary<string, TEdge>());

            // HOOK
            HookListeners(node);
        }

        public void Remove(TEdge edge)
        {
            var edgeHashCode = CreateDirectionalHashCode(edge.Node, edge.AdjacentNode);
            var edgeReverseHashCode = CreateDirectionalHashCode(edge.Node, edge.AdjacentNode);

            _edges.Remove(edgeHashCode);

            // UNHOOK
            UnhookListeners(edge.Node);
            UnhookListeners(edge.AdjacentNode);

            _nodes.Remove(edge.Node.GetHashCode());
            _nodes.Remove(edge.AdjacentNode.GetHashCode());

            // Forward entry
            if (_nodeEdges.ContainsKey(edge.Node.GetHashCode()))
            {
                if (_nodeEdges[edge.Node.GetHashCode()].ContainsKey(edgeHashCode))
                    _nodeEdges[edge.Node.GetHashCode()].Remove(edgeHashCode);
            }

            // Reverse entry - will not fail spatial index
            if (_nodeEdges.ContainsKey(edge.AdjacentNode.GetHashCode()))
            {
                if (_nodeEdges[edge.AdjacentNode.GetHashCode()].ContainsKey(edgeReverseHashCode))
                    _nodeEdges[edge.AdjacentNode.GetHashCode()].Remove(edgeReverseHashCode);
            }
        }

        public bool Contains(TEdge edge)
        {
            // CHECK THE FORWARD DIRECTION ONLY
            var edgeHashCode = CreateDirectionalHashCode(edge.Node, edge.AdjacentNode);

            return _edges.ContainsKey(edgeHashCode);
        }

        /// <summary>
        /// Tries to locate an edge with the given nodes. Throws an exception if one can't be located
        /// </summary>
        public TEdge Find(TNode node1, TNode node2)
        {
            var edgeHashCode = CreateDirectionalHashCode(node1, node2);
            var edgeReverseHashCode = CreateDirectionalHashCode(node1, node2);

            if (_edges.ContainsKey(edgeHashCode))
                return _edges[edgeHashCode];

            else if (_edges.ContainsKey(edgeReverseHashCode))
                return _edges[edgeReverseHashCode];

            else
                throw new FormattedException("Unable to find edge for nodes {0} and {1}", node1, node2);
        }

        public bool HasEdge(TNode node1, TNode node2)
        {
            var edgeHashCode = CreateDirectionalHashCode(node1, node2);
            var edgeReverseHashCode = CreateDirectionalHashCode(node1, node2);

            return _edges.ContainsKey(edgeHashCode) || _edges.ContainsKey(edgeReverseHashCode);
        }

        public bool HasNode(TNode node)
        {
            return _nodeEdges.ContainsKey(node.GetHashCode());
        }

        public IEnumerable<TEdge> GetAdjacentEdges(TNode node)
        {
            if (!_nodeEdges.ContainsKey(node.GetHashCode()))
                return Array.Empty<TEdge>();

            return _nodeEdges[node.GetHashCode()].Values;
        }

        public IEnumerable<TEdge> GetEdges()
        {
            return _edges.Values;
        }

        public IEnumerable<TNode> GetNodes()
        {
            return _nodes.Values;
        }

        public void Clear()
        {
            ClearEdges();
            ClearNodes();
        }

        public void ClearEdges()
        {
            foreach (var edge in _edges.Values)
            {
                UnhookListeners(edge.Node);
                UnhookListeners(edge.AdjacentNode);
            }

            _edges.Clear();
        }

        public void ClearNodes()
        {
            foreach (var node in _nodes.Values)
            {
                UnhookListeners(node);
            }

            _nodes.Clear();
            _nodeEdges.Clear();
        }

        public GraphEdgeCollection(IEnumerable<TEdge> edges)
        {
            Initialize(Array.Empty<TNode>(), edges);
        }
        public GraphEdgeCollection(IEnumerable<TNode> nodes, IEnumerable<TEdge> edges)
        {
            Initialize(nodes, edges);
        }
        public GraphEdgeCollection(IPropertyReader reader)
        {
            var nodes = reader.Read<List<TNode>>("Nodes");
            var edges = reader.Read<List<TEdge>>("Edges");

            Initialize(nodes, edges);
        }
        ~GraphEdgeCollection()
        {
            Clear();
        }
        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("Nodes", _nodes.Values.ToList());
            writer.Write("Edges", _edges.Values.ToList());
        }

        private void Initialize(IEnumerable<TNode> nodes, IEnumerable<TEdge> edges)
        {
            _edges = new SimpleDictionary<string, TEdge>();
            _nodeEdges = new SimpleDictionary<int, SimpleDictionary<string, TEdge>>();
            _nodes = new SimpleDictionary<int, TNode>();

            // Initialize for edges and adjacent vertex lookup
            foreach (var edge in edges)
                Add(edge);

            // Look for any nodes not yet in the collections - add these
            foreach (var node in nodes)
            {
                if (!HasNode(node))
                    AddNode(node);
            }
        }

        private void HookListeners(TNode node)
        {
            node.HashCodeChangedEvent -= OnNodeHashCodeChanged;
            node.HashCodeChangedEvent += OnNodeHashCodeChanged;
        }

        private void UnhookListeners(TNode node)
        {
            node.HashCodeChangedEvent -= OnNodeHashCodeChanged;
        }

        private void OnNodeHashCodeChanged(INotifyDictionaryKey sender, int oldHashCode, int newHashCode)
        {
            var node = (TNode)sender;

            // Update collections with new hash code
            //
            if (!_nodeEdges.ContainsKey(oldHashCode) || !_nodes.ContainsKey(oldHashCode))
                throw new Exception("Improper listener for INotifyDictionaryKey:  GraphEdgeCollection.cs");

            // Get adjacent edges to update
            var edges = _nodeEdges[oldHashCode];

            // Update node adjacency collection
            _nodeEdges.Remove(oldHashCode);
            _nodeEdges.Add(newHashCode, edges);

            // Update nodes
            _nodes.Remove(newHashCode);
            _nodes.Add(newHashCode, node);

            // Update:  For each edge, check both orientations for the one in the edge collection. Update
            //          this hash code
            //
            foreach (var edge in edges.Values)
            {
                // Node -> Adjacent Node
                var forwardHash1 = CreateDirectionalHashCode(oldHashCode, edge.AdjacentNode.GetHashCode());
                var forwardHash2 = CreateDirectionalHashCode(edge.Node.GetHashCode(), oldHashCode);

                // Adjacent Node -> Node
                var reverseHash1 = CreateDirectionalHashCode(oldHashCode, edge.Node.GetHashCode());
                var reverseHash2 = CreateDirectionalHashCode(edge.AdjacentNode.GetHashCode(), oldHashCode);

                if (_edges.ContainsKey(forwardHash1))
                {
                    var hashCode = CreateDirectionalHashCode(newHashCode, edge.AdjacentNode.GetHashCode());

                    _edges.Remove(forwardHash1);
                    _edges.Add(hashCode, edge);
                }

                if (_edges.ContainsKey(forwardHash2))
                {
                    var hashCode = CreateDirectionalHashCode(edge.Node.GetHashCode(), newHashCode);

                    _edges.Remove(forwardHash2);
                    _edges.Add(hashCode, edge);
                }

                if (_edges.ContainsKey(reverseHash1))
                {
                    var hashCode = CreateDirectionalHashCode(newHashCode, edge.Node.GetHashCode());

                    _edges.Remove(reverseHash1);
                    _edges.Add(hashCode, edge);
                }

                if (_edges.ContainsKey(reverseHash2))
                {
                    var hashCode = CreateDirectionalHashCode(edge.AdjacentNode.GetHashCode(), newHashCode);

                    _edges.Remove(reverseHash2);
                    _edges.Add(hashCode, edge);
                }
            }
        }

        private string CreateDirectionalHashCode(TNode node1, TNode node2)
        {
            var node1Hash = node1.GetHashCode();
            var node2Hash = node2.GetHashCode();

            return node1Hash.ToString() + "|" + node2Hash.ToString();
        }

        private string CreateDirectionalHashCode(int oldHashCode1, int oldHashCode2)
        {
            return oldHashCode1.ToString() + "|" + oldHashCode2.ToString();
        }

        private void ParseDirectionalHashCode(string edgeHashCode, ref int node1HashCode, ref int node2HashCode)
        {
            var pieces = edgeHashCode.Split('|');

            node1HashCode = int.Parse(pieces[0]);
            node2HashCode = int.Parse(pieces[1]);
        }
    }
}
