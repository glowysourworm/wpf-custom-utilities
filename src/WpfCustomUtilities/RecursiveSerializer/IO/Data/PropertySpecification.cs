using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Data
{
    internal class PropertySpecification
    {
        /// <summary>
        /// Type of object used to create the specification. THIS IS INCLUDED IN THE
        /// HASH CODE! There could be multiple specifications PER TYPE. 
        /// </summary>
        internal HashedType ObjectType { get; private set; }
        internal IEnumerable<PropertyDefinition> Definitions { get; private set; }
        internal bool IsUserDefined { get; private set; }

        // KEEP HASH FOR PERFORMANCE WHEN RESOLVING
        Dictionary<int, PropertyDefinition> _definitionHash;

        // CACHE HASH CODE FOR PERFORMANCE
        int _calculatedHashCode;

        internal PropertySpecification(HashedType objectType, bool isUserDefined, IEnumerable<PropertyDefinition> definitions)
        {
            this.ObjectType = objectType;
            this.Definitions = definitions;
            this.IsUserDefined = isUserDefined;

            // Calculate to save time iterating later
            _definitionHash = definitions.ToDictionary(definition => RecursiveSerializerHashGenerator.CreateSimpleHash(definition.PropertyName, definition.PropertyType),
                                                       definition => definition);
            _calculatedHashCode = default(int);
        }

        internal bool ContainsHashedDefinition(string propertyName, HashedType propertyType)
        {
            var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(propertyName, propertyType);

            return _definitionHash.ContainsKey(hashCode);
        }

        internal PropertyDefinition GetHashedDefinition(string propertyName, HashedType propertyType)
        {
            var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(propertyName, propertyType);

            if (!_definitionHash.ContainsKey(hashCode))
                throw new System.Exception("Missing hash code for property type" + propertyType.ToString());

            return _definitionHash[hashCode];
        }

        public override bool Equals(object obj)
        {
            var specification = obj as PropertySpecification;

            return this.GetHashCode() == specification.GetHashCode();
        }

        public override int GetHashCode()
        {
            if (_calculatedHashCode == default(int))
            {
                var hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(this.ObjectType, this.IsUserDefined);

                foreach (var definition in this.Definitions)
                    hashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(hashCode, definition);

                _calculatedHashCode = hashCode;
            }

            return _calculatedHashCode;
        }

        public override string ToString()
        {
            return string.Format("{ ObjectType={0}, Definitions[{1}] }", this.ObjectType, this.Definitions.Count());
        }
    }
}
