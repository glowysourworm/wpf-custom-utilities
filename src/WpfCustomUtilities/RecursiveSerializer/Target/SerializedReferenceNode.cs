using System;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal class SerializedReferenceNode : SerializedNodeBase
    {
        internal int ReferenceId { get; private set; }

        /// <summary>
        /// Constructor for NULL, NULL PRIMITIVE types
        /// </summary>
        internal SerializedReferenceNode(int referenceId, ResolvedHashedType resolvedType, RecursiveSerializerMemberInfo memberInfo)
                    : base(SerializedNodeType.Reference, resolvedType, memberInfo)
        {
            this.ReferenceId = referenceId;
        }

        internal override bool RepresentsNullReference()
        {
            throw new Exception("Trying to access reference node object");
        }

        internal override object GetObject()
        {
            throw new Exception("Trying to access reference node object");
        }
    }
}
