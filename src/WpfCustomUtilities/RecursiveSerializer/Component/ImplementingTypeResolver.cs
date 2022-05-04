using RecursiveSerializer.Formatter;

using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    internal class ImplementingTypeResolver
    {
        // TRACK HASHED TYPES
        Dictionary<int, ResolvedHashedType> _typeDict;

        internal ImplementingTypeResolver()
        {
            _typeDict = new Dictionary<int, ResolvedHashedType>();
        }

        internal Dictionary<int, ResolvedHashedType> GetResolvedTypes()
        {
            return _typeDict;
        }

        /// <summary>
        /// MUST CALL FOR THE ROOT OBJECT TO RESOVLE THE ROOT AS IT'S IMPLEMENTING TYPE
        /// </summary>
        internal ResolvedHashedType ResolveRoot(object root)
        {
            // Forces implementing type and declaring type to be equal for the root object
            var rootType = root.GetType();

            return Resolve(root, rootType);
        }

        internal ResolvedHashedType Resolve(object theObject, Type declaringType)
        {
            // Careful resolving null references and null primitives
            if (!ReferenceEquals(theObject, null))
            {
                // PERFORMANCE: Try to prevent creating new hashed type
                var hashCode = RecursiveSerializerTypeFactory.CalculateHashCode(declaringType, theObject.GetType());

                if (_typeDict.ContainsKey(hashCode))
                    return _typeDict[hashCode];

                return Resolve(theObject, new ResolvedHashedType(declaringType));
            }
            else
                return Resolve(theObject, new ResolvedHashedType(declaringType));
        }

        /// <summary>
        /// Calculate object hashed type from the object and from it's property type. VALIDATES TYPE!
        /// </summary>
        internal ResolvedHashedType Resolve(object theObject, ResolvedHashedType theObjectType)
        {
            var isPrimitive = FormatterFactory.IsPrimitiveSupported(theObjectType.GetImplementingType());

            // PRIMITIVE NULL
            if (isPrimitive && ReferenceEquals(theObject, null))
                return CreateDeclaringType(theObjectType);

            // NULL
            if (theObject == null)
                return CreateDeclaringType(theObjectType);

            // PRIMITIVE
            if (isPrimitive)
                return CreateDeclaringAndImplementingType(theObject, theObjectType);

            // *** NOTE:  Trying to work with MSFT Type... So, just using this to catch things we 
            //            might have missed.
            //

            // DECLARING TYPE != IMPLEMENTING TYPE
            if (!theObject.GetType().Equals(theObjectType.GetDeclaringType()))
            {
                // WHAT TO DO?!
                //
                // 1) Re-create HashedType with actual implementing type
                // 2) Validate that the DECLARING type is assignable from the IMPLEMENTING type
                //
                // NOTE*** COVER ALL INHERITANCE BASES (INTERFACE, SUB-CLASS, ETC...)
                //

                // Validate assignability
                if (!theObjectType.GetDeclaringType().IsAssignableFrom(theObject.GetType()))
                    throw new RecursiveSerializerException(theObjectType, "Invalid property definition:  property DECLARING type is NOT ASSIGNABLE from the object IMPLEMENTING type");

                // Re-create HASHED TYPE
                var hashedType = CreateDeclaringAndImplementingType(theObject, theObjectType);

                // INTERFACE (VALIDATING)
                if (theObjectType.GetDeclaringType().IsInterface)
                {
                    return hashedType;
                }
                // SUB CLASS (VALIDATING)
                else if (theObject.GetType().IsSubclassOf(theObjectType.GetDeclaringType()))
                {
                    return hashedType;
                }
                else
                    throw new RecursiveSerializerException(theObjectType, "Unhandled polymorphic object type:  " + theObjectType.ToString());
            }
            // DECLARING TYPE == IMPLEMENTING TYPE
            else
            {
                return CreateDeclaringAndImplementingType(theObject, theObjectType);
            }
        }

        private ResolvedHashedType CreateDeclaringType(ResolvedHashedType hashedType)
        {
            var hashCode = hashedType.GetHashCode();

            if (!_typeDict.ContainsKey(hashCode))
                _typeDict.Add(hashedType.GetHashCode(), hashedType);

            return _typeDict[hashCode];
        }

        private ResolvedHashedType CreateDeclaringAndImplementingType(object theObject, ResolvedHashedType theObjectType)
        {
            // PERFORMANCE: Try to prevent creating new hashed type
            var hashCode = RecursiveSerializerTypeFactory.CalculateHashCode(theObjectType.GetDeclaringType(), theObject.GetType());

            if (!_typeDict.ContainsKey(hashCode))
                _typeDict.Add(hashCode, new ResolvedHashedType(theObjectType.GetDeclaringType(), theObject.GetType()));

            return _typeDict[hashCode];
        }
    }
}
