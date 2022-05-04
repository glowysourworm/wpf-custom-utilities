using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.IO.Interface;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Streaming
{
    internal class HeaderDeserializer
    {
        readonly ISerializationStreamReader _reader;
        readonly RecursiveSerializerConfiguration _configuration;

        internal HeaderDeserializer(ISerializationStreamReader reader, RecursiveSerializerConfiguration configuration)
        {
            _reader = reader;
            _configuration = configuration;
        }

        /// <summary>
        /// Reads in data and stores collections for the PropertyDeserializer. THROWS EXCEPTIONS TO ACCOUNT FOR CONFIGURATION
        /// SETTINGS! RETURNS NULL IF CHANGE PREVIEW RETURNS "NO" FROM THE USER.
        /// </summary>
        internal DeserializedHeader Deserialize()
        {
            // Read ALL raw data from stream
            var header = ReadData();

            // Resolve header data and figure out how to proceed
            var result = ResolveData(header);

            // CHECK FOR CHANGES TO THE HEADER
            if (result.MissingSpecificationLookup.Any() ||
                result.MissingTypeTable.Any() ||
                result.ModifiedSpecificationLookup.Any())
            {
                if (!_configuration.IgnoreRemovedProperties)
                    throw new Exception("Recursive Serializer detected changes to the object type graph in the currently loaded assembly. " +
                                        "This can be IGNORED by setting the IgnoreRemovedProperties flag in the configuration",
                                        new RecursiveSerializerInnerException());

                // PREVIEW CHANGES
                else if (_configuration.PreviewRemovedProperties)
                {
                    var builder = new StringBuilder();

                    builder.AppendLine("Changes to object schema detected. Please preview changes and click OK to proceed.");
                    builder.AppendLine();

                    // MISSING TYPES
                    if (result.MissingTypeTable.Any())
                    {
                        var distinctTypes = result.MissingTypeTable.Values.Distinct().ToList();

                        builder.AppendFormat("Missing Types ({0}):  ", distinctTypes.Count);
                        builder.AppendLine();

                        foreach (var hashedType in distinctTypes)
                            builder.AppendLine(hashedType.Declaring.Type);
                    }

                    // MISSING PROPERTIES
                    if (result.MissingSpecificationLookup.Any())
                    {
                        var distinctSpecifications = result.MissingSpecificationLookup.Values.Distinct().ToList();

                        builder.AppendFormat("Affected Object Graphs ({0}):  ", distinctSpecifications.Count);
                        builder.AppendLine();

                        foreach (var specification in distinctSpecifications)
                            builder.AppendLine(specification.ObjectType.Declaring.Type);
                    }

                    // MODIFIED SPECIFICATIONS
                    if (result.ModifiedSpecificationLookup.Any())
                    {
                        var distinctSpecifications = result.ModifiedSpecificationLookup.Values.Distinct().ToList();

                        builder.AppendFormat("Affected Properties ({0}):  ", distinctSpecifications.Count);
                        builder.AppendLine();

                        foreach (var specification in distinctSpecifications)
                        {
                            builder.AppendFormat("For Type:  {0}", specification.ObjectType.Declaring.Type);
                            builder.AppendLine();

                            foreach (var definition in specification.Definitions)
                                builder.AppendFormat("\tProperty Name:  {0}", definition.PropertyName);
                        }
                    }

                    throw new NotImplementedException("Need to remove the UI from the base library - pass string message back to user code");

                    //if (MessageBox.Show(builder.ToString(), "Changes to Object Graph Detected!", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    //    return result;

                    //else
                    //    return null;
                }
            }

            return result;
        }

        private DeserializedHeader ResolveData(SerializedHeader header)
        {
            // Resolve Root Type
            var resolvedRoot = RecursiveSerializerTypeFactory.ResolveAsActual(header.SerializedType);

            var resolvedTypes = new Dictionary<int, ResolvedHashedType>();
            var missingTypes = new Dictionary<int, HashedType>();
            var resolvedSpecifications = new Dictionary<PropertySpecificationResolved, List<int>>();
            var modifiedSpecifications = new Dictionary<PropertySpecification, List<int>>();
            var missingSpecifications = new Dictionary<PropertySpecification, List<int>>();

            // Resolve types from assembly
            foreach (var type in header.TypeTable.Values)
            {
                var typeExists = RecursiveSerializerTypeFactory.DoesTypeExist(type);

                if (typeExists)
                    resolvedTypes.Add(type.GetHashCode(), RecursiveSerializerTypeFactory.ResolveAsDeclaring(type));

                else
                    missingTypes.Add(type.GetHashCode(), type);
            }

            // Resolve specifications from assembly
            foreach (var specification in header.PropertySpecificationGroups)
            {
                var typeExists = RecursiveSerializerTypeFactory.DoesTypeExist(specification.Key.ObjectType);

                if (typeExists)
                {
                    var actualType = RecursiveSerializerTypeFactory.ResolveAsActual(specification.Key.ObjectType);

                    // REFLECTED SPECIFICATION:  Used for non-user defined specifications
                    var reflectedSpecification = !specification.Key.IsUserDefined ? RecursiveSerializerStore.GetImplementingSpecification(actualType, specification.Key.Definitions) :
                                                                                    null;

                    var missingDefinitions = new List<PropertyDefinition>();
                    var resolvedDefinitions = new List<PropertyDefinitionResolved>();

                    foreach (var definition in specification.Key.Definitions)
                    {
                        var propertyTypeExists = RecursiveSerializerTypeFactory.DoesTypeExist(definition.PropertyType);

                        if (!propertyTypeExists)
                            missingDefinitions.Add(definition);

                        else
                        {
                            // USER DEFINED
                            if (specification.Key.IsUserDefined)
                            {
                                resolvedDefinitions.Add(new PropertyDefinitionResolved(null)
                                {
                                    PropertyName = definition.PropertyName,
                                    PropertyType = RecursiveSerializerTypeFactory.ResolveAsActual(definition.PropertyType)
                                });
                            }

                            // REFLECTED
                            else
                            {
                                var resolvedProperty = reflectedSpecification
                                                            .ResolvedDefinitions
                                                            .FirstOrDefault(reflectedDefinition =>
                                                            {
                                                                return reflectedDefinition.PropertyName.Equals(definition.PropertyName) &&

                                                                       // HASH CODE DEFINITIONS SHOULD MATCH
                                                                       reflectedDefinition.PropertyType.GetHashCode() == definition.PropertyType.GetHashCode();
                                                            });

                                // THIS SHOULD NOT HAPPEN!
                                if (resolvedProperty == null)
                                    throw new Exception("Missing RESOLVED REFLECTED PROPERTY! " + definition.PropertyType.ToString());

                                resolvedDefinitions.Add(new PropertyDefinitionResolved(resolvedProperty.GetReflectedInfo())
                                {
                                    PropertyName = definition.PropertyName,
                                    PropertyType = resolvedProperty.PropertyType
                                });
                            }
                        }
                    }

                    // VERIFY MISSING DEFINITIONS (TODO: PROVIDE WAY TO INSTANTIATE EXTRA (NEW) PROPERTIES)
                    if (!specification.Key.IsUserDefined)
                    {
                        if (missingDefinitions.Count != reflectedSpecification.MissingDefinitions.Count())
                            throw new Exception("MIS-MATCH BETWEEN REFLECTED AND RESOLVED SPECIFICATION:  Modified Definintions != Missing Definitions for " + specification.Key.ObjectType.ToString());

                        // RESOLVED (COMPLETELY (OR) PARTIALLY)
                        resolvedSpecifications.Add(new PropertySpecificationResolved(actualType,
                                                                                     false,
                                                                                     resolvedDefinitions,
                                                                                     reflectedSpecification.ExtraDefinitions,
                                                                                     reflectedSpecification.MissingDefinitions), specification.Value);
                    }
                    else
                    {
                        // RESOLVED (COMPLETELY (OR) PARTIALLY) WITHOUT HELP OF REFLECTED PROPERTIES
                        resolvedSpecifications.Add(new PropertySpecificationResolved(actualType,
                                                                                     true,
                                                                                     resolvedDefinitions,
                                                                                     new PropertyDefinitionResolved[] { },
                                                                                     missingDefinitions),
                                                                                     specification.Value);
                    }


                    // MODIFIED (PARTIAL DEFINITIONS)
                    if (missingDefinitions.Any())
                        modifiedSpecifications.Add(new PropertySpecification(specification.Key.ObjectType, specification.Key.IsUserDefined, missingDefinitions), specification.Value);

                    // PROBLEM: Should have either completely resolved type (or) partially
                    if (resolvedDefinitions.Count + missingDefinitions.Count != specification.Key.Definitions.Count())
                        throw new Exception("Improperly deserialized type specification:  " + specification.Key.ObjectType.ToString());
                }

                // MISSING TYPE
                else
                    missingSpecifications.Add(specification.Key, specification.Value);
            }

            return new DeserializedHeader(resolvedRoot,
                                          header,
                                          resolvedTypes,
                                          missingTypes,
                                          resolvedSpecifications,
                                          modifiedSpecifications,
                                          missingSpecifications);
        }

        private SerializedHeader ReadData()
        {
            var typeTable = new Dictionary<int, HashedType>();
            var specifications = new Dictionary<PropertySpecification, List<int>>();

            // ROOT TYPE
            var rootType = _reader.Read<HashedType>();

            // TYPE TABLE COUNT
            var count = _reader.Read<int>();

            // TYPE TABLE
            for (int index = 0; index < count; index++)
            {
                var hashedType = _reader.Read<HashedType>();

                typeTable.Add(hashedType.GetHashCode(), hashedType);
            }

            // PROPERTY SPECIFICATION TABLE
            var specificationCount = _reader.Read<int>();

            for (int index = 0; index < specificationCount; index++)
            {
                // PROPERTY SPECIFICATION { Count, PropertyDefinition[] }
                var definitionCount = _reader.Read<int>();

                // PROPERTY SPECIFICATION TYPE (HASH CODE ONLY)
                var typeHashCode = _reader.Read<int>();

                // PROPERTY SPECIFICATION -> IS USER DEFINED
                var isUserDefined = _reader.Read<bool>();

                if (!typeTable.ContainsKey(typeHashCode))
                    throw new Exception("Missing HashedType from serialized data");

                var definitions = new List<PropertyDefinition>();

                for (int definitionIndex = 0; definitionIndex < definitionCount; definitionIndex++)
                {
                    var definition = new PropertyDefinition();

                    // PROPERTY NAME
                    definition.PropertyName = _reader.Read<string>();

                    // PROPERTY TYPE (HASH CODE)
                    var definitionTypeHashCode = _reader.Read<int>();

                    if (!typeTable.ContainsKey(definitionTypeHashCode))
                        throw new Exception("Missing HashedType from serialized data");

                    // PROPERTY TYPE
                    definition.PropertyType = typeTable[definitionTypeHashCode];

                    definitions.Add(definition);
                }

                // OBJECT REFERENCES
                var objectIdCount = _reader.Read<int>();

                var objectIds = new List<int>();

                for (int objectIndex = 0; objectIndex < objectIdCount; objectIndex++)
                {
                    // OBJECT ID
                    var objectId = _reader.Read<int>();

                    objectIds.Add(objectId);
                }

                // SPECICIFATION IS READY!
                var specification = new PropertySpecification(typeTable[typeHashCode], isUserDefined, definitions);


                if (specifications.ContainsKey(specification))
                    throw new Exception("Duplicate specification found for type:  " + typeTable[typeHashCode].ToString());

                specifications.Add(specification, objectIds);
            }

            return new SerializedHeader(rootType, typeTable, specifications);
        }
    }
}
