using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.RecursiveSerializer.Component;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal class DeserializedCollectionNode : DeserializedObjectNode
    {
        internal List<DeserializedNodeBase> CollectionNodes { get; private set; }

        internal int Count { get { return _count; } }
        internal CollectionInterfaceType InterfaceType { get { return _interfaceType; } }
        internal HashedType ElementType { get { return _elementType; } }

        // Stored data from serialization
        CollectionInterfaceType _interfaceType;
        int _count;
        HashedType _elementType;

        // ACTUAL COLLECTION
        IEnumerable _collection;

        PropertySpecification _specification;

        internal DeserializedCollectionNode(PropertyDefinition definition,
                                            HashedType type,
                                            int referenceId,
                                            PropertySpecification specification,
                                            HashedType elementType,
                                            int count,
                                            CollectionInterfaceType interfaceType,
                                            SerializationMode mode) : base(definition, type, referenceId, specification, mode)
        {
            _count = count;
            _interfaceType = interfaceType;
            _elementType = elementType;
            _specification = specification;

            this.CollectionNodes = new List<DeserializedNodeBase>(count);
        }

        internal override PropertySpecification GetPropertySpecification()
        {
            return _specification;
        }

        internal void FinalizeCollection(RecursiveSerializerMemberInfo memberInfo, IList<PropertyResolvedInfo> resolvedChildren)
        {
            var declaringElementType = RecursiveSerializerTypeFactory.ResolveAsDeclaring(_elementType);

            for (int index = 0; index < _count; index++)
            {
                var element = resolvedChildren[index];

                // VALIDATE ELEMENT TYPE (NOTE*** ELEMENT TYPE IMPLEMENTING TYPE NOT TRACKED!)
                if (!declaringElementType.GetDeclaringType().IsAssignableFrom(element.ResolvedType.GetImplementingType()))
                    throw new Exception("Invalid collection element type: " + element.ResolvedType.ToString());

                switch (this.InterfaceType)
                {
                    case CollectionInterfaceType.Array:
                        (_collection as Array).SetValue(element.ResolvedObject, index); // SET ARRAY VALUE
                        break;
                    case CollectionInterfaceType.IList:
                        (_collection as IList).Add(element.ResolvedObject);             // ADD TO THE LIST
                        break;
                    default:
                        throw new Exception("Unhandled CollectionInterfaceType DeserializedCollectionNode");
                }
            }
        }

        internal override void Construct(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties)
        {
            switch (memberInfo.Mode)
            {
                case SerializationMode.Default:
                    ConstructDefault(memberInfo, resolvedProperties);
                    break;
                case SerializationMode.Specified:
                    ConstructSpecified(memberInfo, resolvedProperties);
                    break;
                default:
                    throw new Exception("Unhandled SerializationMode type:  DeserializationCollection.cs");
            }
        }

        private void ConstructDefault(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties)
        {
            // NO PROPERTY SUPPORT FOR DEFAULT MODE
            if (resolvedProperties.Any())
                throw new RecursiveSerializerException(this.SerializedType, "No property support for DEFAULT mode collections");

            // CONSTRUCT
            try
            {
                // IList + Array (?)
                _collection = memberInfo.ParameterlessConstructor.Invoke(new object[] { }) as IEnumerable;

                if (_collection == null)
                    throw new Exception("Constructor failed for collection of type:  " + this.SerializedType.Declaring.Type);
            }
            catch (Exception ex)
            {
                throw new RecursiveSerializerException(this.SerializedType, "Error constructing from parameterless constructor", ex);
            }
        }

        private void ConstructSpecified(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties)
        {
            var reader = new PropertyReader(resolvedProperties);

            try
            {
                _collection = memberInfo.SpecifiedConstructor.Invoke(new object[] { reader }) as IEnumerable;

                if (_collection == null)
                    throw new Exception("Constructor failed for collection of type:  " + this.SerializedType.ToString());
            }
            catch (Exception ex)
            {
                throw new RecursiveSerializerException(this.SerializedType, "Error constructing from specified constructor: " + memberInfo.SpecifiedConstructor.Name, ex);
            }
        }

        public override int GetHashCode()
        {
            return this.ReferenceId;
        }

        public override bool Equals(object obj)
        {
            var node = obj as DeserializedCollectionNode;

            return this.GetHashCode() == node.GetHashCode();
        }

        protected override object ProvideResult()
        {
            return _collection;
        }
    }
}
