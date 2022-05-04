using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Planning
{
    /// <summary>
    /// Carries resolved property definitions that have reflection info attached. Built on deserialization to resolve 
    /// types against loaded assemblies.
    /// </summary>
    internal class PropertySpecificationResolved
    {
        /// <summary>
        /// Type has been resolved with the assembly
        /// </summary>
        internal ResolvedHashedType ObjectType { get; private set; }

        /// <summary>
        /// Property definitions have been defined by the user
        /// </summary>
        internal bool IsUserDefined { get; private set; }

        /// <summary>
        /// Resolved definition collection - BUILT BY THE REFLECTION INFO STORE
        /// </summary>
        internal IEnumerable<PropertyDefinitionResolved> ResolvedDefinitions { get; private set; }

        /// <summary>
        /// Extra property definitions built by the REFLECTION INFO STORE - Not found in the user's 
        /// entries.
        /// </summary>
        internal IEnumerable<PropertyDefinitionResolved> ExtraDefinitions { get; private set; }

        /// <summary>
        /// Extra property definitions sent by the constructor. These were found on deserializing
        /// </summary>
        internal IEnumerable<PropertyDefinition> MissingDefinitions { get; private set; }

        // KEEP HASH FOR PERFORMANCE WHEN RESOLVING
        Dictionary<int, PropertyDefinitionResolved> _definitionHash;
        Dictionary<int, PropertyDefinitionResolved> _extraHash;
        Dictionary<int, PropertyDefinition> _missingHash;

        // CACHE HASH CODE FOR PERFORMANCE
        int _calculatedHashCode;

        internal PropertySpecificationResolved(ResolvedHashedType objectType,
                                               bool isUserDefined,
                                               IEnumerable<PropertyDefinitionResolved> resolvedDefinitions,
                                               IEnumerable<PropertyDefinitionResolved> extraDefinitions,
                                               IEnumerable<PropertyDefinition> missingDefinitions)
        {
            this.ObjectType = objectType;
            this.ResolvedDefinitions = resolvedDefinitions;
            this.ExtraDefinitions = extraDefinitions;
            this.MissingDefinitions = missingDefinitions;
            this.IsUserDefined = isUserDefined;

            // Calculate to save time iterating later
            _definitionHash = resolvedDefinitions.ToDictionary(definition => RecursiveSerializerHashGenerator.CreateSimpleHash(definition.PropertyName, definition.PropertyType),
                                                               definition => definition);

            _extraHash = extraDefinitions.ToDictionary(definition => RecursiveSerializerHashGenerator.CreateSimpleHash(definition.PropertyName, definition.PropertyType),
                                                       definition => definition);

            _missingHash = missingDefinitions.ToDictionary(definition => RecursiveSerializerHashGenerator.CreateSimpleHash(definition.PropertyName, definition.PropertyType),
                                                           definition => definition);

            _calculatedHashCode = default(int);
        }

        internal bool IsMissing(string propertyName, ResolvedHashedType propertyType)
        {
            var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(propertyName, propertyType);

            return _missingHash.ContainsKey(hashCode);
        }

        internal bool IsExtra(string propertyName, ResolvedHashedType propertyType)
        {
            var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(propertyName, propertyType);

            return _extraHash.ContainsKey(hashCode);
        }

        internal bool ContainsResolvedDefinition(string propertyName, ResolvedHashedType propertyType)
        {
            var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(propertyName, propertyType);

            return _definitionHash.ContainsKey(hashCode);
        }

        internal PropertyDefinitionResolved GetResolvedDefinition(string propertyName, ResolvedHashedType propertyType)
        {
            var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(propertyName, propertyType);

            if (!_definitionHash.ContainsKey(hashCode))
                throw new System.Exception("Missing hash code for property type" + propertyType.ToString());

            return _definitionHash[hashCode];
        }

        public override bool Equals(object obj)
        {
            var specification = obj as PropertySpecificationResolved;

            return this.GetHashCode() == specification.GetHashCode();
        }

        public override int GetHashCode()
        {
            if (_calculatedHashCode == default(int))
            {
                var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(this.ObjectType,
                                                                             this.IsUserDefined,
                                                                             this.ResolvedDefinitions,
                                                                             this.ExtraDefinitions,
                                                                             this.MissingDefinitions);

                _calculatedHashCode = hashCode;
            }

            return _calculatedHashCode;
        }

        public override string ToString()
        {
            return string.Format("ObjectType={0}, Definitions[{1}]", this.ObjectType, this.ResolvedDefinitions.Count());
        }
    }
}
