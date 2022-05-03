using System;
using System.Collections.Generic;
using System.Linq;

namespace WpfCustomUtilities.SimpleCollections.Collection
{
    // AVL Binary Search Tree Implementation - https://en.wikipedia.org/wiki/AVL_tree
    //
    // - O(log n) Insert, Delete
    // - Self balancing
    // - O(1) Search using dictionary backup (also to retrieve count)
    // 
    // UPDATE:  https://algs4.cs.princeton.edu/code/edu/princeton/cs/algs4/AVLTreeST.java.html
    //
    //          Must cleaner implementation - uses just a left and right pointer; calculates the
    //          height; and rebalances the tree on each operation without using any additional
    //          recursive lookups (height, or balance factor). Also, has an optional "assert check()"
    //          to check the AVL tree "invariants" (the definition of the tree) after each operation
    //  

    /// <summary>
    /// AVL Binary Search Tree implementation that gets the key from the value node by use of an 
    /// indexer delegate. Also, uses a user supplied comparer to compare two node keys. The keys are
    /// supplied by the indexer from the user.
    /// </summary>
    /// <typeparam name="K">Key type</typeparam>
    /// <typeparam name="T">Node type</typeparam>
    public class BinarySearchTree<K, T>
    {
        // Track values to help visualize when debugging
        SimpleDictionary<K, BinarySearchTreeNode<K, T>> _nodeDict;

        // Root node
        BinarySearchTreeNode<K, T> _root;

        // Comparer
        Comparer<K> _keyComparer;

        public int Count
        {
            get { return _nodeDict.Count; }
        }

        public BinarySearchTree(Comparer<K> comparer)
        {
            _nodeDict = new SimpleDictionary<K, BinarySearchTreeNode<K, T>>();
            _keyComparer = comparer;
        }

        public override string ToString()
        {
            return String.Format("Count={0} Root={1}", _nodeDict.Count, _root);
        }

        public void Clear()
        {
            _nodeDict.Clear();
            _root = null;
        }

        public IEnumerable<T> DumpValues()
        {
            return _nodeDict.Values.Select(x => x.Value);
        }

        public void Insert(K key, T value)
        {
            // Insert value into the tree -> Rebalance the tree
            _root = InsertImpl(_root, key, value);

            // Take the O(n log n) hit to find the node and hash it
            var node = Search(key);

            // Track the values for debugging and a fast retrieval using the key
            _nodeDict.Add(key, node);
        }

        public bool Remove(K key)
        {
            if (!_nodeDict.ContainsKey(key))
                throw new ArgumentException("Trying to remove non-existing key from binary search tree");

            // Remove the specified key -> Rebalance the tree
            _root = RemovalImpl(_root, key);

            // Track the values for debugging
            return _nodeDict.Remove(key);
        }

        /// <summary>
        /// Checks dictionary to see if key exists in the tree
        /// </summary>
        public bool ContainsKey(K key)
        {
            return _nodeDict.ContainsKey(key);
        }

        /// <summary>
        /// Returns hash-code backed lookup of the value from the key
        /// </summary>
        public BinarySearchTreeNode<K, T> Get(K key)
        {
            // Utilize dictionary for O(1) lookup
            if (_nodeDict.ContainsKey(key))
                return _nodeDict[key];

            else
                throw new Exception("Trying to retrieve hash-backed node from BST without checking");
        }

        public BinarySearchTreeNode<K, T> Search(K key)
        {
            // Utilize dictionary for O(1) lookup
            if (_nodeDict.ContainsKey(key))
                return _nodeDict[key];

            return SearchImpl(key, _root);
        }

        public BinarySearchTreeNode<K, T> Successor(K searchKey)
        {
            if (!_nodeDict.ContainsKey(searchKey))
                return null;

            return SuccessorImpl(searchKey, _root, null);
        }

        public BinarySearchTreeNode<K, T> Predecessor(K searchKey)
        {
            if (!_nodeDict.ContainsKey(searchKey))
                return null;

            return PredecessorImpl(searchKey, _root, null);
        }

        public T Min()
        {
            var result = MinImpl(_root);

            if (result == null)
                return default(T);

            else
                return result.Value;
        }
        public K MinKey()
        {
            var minNode = MinImpl(_root);

            if (minNode == null)
                throw new Exception("Trying to resolve min key from an empty Binary Search Tree");

            else
                return minNode.Key;
        }

        public T Max()
        {
            var result = MaxImpl(_root);

            if (result == null)
                return default(T);

            else
                return result.Value;
        }
        public K MaxKey()
        {
            var maxNode = MaxImpl(_root);

            if (maxNode == null)
                throw new Exception("Trying to resolve max key from an empty Binary Search Tree");

            else
                return maxNode.Key;
        }

        /// <summary>
        /// Performs insertion of the node recursively. Retraces to update balance factors and re-balance
        /// the tree.
        /// </summary>
        private BinarySearchTreeNode<K, T> InsertImpl(BinarySearchTreeNode<K, T> node, K key, T value)
        {
            if (node == null)
                return new BinarySearchTreeNode<K, T>(key, value);

            var comparison = _keyComparer.Compare(key, node.Key);

            // Insert Left
            if (comparison < 0)
                node.LeftChild = InsertImpl(node.LeftChild, key, value);

            // Insert Right
            else if (comparison > 0)
                node.RightChild = InsertImpl(node.RightChild, key, value);

            else
                throw new Exception("Duplicate key insertion BinarySearchTree");

            // Set the height
            node.Height = System.Math.Max(node.LeftChild?.Height ?? -1, node.RightChild?.Height ?? -1) + 1;

            return Balance(node);
        }

        /// <summary>
        /// Removes node from the tree using next-successor replacement (see procedure). Also, sets up balance factors.
        /// </summary>
        private BinarySearchTreeNode<K, T> RemovalImpl(BinarySearchTreeNode<K, T> node, K key)
        {
            // Procedure:  https://algs4.cs.princeton.edu/code/edu/princeton/cs/algs4/AVLTreeST.java.html
            //

            var comparison = _keyComparer.Compare(key, node.Key);

            if (comparison < 0)
                node.LeftChild = RemovalImpl(node.LeftChild, key);

            else if (comparison > 0)
                node.RightChild = RemovalImpl(node.RightChild, key);

            else
            {
                // One child case
                if (node.LeftChild == null)
                    return node.RightChild;

                else if (node.RightChild == null)
                    return node.LeftChild;

                // Next successor case/
                else
                {
                    var temp = node;
                    node = MinImpl(temp.RightChild);
                    node.RightChild = DeleteMin(temp.RightChild);
                    node.LeftChild = temp.LeftChild;
                }
            }

            node.Height = System.Math.Max(node.LeftChild?.Height ?? -1, node.RightChild?.Height ?? -1) + 1;

            return Balance(node);
        }

        /// <summary>
        /// Deletes the smallest entry from the subtree
        /// </summary>
        private BinarySearchTreeNode<K, T> DeleteMin(BinarySearchTreeNode<K, T> node)
        {
            if (node.LeftChild == null)
                return node.RightChild;

            node.LeftChild = DeleteMin(node.LeftChild);
            node.Height = System.Math.Max(node.LeftChild?.Height ?? -1, node.RightChild?.Height ?? -1) + 1;

            return Balance(node);
        }

        /// <summary>
        /// Returns the lowest value node in the tree
        /// </summary>
        private BinarySearchTreeNode<K, T> MinImpl(BinarySearchTreeNode<K, T> node)
        {
            if (node == null)
                return null;

            return MinImpl(node.LeftChild) ?? node;
        }

        /// <summary>
        /// Returns the greatest value node in the tree
        /// </summary>
        private BinarySearchTreeNode<K, T> MaxImpl(BinarySearchTreeNode<K, T> node)
        {
            if (node == null)
                return null;

            return MaxImpl(node.RightChild) ?? node;
        }

        /// <summary>
        /// Returns the node closest to the provided key
        /// </summary>
        private BinarySearchTreeNode<K, T> SearchImpl(K key, BinarySearchTreeNode<K, T> node)
        {
            if (node == null)
                return null;

            var comparison = _keyComparer.Compare(key, node.Key);

            if (comparison < 0 && node.LeftChild != null)
                return SearchImpl(key, node.LeftChild);

            else if (comparison > 0 && node.RightChild != null)
                return SearchImpl(key, node.RightChild);

            else
                return node;
        }

        /// <summary>
        /// Returns the next successor of the provided key's node
        /// </summary>
        private BinarySearchTreeNode<K, T> SuccessorImpl(K key, BinarySearchTreeNode<K, T> node, BinarySearchTreeNode<K, T> savedParent)
        {
            // Recursively look for the search key's node - keeping track of the last parent with a LEFT child. This 
            // will be the successor if the final search node has no left child.
            //
            // At the final leaf node - continue looking for the MAX of it's RIGHT sub-tree
            //

            if (node == null)
                return null;

            var comparison = _keyComparer.Compare(key, node.Key);

            if (comparison < 0 && node.LeftChild != null)
            {
                // Keep track of this last parent
                savedParent = node;

                return SuccessorImpl(key, node.LeftChild, savedParent);
            }

            else if (comparison > 0 && node.RightChild != null)
                return SuccessorImpl(key, node.RightChild, savedParent);

            // FOUND NODE!
            else
            {
                if (node.RightChild != null)
                    return MinImpl(node.RightChild);

                else
                    return savedParent;
            }
        }

        /// <summary>
        /// Returns the predecessor of the search key's node
        /// </summary>
        private BinarySearchTreeNode<K, T> PredecessorImpl(K key, BinarySearchTreeNode<K, T> node, BinarySearchTreeNode<K, T> savedParent)
        {
            // Recursively look for the search key's node - keeping track of the last parent with a RIGHT child. This 
            // will be the predecessor if the final search node has no right child.
            //
            // At the final leaf node - continue looking for the MIN of it's LEFT sub-tree
            //

            if (node == null)
                return null;

            var comparison = _keyComparer.Compare(key, node.Key);

            if (comparison < 0 && node.LeftChild != null)
                return PredecessorImpl(key, node.LeftChild, savedParent);

            else if (comparison > 0 && node.RightChild != null)
            {
                // Keep track of this last parent
                savedParent = node;

                return PredecessorImpl(key, node.RightChild, savedParent);
            }

            // FOUND NODE!
            else
            {
                if (node.LeftChild != null)
                    return MaxImpl(node.LeftChild);

                else
                    return savedParent;
            }
        }

        /// <summary>
        /// Peforms checking of balance factors and calls the appropriate rotate method
        /// </summary>
        private BinarySearchTreeNode<K, T> Balance(BinarySearchTreeNode<K, T> node)
        {
            // Procedure - https://en.wikipedia.org/wiki/AVL_tree
            //
            // Examine situation with heavy node and direct child:
            //
            // Let: subTree = X, and nextNode = Z. (Next node would be left or right depending on BalanceFactor(Z))

            /* 
 	            Right Right  => Z is a right child of its parent X and Z is  not left-heavy 	(i.e. BalanceFactor(Z) ≥ 0)  (Rotate Left)
	            Left  Left 	 => Z is a left  child of its parent X and Z is  not right-heavy    (i.e. BalanceFactor(Z) ≤ 0)  (Rotate Right)
	            Right Left 	 => Z is a right child of its parent X and Z is  left-heavy 	    (i.e. BalanceFactor(Z) = −1) (Double Rotate RightLeft)
	            Left  Right  => Z is a left  child of its parent X and Z is  right-heavy 	    (i.e. BalanceFactor(Z) = +1) (Double Rotate LeftRight)
            */

            // Update:  https://algs4.cs.princeton.edu/code/edu/princeton/cs/algs4/AVLTreeST.java.html

            if (node.BalanceFactor < -1)
            {
                // Left Right
                if (node.RightChild.BalanceFactor > 0)
                    node.RightChild = RotateRight(node.RightChild);

                node = RotateLeft(node);
            }
            else if (node.BalanceFactor > 1)
            {
                // Right Left
                if (node.LeftChild.BalanceFactor < 0)
                    node.LeftChild = RotateLeft(node.LeftChild);

                node = RotateRight(node);
            }

            return node;
        }

        /// <summary>
        /// Performs a left rotation on the sub-tree and returns the new root
        /// </summary>
        private BinarySearchTreeNode<K, T> RotateLeft(BinarySearchTreeNode<K, T> subTree)
        {
            // Procedure - https://en.wikipedia.org/wiki/AVL_tree
            //
            // - Pre-condition:  Node must have a right-child and a balance factor of -2
            // - Node's parent becomes node's right child
            // - Node's right child become node's right child's left child
            // - Set new balance factors (see Wikipedia)

            //if (subTree.BalanceFactor != -2)
            //    throw new Exception("Invalid RotateLeft pre-condition BinarySearchTree");

            //if (subTree.RightChild == null)
            //    throw new Exception("Invalid RotateLeft pre-condition BinarySearchTree");

            // Refering to variables from Wikipedia entry
            var X = subTree;
            var Z = subTree.RightChild;
            var T = subTree.RightChild.LeftChild ?? null;

            // Node's left child becomes node's parent's right child:  X -> T
            X.RightChild = T;

            // Node's parent becomes the left child of node:  X <- Z
            Z.LeftChild = X;

            // Set up height of nodes
            X.Height = System.Math.Max(X.LeftChild?.Height ?? -1, X.RightChild?.Height ?? -1) + 1;
            Z.Height = System.Math.Max(Z.LeftChild?.Height ?? -1, Z.RightChild?.Height ?? -1) + 1;

            // Return node of the new sub-tree
            return Z;
        }

        /// <summary>
        /// Performs a right rotation on the sub-tree and returns the new root
        /// </summary>
        private BinarySearchTreeNode<K, T> RotateRight(BinarySearchTreeNode<K, T> node)
        {
            // Procedure - https://en.wikipedia.org/wiki/AVL_tree
            //
            // - Pre-condition:  Node must have a left-child and a balance factor of +2
            // - Node's parent becomes node's left child
            // - Node's left child become node's left child's right child
            // - Set new height

            //if (node.BalanceFactor != 2)
            //    throw new Exception("Invalid RotateLeft pre-condition BinarySearchTree");

            //if (node.LeftChild == null)
            //    throw new Exception("Invalid RotateLeft pre-condition BinarySearchTree");

            // Refering to variables from Wikipedia entry
            var X = node;
            var Z = node.LeftChild;
            var T = node.LeftChild.RightChild ?? null;

            // Node's right child becomes node's parent's left child:  T <- X
            X.LeftChild = T;

            // Node's parent becomes the right child of node:  Z -> X
            Z.RightChild = X;

            // Set up height of nodes
            X.Height = System.Math.Max(X.LeftChild?.Height ?? -1, X.RightChild?.Height ?? -1) + 1;
            Z.Height = System.Math.Max(Z.LeftChild?.Height ?? -1, Z.RightChild?.Height ?? -1) + 1;

            // Return node of the new sub-tree
            return Z;
        }
    }
}
