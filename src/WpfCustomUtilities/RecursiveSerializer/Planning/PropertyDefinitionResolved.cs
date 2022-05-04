using System;
using System.Reflection;

using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Planning
{
    /// <summary>
    /// Used for pre-planning Deserialization
    /// </summary>
    internal class PropertyDefinitionResolved
    {
        readonly PropertyInfo _reflectionInfo;

        /// <summary>
        /// Create PropertyDefinition from reflected property. Set to NULL for user defined properties
        /// </summary>
        public PropertyDefinitionResolved(PropertyInfo reflectionInfo)
        {
            _reflectionInfo = reflectionInfo;
        }

        public string PropertyName { get; set; }
        public ResolvedHashedType PropertyType { get; set; }

        public PropertyInfo GetReflectedInfo()
        {
            if (_reflectionInfo == null)
                throw new Exception("Trying to retrieve NULL REFLECTION INFO from PropertyDefinition");

            return _reflectionInfo;
        }

        public override bool Equals(object obj)
        {
            var definition = obj as PropertyDefinitionResolved;

            return definition.PropertyName.Equals(this.PropertyName) &&
                   definition.PropertyType.Equals(this.PropertyType);
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.PropertyName, this.PropertyType);
        }

        public override string ToString()
        {
            return string.Format("Name={0}, Type={1}", this.PropertyName, this.PropertyType);
        }
    }
}
