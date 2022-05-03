using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.IO.Interface;
using WpfCustomUtilities.RecursiveSerializer.IO.Streaming;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Target;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    internal class PropertyDeserializer
    {
        // Creates wrapped objects for deserialization
        DeserializationObjectFactory _factory;

        // HEADER DATA
        DeserializedHeader _header;

        // NODE DATA
        Queue<SerializedNodeData> _serializedData;

        readonly RecursiveSerializerConfiguration _configuration;

        internal PropertyDeserializer(RecursiveSerializerConfiguration configuration)
        {
            _factory = new DeserializationObjectFactory();
            _configuration = configuration;
        }

        internal DeserializedHeader GetDeserializedHeader()
        {
            return _header;
        }

        internal T Deserialize<T>(ISerializationStreamReader reader)
        {
            // Procedure
            //
            // 1) Read header
            // 2) Read serialized data from stream
            // 3) Begin recursion through the tree
            //      - Wrap node for deserializing
            //      - Recurse on node
            //      (Validate against serialized data)
            //
            // 5) Resolve the object graph
            //

            // Read header
            var headerDeserializer = new HeaderDeserializer(reader, _configuration);

            // TAKES INTO ACCOUNT THE MISSING / IGNORED PROPERTIES
            _header = headerDeserializer.Deserialize();

            // PREVIEW CHANGES OPTION
            if (_header == null)
                return default(T);

            // Read body
            var nodeDeserializer = new NodeDeserializer(reader, _header);

            // Create resolver for the graph
            var implementingTypeResolver = new ImplementingTypeResolver();
            var resolver = new DeserializationResolver(_configuration, implementingTypeResolver, _header);

            // READ FLAT NODE DATA
            _serializedData = nodeDeserializer.Deserialize();

            // Read root - EXPECTED THAT DECLARING AND IMPLEMENTING TYPES ARE THE SAME
            var hashedType = RecursiveSerializerTypeFactory.Build(typeof(T), typeof(T));
            var rootData = ReadNext(hashedType);
            var rootNode = CreateNode(rootData, hashedType, PropertyDefinition.RootNode);

            // Read stream recursively
            ReadRecurse(rootNode);

            // Stitch together object references recursively:  And -> We've -> Resolved() -> TheObject()! :)
            return resolver.Resolve<T>(rootNode as DeserializedObjectNode);
        }

        private void ReadRecurse(DeserializedNodeBase node)
        {
            // FROM SERIALIZER:
            //
            // Recursively identify node children and analyze by type inspection for SerializationObjectBase.
            // Select formatter for objects that are value types if no ctor / get methods are supplied
            //

            // COLLECTION
            if (node is DeserializedCollectionNode)
            {
                var collectionNode = node as DeserializedCollectionNode;

                // Fetch definitions for properties from the node object
                var specification = collectionNode.GetPropertySpecification();

                // RECURSE ANY CUSTOM PROPERTIES
                foreach (var definition in specification.Definitions)
                {
                    // READ NEXT
                    var rawData = ReadNext(definition.PropertyType);

                    // READ NEXT
                    var propertyNode = CreateNode(rawData, definition.PropertyType, definition);

                    // Store sub-node
                    collectionNode.SubNodes.Add(propertyNode);

                    // RECURSE
                    ReadRecurse(propertyNode);
                }

                // Iterate expected ELEMENT TYPES
                var collection = (collectionNode as DeserializedCollectionNode);

                // VALIDATE ELEMENT TYPE
                if (!_header.Data.TypeTable.ContainsKey(collection.ElementType.GetHashCode()))
                    throw new Exception("Missing ElementType for collection: " + collection.SerializedType.ToString());

                for (int index = 0; index < collection.Count; index++)
                {
                    // READ NEXT
                    var rawData = ReadNext(collection.ElementType);

                    // READ NEXT (ELEMENT TYPE => DECLARING TYPE)
                    var elementNode = CreateCollectionElementNode(rawData, collection.ElementType);

                    // STORE CHILD NODE
                    collectionNode.CollectionNodes.Add(elementNode);

                    // RECURSE
                    ReadRecurse(elementNode);
                }
            }

            // NODE
            else if (node is DeserializedObjectNode)
            {
                var nextNode = node as DeserializedObjectNode;

                var specification = nextNode.GetPropertySpecification();

                // Loop properties:  Verify sub-nodes -> Recurse
                foreach (var definition in specification.Definitions)
                {
                    // READ NEXT
                    var rawData = ReadNext(definition.PropertyType);

                    // Create node
                    var subNode = CreateNode(rawData, definition.PropertyType, definition);

                    // Store sub-node
                    nextNode.SubNodes.Add(subNode);

                    // RECURSE
                    ReadRecurse(subNode);
                }
            }

            // REFERENCE NODE -> Halt Recursion
            else if (node is DeserializedReferenceNode)
            {
                return;
            }

            // LEAF NODE -> Halt Recursion
            else if (node is DeserializedLeafNode)
            {
                return;
            }

            // NULL LEAF NODE -> Halt Recursion
            else if (node is DeserializedNullLeafNode)
            {
                return;
            }

            else
                throw new Exception("Unhandled DeserializedNodeBase type:  PropertyDeserializer.cs");
        }

        private DeserializedNodeBase CreateCollectionElementNode(SerializedNodeData nodeData, HashedType elementType)
        {
            if (!_header.Data.TypeTable.ContainsKey(nodeData.TypeHashCode))
                throw new Exception("MISSING TYPE HASH CODE for ELEMENT type:  " + elementType.ToString());

            // HAVE TO SET IMPLEMENTING TYPE FOR THE COLLECTION ELEMENT
            if (nodeData.TypeHashCode != elementType.GetHashCode())
            {
                // This was the ACTUAL (IMPLEMENTING + DECLARED) type when the element was serialized
                var actualType = _header.Data.TypeTable[nodeData.TypeHashCode];

                // Allow this to be the expected type for collection elements; but check that the collection
                // is assignable
                var resolvedType = RecursiveSerializerTypeFactory.ResolveAsActual(actualType);
                var resolvedElementType = RecursiveSerializerTypeFactory.ResolveAsDeclaring(elementType);

                if (!resolvedElementType.GetDeclaringType().IsAssignableFrom(resolvedType.GetImplementingType()))
                    throw new Exception("COLLECTION ELEMENT TYPE NOT ASSIGNABLE:  " + elementType.ToString());

                return CreateNode(nodeData, actualType, PropertyDefinition.CollectionElement);
            }
            else
                return CreateNode(nodeData, elementType, PropertyDefinition.CollectionElement);
        }

        private DeserializedNodeBase CreateNode(SerializedNodeData nodeData, HashedType expectedType, PropertyDefinition definition)
        {
            DeserializedNodeBase nodeBase = null;

            if (!_header.Data.TypeTable.ContainsKey(nodeData.TypeHashCode))
                throw new Exception("MISSING TYPE HASH CODE for expected type:  " + expectedType.ToString());

            if (nodeData.TypeHashCode != expectedType.GetHashCode())
                throw new Exception("MIS-MATCHING TYPE HASH CODE for expected type:  " + expectedType.ToString());

            switch (nodeData.NodeType)
            {
                case SerializedNodeType.NullPrimitive:
                    nodeBase = _factory.CreateNullPrimitive(definition, expectedType, nodeData.Mode);
                    break;
                case SerializedNodeType.Null:
                    nodeBase = _factory.CreateNullReference(definition, expectedType, nodeData.Mode);
                    break;
                case SerializedNodeType.Primitive:
                    nodeBase = _factory.CreatePrimitive(definition, nodeData.PrimitiveValue, expectedType, nodeData.Mode);
                    break;
                case SerializedNodeType.Object:
                {
                    // LOOKUP PROPRETY SPECIFICATION FROM SERIALIZED HEADER
                    if (!_header.Data.PropertySpecificationLookup.ContainsKey(nodeData.ObjectId))
                        throw new Exception("Missing property specification for expected type:  " + expectedType.ToString());

                    nodeBase = _factory.CreateObject(definition,
                                                     nodeData.ObjectId,
                                                     expectedType,
                                                     nodeData.Mode,
                                                     _header.Data.PropertySpecificationLookup[nodeData.ObjectId]);
                }
                break;
                case SerializedNodeType.Reference:
                    nodeBase = _factory.CreateReference(definition, nodeData.ReferenceId, expectedType, nodeData.Mode);
                    break;
                case SerializedNodeType.Collection:
                {
                    // LOOKUP PROPRETY SPECIFICATION FROM SERIALIZED HEADER
                    if (!_header.Data.PropertySpecificationLookup.ContainsKey(nodeData.ObjectId))
                        throw new Exception("Missing property specification for expected type:  " + expectedType.ToString());

                    // LOOKUP ELEMENT TYPE FOR NODE
                    if (!_header.Data.TypeTable.ContainsKey(nodeData.CollectionElementTypeHashCode))
                        throw new Exception("Missing element type for expected type: " + expectedType.ToString());

                    nodeBase = _factory.CreateCollection(definition,
                                                         nodeData.ObjectId,
                                                         expectedType,
                                                         nodeData.CollectionInterfaceType,
                                                         nodeData.CollectionCount,
                                                         nodeData.Mode,
                                                         _header.Data.PropertySpecificationLookup[nodeData.ObjectId],
                                                         _header.Data.TypeTable[nodeData.CollectionElementTypeHashCode]);
                }
                break;
                default:
                    throw new Exception("Unhandled SerializedNodeType:  PropertyDeserializer.cs");
            }

            return nodeBase;
        }


        private SerializedNodeData ReadNext(HashedType expectedType)
        {
            // DEQUEUE SERIALIZED DATA
            var nextNode = _serializedData.Dequeue();

            if (!_header.Data.TypeTable.ContainsKey(nextNode.TypeHashCode))
                throw new Exception("MISSING HASHED TYPE for expected type:  " + expectedType.ToString());

            return nextNode;
        }
    }
}
