using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal abstract class DeserializedNodeBase
    {
        /// <summary>
        /// The defining property for the deserialized node
        /// </summary>
        internal PropertyDefinition Property { get; private set; }

        /// <summary>
        /// HashedType as read from the serialized stream
        /// </summary>
        internal HashedType SerializedType { get; private set; }

        internal SerializationMode Mode { get; private set; }

        public DeserializedNodeBase(PropertyDefinition property, HashedType type, SerializationMode mode)
        {
            this.Property = property;
            this.SerializedType = type;
            this.Mode = mode;
        }

        /// <summary>
        /// Constructs and stores the object using the appropriate constructor (See RecursiveSerializerMemberInfo)
        /// </summary>
        internal abstract void Construct(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties);

        /// <summary>
        /// Retrieves list of property definitions passed in at the constructor (SHOULD BE FOR REFERENCE TYPES ONLY)
        /// </summary>
        /// <returns></returns>
        internal abstract PropertySpecification GetPropertySpecification();

        /// <summary>
        /// Resolves the object's value with the initial reference and returns the 
        /// final result. 
        /// </summary>
        internal object Resolve()
        {
            return ProvideResult();
        }


        protected abstract object ProvideResult();

        public override string ToString()
        {
            return this.Property.ToString();
        }
    }
}
