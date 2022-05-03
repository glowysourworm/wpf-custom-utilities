using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Planning
{
    internal class DeserializedHeader
    {
        /// <summary>
        /// Directly deserialized data from the header
        /// </summary>
        internal SerializedHeader Data { get; private set; }

        /// <summary>
        /// RESOLVED hashed types from the header
        /// </summary>
        internal Dictionary<int, ResolvedHashedType> ResolvedTypeTable { get; private set; }

        /// <summary>
        /// MISSING types from the header
        /// </summary>
        internal Dictionary<int, HashedType> MissingTypeTable { get; private set; }

        /// <summary>
        /// RESOLVED Property specification groups of object ids (RESOLVED TYPES with RESOLVED DEFINITIONS)
        /// </summary>
        internal Dictionary<int, PropertySpecificationResolved> ResolvedSpecificationLookup { get; private set; }

        /// <summary>
        /// MODIFIED Property specification (MISSING DEFINITIONS) groups of object ids (RESOLVED TYPES with RESOLVED DEFINITIONS)
        /// </summary>
        internal Dictionary<int, PropertySpecification> ModifiedSpecificationLookup { get; private set; }

        /// <summary>
        /// MISSING Property specification (MISSING WHOLE TYPE!) groups of object ids (RESOLVED TYPES with RESOLVED DEFINITIONS)
        /// </summary>
        internal Dictionary<int, PropertySpecification> MissingSpecificationLookup { get; private set; }

        internal DeserializedHeader(SerializedHeader headerData,
                                    Dictionary<int, ResolvedHashedType> resolvedTypeTable,
                                    Dictionary<int, HashedType> missingTypeTable,
                                    Dictionary<PropertySpecificationResolved, List<int>> resolvedPropertySpecifications,
                                    Dictionary<PropertySpecification, List<int>> modifiedPropertySpecifications,
                                    Dictionary<PropertySpecification, List<int>> missingPropertySpecifications)
        {
            this.Data = headerData;
            this.ResolvedTypeTable = resolvedTypeTable;
            this.MissingTypeTable = missingTypeTable;
            this.ResolvedSpecificationLookup = CreateLookup(resolvedPropertySpecifications);
            this.ModifiedSpecificationLookup = CreateLookup(modifiedPropertySpecifications);
            this.MissingSpecificationLookup = CreateLookup(missingPropertySpecifications);
        }

        private Dictionary<int, K> CreateLookup<K>(Dictionary<K, List<int>> dictionary)
        {
            var result = new Dictionary<int, K>();

            foreach (var element in dictionary)
            {
                foreach (var objectId in element.Value)
                {
                    if (!result.ContainsKey(objectId))
                        result.Add(objectId, element.Key);

                    else
                        throw new System.Exception("Duplicate Property Specification found for object:  " + element.Key.ToString());
                }
            }

            return result;
        }
    }
}
