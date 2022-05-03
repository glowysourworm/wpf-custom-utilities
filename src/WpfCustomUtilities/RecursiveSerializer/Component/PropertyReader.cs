using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    /// <summary>
    /// Reads properties (from serialization backend) for the invoker
    /// </summary>
    public class PropertyReader : IPropertyReader
    {
        readonly Dictionary<string, PropertyResolvedInfo> _properties;

        internal PropertyReader(IEnumerable<PropertyResolvedInfo> properties)
        {
            _properties = properties.ToDictionary(property => property.PropertyName, property => property);
        }

        internal IEnumerable<PropertyResolvedInfo> Properties { get { return _properties.Values; } }

        public T Read<T>(string propertyName)
        {
            if (!_properties.ContainsKey(propertyName))
                throw new ArgumentException("Property not present in the underlying stream:  " + propertyName);

            // CHECK ASSIGNABILITY
            if (!typeof(T).IsAssignableFrom(_properties[propertyName].ResolvedType.GetImplementingType()))
                throw new ArgumentException("Requested property type is invalid:  " + propertyName);

            return (T)_properties[propertyName].ResolvedObject;
        }
    }
}
