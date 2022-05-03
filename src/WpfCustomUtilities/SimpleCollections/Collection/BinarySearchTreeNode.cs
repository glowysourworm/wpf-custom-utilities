namespace WpfCustomUtilities.SimpleCollections.Collection
{
    public class BinarySearchTreeNode<K, T>
    {
        public BinarySearchTreeNode<K, T> LeftChild { get; set; }
        public BinarySearchTreeNode<K, T> RightChild { get; set; }
        public K Key { get; set; }
        public T Value { get; set; }
        public int Height { get; set; }
        public int BalanceFactor
        {
            get { return (this.LeftChild?.Height ?? -1) - (this.RightChild?.Height ?? -1); }
        }

        public BinarySearchTreeNode(K key, T value)
        {
            this.Key = key;
            this.Value = value;
        }

        public override string ToString()
        {
            return string.Format("(Key={0}, BF={1}) -> Left:  {2}, Right: {3}",
                                    this.Key,
                                    this.BalanceFactor,
                                    this.LeftChild?.Key?.ToString() ?? "null",
                                    this.RightChild?.Key?.ToString() ?? "null");
        }
    }
}
