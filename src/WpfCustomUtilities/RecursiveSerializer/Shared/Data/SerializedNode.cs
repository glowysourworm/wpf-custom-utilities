namespace WpfCustomUtilities.RecursiveSerializer.Shared.Data
{
    /// <summary>
    /// Data from an individual serialized node
    /// </summary>
    public struct SerializedNode
    {
        // ALL NODES
        public byte NodeType { get; set; }
        public byte Mode { get; set; }
        public int TypeHashCode { get; set; }

        // PRIMITIVE
        public object PrimitiveValue { get; set; }

        // OBJECT
        public int ObjectId { get; set; }

        // REFERENCE
        public int ReferenceId { get; set; }

        // COLLECTION
        public byte CollectionInterfaceType { get; set; }
        public int CollectionCount { get; set; }
        public int CollectionElementTypeHashCode { get; set; }
    }
}
