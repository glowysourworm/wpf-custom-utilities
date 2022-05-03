﻿using System;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Target;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.RecursiveSerializer.Component
{
    internal static class PropertyWriterFactory
    {
        /// <summary>
        /// Creates a PropertyWriter and resolves properties according to the serialization object. Returned writer can then
        /// be used to query for the property specification and property resolved info collection.
        /// </summary>
        internal static PropertyWriter CreateAndResolve(ImplementingTypeResolver resolver, SerializedNodeBase objectBase)
        {
            if (objectBase is SerializedLeafNode)
                throw new Exception("Trying to resolve properties using PropertyWriter for non-reference serialization object:  PropertyWriterFactory.cs");

            var writer = new PropertyWriter(resolver, objectBase.Type);

            // DEFAULT MODE -> NO SUPPORT FOR COLLECTIONS!
            if (objectBase.Mode == SerializationMode.Default)
            {
                // Initialization complete for collections
                if (objectBase is SerializedCollectionNode)
                    return writer;

                // Initialize writer using reflection
                writer.ReflectProperties(objectBase);
            }

            // SPECIFIED MODE
            else
            {
                try
                {
                    objectBase.MemberInfo.GetMethod.Invoke(objectBase.GetObject(), new object[] { writer });
                }
                catch (Exception innerException)
                {
                    throw new RecursiveSerializerException(objectBase.Type, "Error trying to read properties", innerException);
                }
            }

            return writer;
        }
    }
}
