using System;

namespace WpfCustomUtilities.RecursiveSerializer.Manifest
{
    [Serializable]
    public struct SerializedStreamData
    {
        public long Position { get; set; }
        public long DataSize { get; set; }
        public Type DataType { get; set; }
    }
}
