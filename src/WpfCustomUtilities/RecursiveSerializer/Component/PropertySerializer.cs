using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.Extensions.Collection;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.IO.Interface;
using WpfCustomUtilities.RecursiveSerializer.IO.Streaming;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    internal class PropertySerializer
    {
        // Collection of UNIQUE objects that HAVE BEEN SERIALIZED
        Dictionary<int, SerializedNodeBase> _serializedObjects;

        // Final collection of serialized data
        Queue<SerializedNodeData> _serializedData;

        // SINGLE INSTANCE - ONE PER RUN
        ImplementingTypeResolver _resolver;

        readonly RecursiveSerializerConfiguration _configuration;

        internal PropertySerializer(RecursiveSerializerConfiguration configuration)
        {
            _serializedObjects = new Dictionary<int, SerializedNodeBase>();
            _serializedData = new Queue<SerializedNodeData>();
            _resolver = new ImplementingTypeResolver();
            _configuration = configuration;
        }

        internal void Serialize<T>(ISerializationStreamWriter writer, T theObject)
        {
            if (theObject == null)
                throw new NullReferenceException("Property Serializer cannot serialize a null object");

            // SINGLE INSTANCE PER RUN - STORES ALL RESOLVED TYPES
            _resolver = new ImplementingTypeResolver();

            // CLEAR OUT OLD DATA
            _serializedObjects.Clear();
            _serializedData.Clear();

            // Procedure
            //
            // 0) Reset the OBJECT ID COUNTER
            // 1) Run the planner5
            // 2) Serialize the header
            // 3) (Recurse) Serialize the node graph OBJECTS -> SerializedNodeData
            // 4) Validate OUR serialized objects against the ISerializationPlan
            // 5) Feed the nodes to the Node Serializer
            //

            // Setup components
            var headerSerializer = new HeaderSerializer(writer);
            var planner = new SerializationPlanner<T>(_resolver);
            var nodeSerializer = new NodeSerializer(writer);

            // Reset Id Counter
            SerializedNodeBase.ResetCounter();

            // Run the planner
            var plan = planner.Plan(theObject);

            // Collect data for the header serializer
            var distinctTypes = _resolver.GetResolvedTypes()
                                         .Union(plan.ElementTypeDict)
                                         .DistinctByKey(keyValuePair => keyValuePair.Key)

                                         // Create serializable HashedType data
                                         .ToDictionary(pair => pair.Key, pair => RecursiveSerializerTypeFactory.BuildHashedType(pair.Value));

            var specificationGroups = plan.PropertySpecificationGroups
                                          .ToDictionary(pair =>
                                          {
                                              // Create serializable HashedType data
                                              var hashedType = RecursiveSerializerTypeFactory.BuildHashedType(pair.Key.ObjectType);

                                              var definitions = pair.Key.ResolvedDefinitions.Select(definition => new PropertyDefinition()
                                              {
                                                  PropertyName = definition.PropertyName,
                                                  PropertyType = RecursiveSerializerTypeFactory.BuildHashedType(definition.PropertyType)
                                              });

                                              return new PropertySpecification(hashedType, pair.Key.IsUserDefined, definitions);

                                          }, pair => pair.Value.Select(node => node.Id).ToList());

            // Get type data for the root - set to the header
            var rootType = RecursiveSerializerTypeFactory.Build(typeof(T));

            // Set data in the header
            var header = new SerializedHeader(rootType, distinctTypes, specificationGroups);

            // Serialize the header
            headerSerializer.Serialize(header);

            // Recurse
            SerializeRecurse(plan.RootNode);

            // Validate
            foreach (var objectBase in plan.ReferenceObjects)
            {
                if (!_serializedObjects.ContainsKey(objectBase.Id))
                    throw new Exception("Serialization plan doesn't match the serialized manifest:  " + objectBase.Type.ToString());
            }

            // SERIALIZE!
            nodeSerializer.Serialize(_serializedData);
        }

        internal IEnumerable<SerializedNodeBase> GetSerializedObjects()
        {
            return _serializedObjects.Values;
        }

        private void SerializeRecurse(SerializedNodeBase node)
        {
            // Recursively identify node children and analyze by type inspection for SerializationObjectBase.
            // Select formatter for objects that are value types if no ctor / get methods are supplied

            // LEAF (PRIMITIVE)
            if (node is SerializedLeafNode)
                SerializeNodeObject(node);

            // REFERENCE
            else if (node is SerializedReferenceNode)
                SerializeNodeObject(node);

            // COLLECTION 
            else if (node is SerializedCollectionNode)
            {
                var collectionNode = node as SerializedCollectionNode;

                // Serialize Collection Node (STORES CHILD COUNT)
                SerializeNodeObject(collectionNode);

                // Serialize Sub-Nodes (USED FOR CUSTOM COLLECTION PROPERTIES)
                foreach (var subNode in collectionNode.SubNodes)
                    SerializeRecurse(subNode);

                // Serialize Child Nodes
                foreach (var childNode in collectionNode.CollectionNodes)
                    SerializeRecurse(childNode);
            }

            // OBJECT NODE
            else if (node is SerializedObjectNode)
            {
                var objectNode = node as SerializedObjectNode;

                // Serialize Node
                SerializeNodeObject(objectNode);

                // Serialize Sub-Nodes
                foreach (var subNode in objectNode.SubNodes)
                    SerializeRecurse(subNode);
            }
            else
                throw new Exception("Unhandled SerializedNodeBase type:  PropertySerializer.cs");
        }

        private void SerializeNodeObject(SerializedNodeBase nodeObject)
        {
            // Procedure
            //
            // *** For each type of serialization object store the SerializedNodeType, and the 
            //     SerializationMode ENUMS before continuing with the rest of the data.
            //
            //     Store the TYPE for the NODE OBJECT - then either the NULL, the
            //     DATA (PRIMITIVE), or Object Info ID (for all reference types). 
            //
            //     For COLLECTIONS, also store the child count, and the CollectionInterfaceType ENUM. 
            //

            // LEAF
            if (nodeObject is SerializedLeafNode)
            {
                switch (nodeObject.NodeType)
                {
                    case SerializedNodeType.NullPrimitive:
                    case SerializedNodeType.Null:
                    case SerializedNodeType.Primitive:
                        PrepareNodeData(nodeObject.NodeType, nodeObject.Mode, nodeObject);
                        return;
                    default:
                        throw new Exception("Improperly typed SerializedLeafNode:  PropertySerializer.cs");
                }
            }

            // REFERENCE
            else if (nodeObject is SerializedReferenceNode)
            {
                PrepareNodeData(nodeObject.NodeType, nodeObject.Mode, nodeObject);

                return;
            }

            // *** REFERENCE TYPES

            // STORE REFERENCE
            if (_serializedObjects.ContainsKey(nodeObject.Id))
                throw new Exception("Duplicate reference found:  " + nodeObject.Type.GetImplementingType());

            _serializedObjects.Add(nodeObject.Id, nodeObject);

            // COLLECTION : OBJECT
            if (nodeObject is SerializedCollectionNode)
            {
                if (nodeObject.NodeType != SerializedNodeType.Collection)
                    throw new Exception("Improperly typed SerializedObjectNode:  PropertySerializer.cs");

                PrepareNodeData(nodeObject.NodeType, nodeObject.Mode, nodeObject);
            }

            // OBJECT
            else if (nodeObject is SerializedObjectNode)
            {
                if (nodeObject.NodeType != SerializedNodeType.Object)
                    throw new Exception("Improperly typed SerializedObjectNode:  PropertySerializer.cs");

                PrepareNodeData(nodeObject.NodeType, nodeObject.Mode, nodeObject);

                return;
            }

            else
                throw new Exception("Invalid SerializedNodeBase PropertySerializer.SerializeNodeObject");
        }

        private void PrepareNodeData(SerializedNodeType type,
                                     SerializationMode mode,
                                     SerializedNodeBase nodeObject)
        {
            var nodeData = new SerializedNodeData()
            {
                Mode = mode,
                NodeType = type,

                // HASH CODE FOR TYPE
                TypeHashCode = nodeObject.Type.GetHashCode()
            };

            switch (type)
            {
                case SerializedNodeType.NullPrimitive:
                case SerializedNodeType.Null:
                    break;

                case SerializedNodeType.Primitive:
                    nodeData.PrimitiveValue = nodeObject.GetObject();
                    break;

                case SerializedNodeType.Object:
                    nodeData.ObjectId = nodeObject.Id;
                    break;

                case SerializedNodeType.Reference:
                    nodeData.ReferenceId = (nodeObject as SerializedReferenceNode).ReferenceId;
                    break;

                case SerializedNodeType.Collection:

                    var collection = (nodeObject as SerializedCollectionNode);

                    nodeData.ObjectId = collection.Id;
                    nodeData.CollectionCount = collection.Count;
                    nodeData.CollectionInterfaceType = collection.InterfaceType;
                    nodeData.CollectionElementTypeHashCode = collection.ElementDeclaringType.GetHashCode();
                    break;
                default:
                    throw new Exception("Unhandled SerializedNodeType PropertySerializer.cs");
            }

            _serializedData.Enqueue(nodeData);
        }
    }
}
