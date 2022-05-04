using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal class DeserializedLeafNode : DeserializedNodeBase
    {
        readonly object _theObject;

        public DeserializedLeafNode(PropertyDefinition definition, HashedType resolvedType, object theObject, SerializationMode mode) : base(definition, resolvedType, mode)
        {
            _theObject = theObject;
        }

        internal override PropertySpecification GetPropertySpecification()
        {
            throw new Exception("Trying to get property definitions for a primitive:  DeserializationPrimitive.cs");
        }

        internal override void Construct(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties)
        {
            throw new Exception("Trying to call constructor on a primitive");
        }

        public override int GetHashCode()
        {
            if (ReferenceEquals(_theObject, null))
                return this.SerializedType.GetHashCode();

            else
                return RecursiveSerializerHashGenerator.CreateSimpleHash(_theObject, this.SerializedType);
        }

        public override bool Equals(object obj)
        {
            var node = obj as DeserializedLeafNode;

            return this.GetHashCode() == node.GetHashCode();
        }

        protected override object ProvideResult()
        {
            return _theObject;
        }
    }
}
