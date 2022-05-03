using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.Target
{
    internal class SerializedObjectNode : SerializedNodeBase
    {
        internal List<SerializedNodeBase> SubNodes { get; private set; }

        readonly object _theObject;

        internal SerializedObjectNode(SerializedNodeType nodeType, ResolvedHashedType resolvedType, object theObject, RecursiveSerializerMemberInfo memberInfo)
                    : base(nodeType, resolvedType, memberInfo)
        {
            _theObject = theObject;

            this.SubNodes = new List<SerializedNodeBase>();
        }

        internal override bool RepresentsNullReference()
        {
            return ReferenceEquals(_theObject, null);
        }

        internal override object GetObject()
        {
            return _theObject;
        }
    }
}
