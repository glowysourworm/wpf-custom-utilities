using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal class DeserializedNullLeafNode : DeserializedNodeBase
    {
        internal DeserializedNullLeafNode(PropertyDefinition definition, HashedType resolvedType, SerializationMode mode) : base(definition, resolvedType, mode)
        {
        }

        internal override PropertySpecification GetPropertySpecification()
        {
            throw new Exception("Trying to get property definitions for a null refereence:  DeserializationNullLeafNode.cs");
        }

        internal override void Construct(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties)
        {
            throw new Exception("Trying to construct a null reference:  DeserializationNullLeafNode.cs");
        }

        protected override object ProvideResult()
        {
            throw new Exception("Trying to provide result from a null reference:  DeserializationNullLeafNode.cs");
        }

        public override bool Equals(object obj)
        {
            var node = obj as DeserializedNullLeafNode;

            return this.GetHashCode() == node.GetHashCode();
        }
        public override int GetHashCode()
        {
            return this.SerializedType.GetHashCode();
        }
    }
}
