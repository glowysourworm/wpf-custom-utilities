using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Component;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal class DeserializedObjectNode : DeserializedNodeBase
    {
        internal List<DeserializedNodeBase> SubNodes { get; private set; }

        /// <summary>
        /// ID FROM SERIALIZATION PROCEDURE
        /// </summary>
        internal int ReferenceId { get; private set; }

        // Property definitions
        readonly PropertySpecification _specification;

        // Object being constructed
        private object _defaultObject;

        internal DeserializedObjectNode(PropertyDefinition definition,
                                        HashedType type, int referenceId,
                                        PropertySpecification specification,
                                        SerializationMode mode) : base(definition, type, mode)
        {
            this.ReferenceId = referenceId;
            this.SubNodes = new List<DeserializedNodeBase>();

            _specification = specification;
        }

        internal override PropertySpecification GetPropertySpecification()
        {
            return _specification;
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
                    throw new Exception("Unhandled SerializationMode type:  DeserializationObject.cs");
            }
        }

        private void ConstructDefault(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties)
        {
            // CONSTRUCT
            try
            {
                _defaultObject = memberInfo.ParameterlessConstructor.Invoke(new object[] { });
            }
            catch (Exception ex)
            {
                throw new RecursiveSerializerException(this.SerializedType, "Error constructing from parameterless constructor", ex);
            }

            // SET PROPERTIES
            try
            {
                foreach (var property in resolvedProperties)
                {
                    if (_specification.IsUserDefined)
                        throw new RecursiveSerializerException(property.ResolvedType, "Trying to set user defined property using DEFAULT mode");

                    // SAFE TO CALL
                    property.GetReflectedInfo()
                            .SetValue(_defaultObject, property.ResolvedObject);
                }
            }
            catch (Exception ex)
            {
                throw new RecursiveSerializerException(this.SerializedType, "Error constructing object from properties", ex);
            }
        }

        private void ConstructSpecified(RecursiveSerializerMemberInfo memberInfo, IEnumerable<PropertyResolvedInfo> resolvedProperties)
        {
            var reader = new PropertyReader(resolvedProperties);

            try
            {
                _defaultObject = memberInfo.SpecifiedConstructor.Invoke(new object[] { reader });
            }
            catch (Exception ex)
            {
                throw new RecursiveSerializerException(this.SerializedType, "Error constructing from specified constructor", ex);
            }
        }

        public override bool Equals(object obj)
        {
            var node = obj as DeserializedObjectNode;

            return this.ReferenceId == node.ReferenceId;
        }
        public override int GetHashCode()
        {
            return this.ReferenceId;
        }

        protected override object ProvideResult()
        {
            return _defaultObject;
        }
    }
}
