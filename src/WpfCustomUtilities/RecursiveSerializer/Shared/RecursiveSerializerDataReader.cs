using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using WpfCustomUtilities.Extensions.Collection;
using WpfCustomUtilities.RecursiveSerializer.IO;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.IO.Streaming;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Shared.Data;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    /// <summary>
    /// Component for reading serialized data agnostically.
    /// </summary>
    public class RecursiveSerializerDataReader
    {
        /// <summary>
        /// Type of obejct serialized
        /// </summary>
        public TypeInfo SerializedType { get; private set; }

        /// <summary>
        /// RESOLVED hashed types from the header
        /// </summary>
        public SimpleDictionary<int, TypeInfo> ResolvedTypeTable { get; private set; }

        /// <summary>
        /// MISSING types from the header
        /// </summary>
        public SimpleDictionary<int, TypeInfo> MissingTypeTable { get; private set; }

        /// <summary>
        /// RESOLVED Property specification groups of object ids (RESOLVED TYPES with RESOLVED DEFINITIONS)
        /// </summary>
        public SimpleDictionary<int, SerializedTypeSpecification> ResolvedSpecificationLookup { get; private set; }

        /// <summary>
        /// MODIFIED Property specification (MISSING DEFINITIONS) groups of object ids (RESOLVED TYPES with RESOLVED DEFINITIONS)
        /// </summary>
        public SimpleDictionary<int, SerializedTypeSpecification> ModifiedSpecificationLookup { get; private set; }

        /// <summary>
        /// MISSING Property specification (MISSING WHOLE TYPE!) groups of object ids (RESOLVED TYPES with RESOLVED DEFINITIONS)
        /// </summary>
        public SimpleDictionary<int, SerializedTypeSpecification> MissingSpecificationLookup { get; private set; }

        /// <summary>
        /// Raw serialized node data
        /// </summary>
        public List<SerializedNode> SerializedNodes { get; private set; }

        /// <summary>
        /// Set to TRUE if data read was successful
        /// </summary>
        public bool IsValid { get; private set; }

        readonly RecursiveSerializerConfiguration _configuration;

        public RecursiveSerializerDataReader(RecursiveSerializerConfiguration configuration)
        {
            _configuration = configuration;

            this.SerializedType = new TypeInfo();
            this.ResolvedTypeTable = new SimpleDictionary<int, TypeInfo>();
            this.MissingTypeTable = new SimpleDictionary<int, TypeInfo>();
            this.ResolvedSpecificationLookup = new SimpleDictionary<int, SerializedTypeSpecification>();
            this.ModifiedSpecificationLookup = new SimpleDictionary<int, SerializedTypeSpecification>();
            this.MissingSpecificationLookup = new SimpleDictionary<int, SerializedTypeSpecification>();
            this.SerializedNodes = new List<SerializedNode>();
            this.IsValid = false;
        }

        public void Read(Stream stream)
        {
            this.IsValid = false;

            // Procedure
            //
            // 1) Read header
            // 2) Read serialized data from stream
            //

            // This class performs the first few steps of deserialization
            // agnostically.
            //
            var reader = new SerializationStream(stream);

            // Read header
            var headerDeserializer = new HeaderDeserializer(reader, _configuration);

            // TAKES INTO ACCOUNT THE MISSING / IGNORED PROPERTIES
            var header = headerDeserializer.Deserialize();

            //// PREVIEW CHANGES OPTION
            //if (header == null)
            //    return;

            //// Read body
            //var nodeDeserializer = new NodeDeserializer(reader, header);

            //// READ FLAT NODE DATA
            //var serializedData = nodeDeserializer.Deserialize();

            BuildDataStructures(header, Array.Empty<SerializedNodeData>());

            this.IsValid = true;
        }

        private void BuildDataStructures(DeserializedHeader header, IEnumerable<SerializedNodeData> nodes)
        {
            // Serialized Type
            this.SerializedType = new TypeInfo(header.SerializedType.GetImplementingType());

            // Resolved Types
            foreach (var element in header.ResolvedTypeTable)
            {
                this.ResolvedTypeTable.Add(element.Key, new TypeInfo(element.Value.GetImplementingType()));
            }

            // Missing Types
            foreach (var element in header.MissingTypeTable)
            {
                this.MissingTypeTable.Add(element.Key, new TypeInfo()
                {
                    Assembly = element.Value.Declaring.Assembly,
                    Name = element.Value.Declaring.Type
                });
            }

            // Resolved Specifications
            foreach (var element in header.ResolvedSpecificationLookup)
            {
                this.ResolvedSpecificationLookup.Add(element.Key,
                    new SerializedTypeSpecification(new TypeInfo(element.Value.ObjectType.GetDeclaringType()),
                                                    new TypeInfo(element.Value.ObjectType.GetImplementingType()),
                                                    element.Value.ResolvedDefinitions.Select(def => new PropertyInfo()
                                                    {
                                                        Declaring = new TypeInfo(def.PropertyType.GetDeclaringType()),
                                                        Implementing = new TypeInfo(def.PropertyType.GetImplementingType()),
                                                        PropertyName = def.PropertyName,
                                                        IsUserDefined = element.Value.IsUserDefined

                                                    }).Actualize(),
                                                    element.Value.ExtraDefinitions.Select(def => new PropertyInfo()
                                                    {
                                                        Declaring = new TypeInfo(def.PropertyType.GetDeclaringType()),
                                                        Implementing = new TypeInfo(def.PropertyType.GetImplementingType()),
                                                        PropertyName = def.PropertyName,
                                                        IsUserDefined = element.Value.IsUserDefined

                                                    }).Actualize(),
                                                    element.Value.MissingDefinitions.Select(def => new PropertyInfo()
                                                    {
                                                        PropertyName = def.PropertyName,
                                                        Declaring = new TypeInfo(def.PropertyType.Declaring.Assembly, def.PropertyType.Declaring.Type),
                                                        Implementing = new TypeInfo(def.PropertyType.Implementing.Assembly, def.PropertyType.Implementing.Type),
                                                        IsUserDefined = element.Value.IsUserDefined

                                                    }).Actualize()));
            }

            // Modified Specifications
            foreach (var element in header.ModifiedSpecificationLookup)
            {
                this.ModifiedSpecificationLookup.Add(element.Key,
                    new SerializedTypeSpecification(new TypeInfo(element.Value.ObjectType.Declaring),
                                                    new TypeInfo(element.Value.ObjectType.Implementing),
                                                    element.Value.Definitions.Select(def => new PropertyInfo()
                                                    {
                                                        Declaring = new TypeInfo(def.PropertyType.Declaring),
                                                        Implementing = new TypeInfo(def.PropertyType.Implementing),
                                                        PropertyName = def.PropertyName,
                                                        IsUserDefined = element.Value.IsUserDefined

                                                    }).Actualize(), Array.Empty<PropertyInfo>(), Array.Empty<PropertyInfo>()));
            }

            // Missing Specifications
            foreach (var element in header.MissingSpecificationLookup)
            {
                this.MissingSpecificationLookup.Add(element.Key,
                    new SerializedTypeSpecification(new TypeInfo(element.Value.ObjectType.Declaring),
                                                    new TypeInfo(element.Value.ObjectType.Implementing),
                                                    element.Value.Definitions.Select(def => new PropertyInfo()
                                                    {
                                                        Declaring = new TypeInfo(def.PropertyType.Declaring),
                                                        Implementing = new TypeInfo(def.PropertyType.Implementing),
                                                        PropertyName = def.PropertyName,
                                                        IsUserDefined = element.Value.IsUserDefined

                                                    }).Actualize(), Array.Empty<PropertyInfo>(), Array.Empty<PropertyInfo>()));
            }

            // Raw Node Data
            foreach (var node in nodes)
                this.SerializedNodes.Add(new SerializedNode()
                {
                    CollectionCount = node.CollectionCount,
                    CollectionElementTypeHashCode = node.CollectionElementTypeHashCode,
                    CollectionInterfaceType = (byte)node.CollectionInterfaceType,
                    Mode = (byte)node.Mode,
                    NodeType = (byte)node.NodeType,
                    ObjectId = node.ObjectId,
                    PrimitiveValue = node.PrimitiveValue,
                    ReferenceId = node.ReferenceId,
                    TypeHashCode = node.TypeHashCode
                });
        }
    }
}
