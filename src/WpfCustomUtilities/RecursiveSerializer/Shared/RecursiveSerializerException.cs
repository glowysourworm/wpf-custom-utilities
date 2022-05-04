using System;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    public class RecursiveSerializerException : Exception
    {
        internal HashedType Type { get; private set; }
        internal ResolvedHashedType ResolvedType { get; private set; }

        readonly static string BASE_MESSAGE = "Recursive Serialization Exception: {0} {1}";

        internal RecursiveSerializerException(HashedType type)
                    : base(string.Format(BASE_MESSAGE, type.Declaring.Type), new RecursiveSerializerInnerException())
        {
            this.Type = type;
        }
        internal RecursiveSerializerException(ResolvedHashedType type)
            : base(string.Format(BASE_MESSAGE, type.GetDeclaringType().Name, ""), new RecursiveSerializerInnerException())
        {
            this.ResolvedType = type;
        }
        internal RecursiveSerializerException(HashedType type, string message)
            : base(string.Format(BASE_MESSAGE, type.Declaring.Type, message), new RecursiveSerializerInnerException())
        {
            this.Type = type;
        }
        internal RecursiveSerializerException(ResolvedHashedType type, string message)
            : base(string.Format(BASE_MESSAGE, type.GetDeclaringType().Name, message), new RecursiveSerializerInnerException())
        {
            this.ResolvedType = type;
        }
        internal RecursiveSerializerException(HashedType type, string message, Exception innerException)
            : base(string.Format(BASE_MESSAGE, type, message), new RecursiveSerializerInnerException(innerException))
        {
            this.Type = type;
        }
        internal RecursiveSerializerException(ResolvedHashedType type, string message, Exception innerException)
            : base(string.Format(BASE_MESSAGE, type.GetDeclaringType().Name, message), new RecursiveSerializerInnerException(innerException))
        {
            this.ResolvedType = type;
        }
    }
}
