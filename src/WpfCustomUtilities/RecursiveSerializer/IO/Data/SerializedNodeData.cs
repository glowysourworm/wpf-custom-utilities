using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Data
{
    internal struct SerializedNodeData
    {
        internal static SerializedNodeData Empty = new SerializedNodeData()
        {
            NodeType = default(SerializedNodeType),
            Mode = default(SerializationMode),
            TypeHashCode = default(int),
            CollectionCount = default(int),
            CollectionElementTypeHashCode = default(int),
            CollectionInterfaceType = default(CollectionInterfaceType),
            ObjectId = default(int),
            PrimitiveValue = default(object),
            ReferenceId = default(int)
        };

        // ALL NODES
        internal SerializedNodeType NodeType { get; set; }
        internal SerializationMode Mode { get; set; }
        internal int TypeHashCode { get; set; }

        // PRIMITIVE
        internal object PrimitiveValue { get; set; }

        // OBJECT
        internal int ObjectId { get; set; }

        // REFERENCE
        internal int ReferenceId { get; set; }

        // COLLECTION
        internal CollectionInterfaceType CollectionInterfaceType { get; set; }
        internal int CollectionCount { get; set; }
        internal int CollectionElementTypeHashCode { get; set; }

        public override bool Equals(object obj)
        {
            SerializedNodeData nodeData = (SerializedNodeData)obj;

            return this.CollectionCount == nodeData.CollectionCount &&
                   this.CollectionElementTypeHashCode == nodeData.CollectionElementTypeHashCode &&
                   this.CollectionInterfaceType == nodeData.CollectionInterfaceType &&
                   this.ReferenceId == nodeData.ReferenceId &&
                   this.ObjectId == nodeData.ObjectId &&
                   ComparePrimitives(this.PrimitiveValue, nodeData.PrimitiveValue) &&
                   this.TypeHashCode == nodeData.TypeHashCode &&
                   this.Mode == nodeData.Mode &&
                   this.NodeType == nodeData.NodeType;
        }

        private bool ComparePrimitives(object value1, object value2)
        {
            if (ReferenceEquals(value1, value2))
                return true;

            else if (ReferenceEquals(value1, null))
                return false;

            else if (ReferenceEquals(value2, null))
                return false;

            return value1.Equals(value2);
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.CollectionCount,
                                                                     this.CollectionElementTypeHashCode,
                                                                     this.CollectionInterfaceType,
                                                                     this.Mode,
                                                                     this.NodeType,
                                                                     this.ObjectId,
                                                                     this.PrimitiveValue,
                                                                     this.ReferenceId,
                                                                     this.TypeHashCode);
        }
    }
}
