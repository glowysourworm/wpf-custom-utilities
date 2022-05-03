using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.IO.Interface;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Streaming
{
    class NodeSerializer
    {
        readonly ISerializationStreamWriter _writer;

        internal NodeSerializer(ISerializationStreamWriter writer)
        {
            _writer = writer;
        }

        internal void Serialize(Queue<SerializedNodeData> nodeData)
        {
            // WRITE COUNT TO STREAM
            _writer.Write<int>(nodeData.Count);

            while (nodeData.Count > 0)
            {
                WriteNext(nodeData.Dequeue());
            }
        }

        private void WriteNext(SerializedNodeData data)
        {
            // FORMAT
            //
            // Serialize:  [ Null Primitive = 0, Serialization Mode, Hashed Type Code ]
            // Serialize:  [ Null = 1,           Serialization Mode, Hashed Type Code ]
            // Serialize:  [ Primitive = 2,      Serialization Mode, Hashed Type Code, Primitive Value ]
            // Serialize:  [ Object = 3,         Serialization Mode, Hashed Type Code, Object Id ] (Recruse Sub - graph)
            // Serialize:  [ Reference = 4,      Serialization Mode, Hashed Type Code, Reference Object Id ]
            // Serialize:  [ Collection = 5,     Serialization Mode, Hashed Type Code, Object Id,
            //                                   Collection Interface Type,
            //                                   Child Count,
            //                                   Child Hash Type Code ] (loop) Children (Recruse Sub-graphs)

            _writer.Write<SerializedNodeType>(data.NodeType);
            _writer.Write<SerializationMode>(data.Mode);
            _writer.Write<int>(data.TypeHashCode);

            switch (data.NodeType)
            {
                case SerializedNodeType.NullPrimitive:
                case SerializedNodeType.Null:
                    break;
                case SerializedNodeType.Primitive:
                {
                    // READ PRIMITIVE VALUE FROM STREAM
                    _writer.Write(data.PrimitiveValue, data.PrimitiveValue.GetType());
                    break;
                }
                case SerializedNodeType.Object:
                {
                    // READ OBJECT ID FROM STREAM
                    _writer.Write<int>(data.ObjectId);
                    break;
                }

                case SerializedNodeType.Reference:
                {
                    // READ OBJECT ID FROM STREAM
                    _writer.Write<int>(data.ReferenceId);
                    break;
                }
                case SerializedNodeType.Collection:
                {
                    // READ OBJECT ID FROM STREAM
                    _writer.Write<int>(data.ObjectId);

                    // READ INTERFACE TYPE
                    _writer.Write<CollectionInterfaceType>(data.CollectionInterfaceType);

                    // READ CHILD COUNT
                    _writer.Write<int>(data.CollectionCount);

                    // ELEMENT HASH TYPE CODE
                    _writer.Write<int>(data.CollectionElementTypeHashCode);
                    break;
                }
                default:
                    throw new Exception("Unhandled SerializedNodeType:  NodeDeserializer.ReadNext");
            }
        }
    }
}
