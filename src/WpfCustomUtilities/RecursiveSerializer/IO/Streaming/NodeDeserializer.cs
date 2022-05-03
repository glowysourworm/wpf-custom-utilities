using RecursiveSerializer.Formatter;

using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.IO.Interface;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Streaming
{
    internal class NodeDeserializer
    {
        readonly ISerializationStreamReader _reader;
        readonly DeserializedHeader _header;

        internal NodeDeserializer(ISerializationStreamReader reader, DeserializedHeader header)
        {
            _reader = reader;
            _header = header;
        }

        internal Queue<SerializedNodeData> Deserialize()
        {
            var result = new Queue<SerializedNodeData>();

            // READ COUNT TO STREAM
            var count = _reader.Read<int>();

            for (int index = 0; index < count; index++)
            {
                var nodeData = ReadNext();

                // Verify node type
                if (!_header.Data.TypeTable.ContainsKey(nodeData.TypeHashCode))
                    throw new Exception("Missing type definition for node!!");

                result.Enqueue(nodeData);
            }

            return result;
        }

        private SerializedNodeData ReadNext()
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

            var nextNode = _reader.Read<SerializedNodeType>();
            var nextMode = _reader.Read<SerializationMode>();
            var nextTypeHash = _reader.Read<int>();

            // PRIMITIVE
            object primitiveValue = default(object);

            // OBJECT
            int objectId = default(int);

            // REFERENCE
            int referenceId = default(int);

            // Collection
            CollectionInterfaceType interfaceType = CollectionInterfaceType.IList;
            int childCount = default(int);
            int elementTypeHashCode = default(int);

            switch (nextNode)
            {
                case SerializedNodeType.NullPrimitive:
                case SerializedNodeType.Null:
                    break;
                case SerializedNodeType.Primitive:
                {
                    // NOTE*** Have to read the actual typed data from the stream here. So, must
                    //         have a way to resolve ALL PRIMITIVE TYPES - even if they are NOT
                    //         IN THE LOADED ASSEMBLIES! 
                    //
                    //         This should include ENUM types - which will have PRIMITIVE support
                    //         for their UNDERLYING TYPE. This is built into the HashedType.
                    //
                    var hashedType = GetHashedType(nextTypeHash);
                    var primitiveType = default(Type);

                    // Try and resolve against LOADED ASSEMBLIES
                    if (RecursiveSerializerTypeFactory.DoesTypeExist(hashedType))
                    {
                        var resolvedType = RecursiveSerializerTypeFactory.ResolveAsActual(hashedType);

                        // Resolve as ACTUAL -> IMPLEMENTING
                        primitiveType = resolvedType.GetImplementingType();
                    }

                    // RESOLVE AGAINST PRIMITIVES + ENUM UNDERLYING TYPE
                    else
                        primitiveType = RecursiveSerializerTypeFactory.ResolveAssemblyType(hashedType.Implementing.EnumUnderlyingType);

                    if (!FormatterFactory.IsPrimitiveSupported(primitiveType))
                        throw new Exception("Unsupported primitive type found:  " + primitiveType.FullName);

                    // READ PRIMITIVE VALUE FROM STREAM
                    primitiveValue = _reader.Read(primitiveType);
                    break;
                }
                case SerializedNodeType.Object:
                {
                    // READ OBJECT ID FROM STREAM
                    objectId = _reader.Read<int>();
                    break;
                }

                case SerializedNodeType.Reference:
                {
                    // READ OBJECT ID FROM STREAM
                    referenceId = _reader.Read<int>();
                    break;
                }
                case SerializedNodeType.Collection:
                {
                    // READ OBJECT ID FROM STREAM
                    objectId = _reader.Read<int>();

                    // READ INTERFACE TYPE
                    interfaceType = _reader.Read<CollectionInterfaceType>();

                    // READ CHILD COUNT
                    childCount = _reader.Read<int>();

                    // ELEMENT HASH TYPE CODE
                    elementTypeHashCode = _reader.Read<int>();
                    break;
                }
                default:
                    throw new Exception("Unhandled SerializedNodeType:  NodeDeserializer.ReadNext");
            }

            return new SerializedNodeData()
            {
                CollectionCount = childCount,
                CollectionElementTypeHashCode = elementTypeHashCode,
                CollectionInterfaceType = interfaceType,
                Mode = nextMode,
                NodeType = nextNode,
                ObjectId = objectId,
                PrimitiveValue = primitiveValue,
                ReferenceId = referenceId,
                TypeHashCode = nextTypeHash
            };
        }

        private HashedType GetHashedType(int hashedTypeCode)
        {
            // CHECK THAT TYPE IS LOADED
            if (!_header.Data.TypeTable.ContainsKey(hashedTypeCode))
                throw new Exception("MISSING HASH CODE read from stream");

            return _header.Data.TypeTable[hashedTypeCode];
        }
    }
}
