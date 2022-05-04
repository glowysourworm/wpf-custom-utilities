using System;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Interface;
using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Target;

namespace WpfCustomUtilities.RecursiveSerializer.Shared
{
    /// <summary>
    /// Class to wrap a System.Type object and provide a reference-like wrapper that is serializable
    /// </summary>
    public class SerializableType : IRecursiveSerializable
    {
        // DECLARING ONLY
        ResolvedHashedType _instance;

        public Type Instance { get { return _instance.GetDeclaringType(); } }

        public SerializableType(Type type)
        {
            // CAREFUL:  THIS EXPOSES THE STATIC COLLECTIONS (NOT-THREAD-SAFE)
            _instance = RecursiveSerializerTypeFactory.BuildAndResolve(type);
        }

        public SerializableType(IPropertyReader reader)
        {
            // TREATED AS PRIMITIVE IN THE SERIALIZER NAMESPACE
            var hashedType = reader.Read<HashedType>("Type");

            _instance = RecursiveSerializerTypeFactory.ResolveAsActual(hashedType);
        }

        public void GetProperties(IPropertyWriter writer)
        {
            // TREATED AS PRIMITIVE IN THE SERIALIZER NAMESPACE
            var hashedType = RecursiveSerializerTypeFactory.Build(_instance.GetDeclaringType());

            writer.Write("Type", hashedType);
        }
    }
}
