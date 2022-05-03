using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Manifest;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Interface
{
    internal interface ISerializationStreamWriter
    {
        void Write<T>(T theObject);
        void Write(object theObject, Type theObjectType);

        IEnumerable<SerializedStreamData> GetStreamData();
    }
}
