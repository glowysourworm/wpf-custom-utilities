using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Target;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    public class PropertyWriter : IPropertyWriter
    {
        // Actual object contextual properties
        Dictionary<string, PropertyResolvedInfo> _properties;

        // Object type for this writer
        ResolvedHashedType _objectType;

        // Flag for user defined properties
        bool _isUserDefined;

        // RESOLVER FOR OBJECT INFO + HASHED TYPE RULES!
        readonly ImplementingTypeResolver _resolver;

        internal PropertyWriter(ImplementingTypeResolver resolver, ResolvedHashedType objectType)
        {
            _objectType = objectType;
            _properties = new Dictionary<string, PropertyResolvedInfo>();
            _resolver = resolver;
            _isUserDefined = false;
        }

        public void Write<T>(string propertyName, T property)
        {
            if (_properties.ContainsKey(propertyName))
                throw new ArgumentException("Property with the same name is already added to the reader:  " + propertyName);

            // Create the HASH TYPE from the template "T" and the implementing property type
            var hashedType = ResolveType(property, typeof(T));

            // Set user defined
            _isUserDefined = true;

            _properties.Add(propertyName, new PropertyResolvedInfo(null)
            {
                PropertyName = propertyName,
                ResolvedObject = property,
                ResolvedType = hashedType
            });
        }

        /// <summary>
        /// Returns entire list of properties
        /// </summary>
        internal IEnumerable<PropertyResolvedInfo> GetProperties()
        {
            return _properties.Values;
        }

        internal PropertySpecificationResolved GetPropertySpecification()
        {
            // BUILD SPECIFICATION
            return new PropertySpecificationResolved(_objectType, _isUserDefined,
                _properties.Values
                           .Select(info =>
                           {
                               // NULL for user-defined properties
                               return new PropertyDefinitionResolved(_isUserDefined ? null : info.GetReflectedInfo())
                               {
                                   PropertyName = info.PropertyName,
                                   PropertyType = info.ResolvedType
                               };
                           }), new PropertyDefinitionResolved[] { },
                               new PropertyDefinition[] { });
        }

        internal void ReflectProperties(SerializedNodeBase objectNode)
        {
            if (ReferenceEquals(objectNode.GetObject(), null))
                throw new RecursiveSerializerException(objectNode.Type, "Trying to reflect properties on a null object");

            // FETCH SPECIFICATION (BASED ON HASHED TYPE ONLY!)
            var specification = RecursiveSerializerStore.GetDeclaringSpecification(objectNode.Type);

            // FLAG REFLECTED PROPERTIES
            _isUserDefined = false;

            // RESOLVE USING REFLECTION
            try
            {
                _properties = specification.ResolvedDefinitions.Select(definition =>
                {
                    // Use Reflection
                    var resolvedObject = definition.GetReflectedInfo()
                                                   .GetValue(objectNode.GetObject());

                    // RESOLVE TYPE for property
                    var resolvedType = _resolver.Resolve(resolvedObject, definition.PropertyType);

                    return new PropertyResolvedInfo(definition.GetReflectedInfo())
                    {
                        PropertyName = definition.PropertyName,
                        ResolvedObject = resolvedObject,
                        ResolvedType = resolvedType
                    };

                }).ToDictionary(info => info.PropertyName, info => info);
            }
            catch (Exception innerException)
            {
                throw new Exception("Error reflecting type:  " + objectNode.Type.ToString(), innerException);
            }
        }

        private ResolvedHashedType ResolveType(object theObject, Type theObjectType)
        {
            // Catch "Backend" exception to hide from user 
            try
            {
                // Validate the object info
                return _resolver.Resolve(theObject, theObjectType);
            }
            catch (Exception innerException)
            {
                throw new Exception("Error writing property for type " + theObjectType.FullName, innerException);
            }
        }
    }
}
