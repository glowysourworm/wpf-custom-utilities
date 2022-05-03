using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Manifest;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Interface
{
    internal interface ISerializationStreamReader
    {
        T Read<T>();
        object Read(Type type);

        IEnumerable<SerializedStreamData> GetStreamData();
    }
}
