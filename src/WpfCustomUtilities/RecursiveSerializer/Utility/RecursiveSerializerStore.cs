using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Interface;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    /// <summary>
    /// Static store for reflected type info
    /// </summary>
    internal static class RecursiveSerializerStore
    {
        // Keep track of property types to avoid extra reflection calls
        static Dictionary<int, PropertySpecificationResolved> _implementingDict;
        static Dictionary<ResolvedHashedType, PropertySpecificationResolved> _declaredDict;

        // Keep track of constructors and get methods
        static Dictionary<ResolvedHashedType, RecursiveSerializerMemberInfo> _memberInfoDict;

        internal static readonly string GetMethodName = "GetProperties";

        static RecursiveSerializerStore()
        {
            _implementingDict = new Dictionary<int, PropertySpecificationResolved>();
            _declaredDict = new Dictionary<ResolvedHashedType, PropertySpecificationResolved>();
            _memberInfoDict = new Dictionary<ResolvedHashedType, RecursiveSerializerMemberInfo>();
        }

        /// <summary>
        /// Validates the type and CREATES its serialization mode while retrieving members for the recursive serializer. THROWS 
        /// EXCEPTIONS! (FOR SERIALIZATION)
        /// </summary>
        internal static RecursiveSerializerMemberInfo GetMemberInfo(ResolvedHashedType type)
        {
            // Fetch + Validate the serialization mode
            var serializationMode = GetMemberInfoMode(type);

            return CreateBaseMemberInfo(type, serializationMode);
        }

        internal static RecursiveSerializerMemberInfo GetMemberInfo(ResolvedHashedType type, SerializationMode mode)
        {
            return CreateBaseMemberInfo(type, mode);
        }

        private static SerializationMode GetMemberInfoMode(ResolvedHashedType type)
        {
            if (_memberInfoDict.ContainsKey(type))
                return _memberInfoDict[type].Mode;

            var hasInterface = type.GetImplementingType().HasInterface<IRecursiveSerializable>();

            if (hasInterface)
                return SerializationMode.Specified;

            else
                return SerializationMode.Default;
        }

        private static RecursiveSerializerMemberInfo CreateBaseMemberInfo(ResolvedHashedType type, SerializationMode mode)
        {
            // CAN ONLY UTILIZE IF MODES MATCH
            if (_memberInfoDict.ContainsKey(type))
            {
                if (_memberInfoDict[type].Mode != mode)
                    throw new RecursiveSerializerException(type, "Serialization mode for type doesn't match the code definition");
            }

            var parameterlessCtor = type.GetImplementingType().GetConstructor(new Type[] { });
            var specifiedModeCtor = type.GetImplementingType().GetConstructor(new Type[] { typeof(IPropertyReader) });
            var getMethod = type.GetImplementingType().GetMethod(RecursiveSerializerStore.GetMethodName, new Type[] { typeof(IPropertyWriter) });

            var hasInterface = type.GetImplementingType().HasInterface<IRecursiveSerializable>();

            if (mode == SerializationMode.Default && parameterlessCtor == null)
                throw new RecursiveSerializerException(type, "Improper use of Recursive Serializer - must have a parameterless constructor. (See Inner Exception)");

            if (mode == SerializationMode.Specified)
            {
                if (!hasInterface)
                    throw new RecursiveSerializerException(type, "Improper use of SerializationMode.Specified - must implement IRecursiveSerializable");
            }

            // Create the primary members for the serializer
            var memberInfo = new RecursiveSerializerMemberInfo(parameterlessCtor,
                                                               mode == SerializationMode.Specified ? specifiedModeCtor : null,
                                                               mode == SerializationMode.Specified ? getMethod : null,
                                                               mode);

            _memberInfoDict[type] = memberInfo;

            return memberInfo;
        }

        /// <summary>
        /// (FOR SERIAILZATION) NOTE*** ORDERED TO KEEP SERIALIZATION CONSISTENT!!! Builds the type specification for the implementing type. This is assumed to be a
        /// loaded type against the assembly (loaded assemblies). This will simply reflect properties that have a public get / set method and
        /// order them by name - creating PropertyDefinition instances. THESE REPRESENT DECLARING TYPES; AND MUST BE VERIFIED BY PROPERTY AGAINST THE ASSEMBLY!
        /// </summary>
        internal static PropertySpecificationResolved GetDeclaringSpecification(ResolvedHashedType actualType)
        {
            if (!_declaredDict.ContainsKey(actualType))
            {
                // ORDER BY PROPERTY NAME
                var reflectedDefinitions = actualType.GetImplementingType()
                                                     .GetProperties()
                                                     .Where(property => property.GetMethod != null && property.SetMethod != null)
                                                     .OrderBy(property => property.Name)
                                                     .Select(property => new PropertyDefinition()
                                                     {
                                                         PropertyName = property.Name,
                                                         PropertyType = RecursiveSerializerTypeFactory.Build(property.PropertyType)

                                                     }).ToList();

                var reflectedSpecification = CreateSpecification(actualType, reflectedDefinitions);

                // VERIFY THAT THE SPECIFICATION IS VALID
                if (reflectedSpecification.ExtraDefinitions.Any() ||
                    reflectedSpecification.MissingDefinitions.Any())
                    throw new Exception("MIS-MATCHING REFLECTED SPECIFICATION FOR TYPE:  " + actualType.ToString());

                _declaredDict.Add(actualType, reflectedSpecification);
            }

            return _declaredDict[actualType];
        }

        /// <summary>
        /// (FOR DESERIALIZATION) NOTE*** ORDERED TO KEEP SERIALIZATION CONSISTENT!!! THESE NEED TO BE RESOLVED AGAINST THE OBJECT IMPLEMENTATION! The implementing properties
        /// carry the object property implementing types; and must be sent in to be resolved. If there are any discrepancies in the type they will be 
        /// saved as part of the specification. ACTUAL TYPE MEANS BOTH DECLARING AND IMPLEMENTING ARE RESOLVED WITH THE ASSEMBLY. The implementing properties
        /// are NOT YET RESOLVED. These will be looked at and resolved against the assembly (loaded assemblies).
        /// </summary>
        internal static PropertySpecificationResolved GetImplementingSpecification(ResolvedHashedType actualType, IEnumerable<PropertyDefinition> implementingProperties)
        {
            var hashCode = CreateHashCode(actualType, implementingProperties);

            if (!_implementingDict.ContainsKey(hashCode))
            {
                _implementingDict.Add(hashCode, CreateSpecification(actualType, implementingProperties));
            }

            return _implementingDict[hashCode];
        }

        // Creates the specification based on the supplied implementing properties
        private static PropertySpecificationResolved CreateSpecification(ResolvedHashedType actualType, IEnumerable<PropertyDefinition> implementingProperties)
        {
            // ORDER BY PROPERTY NAME
            var reflectedDefinitions = actualType.GetImplementingType()
                                                 .GetProperties()
                                                 .Where(property => property.GetMethod != null && property.SetMethod != null)
                                                 .OrderBy(property => property.Name)
                                                 .Select(property =>
                                                 {
                                                     // Match property definition by PROPERTY NAME and PROPERTY DECLARING TYPE
                                                     var implementingDefinition = implementingProperties.FirstOrDefault(definition =>
                                                     {
                                                         var declaringType = RecursiveSerializerTypeFactory.ResolveAssemblyType(definition.PropertyType.Declaring);

                                                         return definition.PropertyName == property.Name &&
                                                                declaringType.Equals(property.PropertyType);
                                                     });

                                                     // RESOLVED DEFINITION
                                                     if (implementingDefinition != null)
                                                     {
                                                         // Take the implementing data and resolve it
                                                         var declaringType = RecursiveSerializerTypeFactory.ResolveAssemblyType(implementingDefinition.PropertyType.Declaring);
                                                         var implementingType = RecursiveSerializerTypeFactory.ResolveAssemblyType(implementingDefinition.PropertyType.Implementing);

                                                         // Build hashed type for the resolved property
                                                         var actualHashedType = RecursiveSerializerTypeFactory.Build(declaringType, implementingType);
                                                         var resolvedHashType = RecursiveSerializerTypeFactory.ResolveAsActual(actualHashedType);

                                                         return new
                                                         {
                                                             Definition = new PropertyDefinitionResolved(property)
                                                             {
                                                                 PropertyName = property.Name,
                                                                 PropertyType = resolvedHashType
                                                             },
                                                             IsResolved = true
                                                         };
                                                     }

                                                     // EXTRA DEFINITION
                                                     else
                                                     {
                                                         return new
                                                         {
                                                             Definition = new PropertyDefinitionResolved(property)
                                                             {
                                                                 PropertyName = property.Name,

                                                                 // Resolving as DECLARED TYPE
                                                                 PropertyType = RecursiveSerializerTypeFactory.BuildAndResolve(property.PropertyType),
                                                             },
                                                             IsResolved = true
                                                         };
                                                     }
                                                 })
                                                 .ToList();

            // Resolved Definitions
            var resolvedDefinitions = reflectedDefinitions.Where(x => x.IsResolved).Select(x => x.Definition).ToList();

            // Extra Definitions
            var extraDefinitions = reflectedDefinitions.Where(x => !x.IsResolved).Select(x => x.Definition).ToList();

            // Missing Definitions (from loaded Assemblies)
            var missingDefinitions = implementingProperties.Where(definition => !resolvedDefinitions.Any(reflectedDefinition => reflectedDefinition.PropertyName == definition.PropertyName))
                                                           .ToList();

            return new PropertySpecificationResolved(actualType, false, resolvedDefinitions, extraDefinitions, missingDefinitions);
        }

        /// <summary>
        /// Creates hash code for implementing property specification
        /// </summary>
        private static int CreateHashCode(ResolvedHashedType actualType, IEnumerable<PropertyDefinition> definitions)
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(actualType, definitions);
        }
    }
}
