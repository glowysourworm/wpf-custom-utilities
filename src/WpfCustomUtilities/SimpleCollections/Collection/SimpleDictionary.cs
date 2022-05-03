using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Interface;

namespace WpfCustomUtilities.SimpleCollections.Collection
{
    /// <summary>
    /// Simple dictionary wrapper to work with SimpleDictionaryExtension. Wrappers utilized for extensibility
    /// purposes.
    /// </summary>
    public class SimpleDictionary<K, V> : Dictionary<K, V>, IRecursiveSerializable
    {
        public SimpleDictionary() : base()
        { }

        public SimpleDictionary(int capacity) : base(capacity)
        { }

        public SimpleDictionary(IDictionary<K, V> dictionary) : base(dictionary)
        { }

        public SimpleDictionary(IPropertyReader reader)
        {
            var count = reader.Read<int>("Count");

            for (int index = 0; index < count; index++)
            {
                var key = reader.Read<K>("Key" + index.ToString());
                var value = reader.Read<V>("Value" + index.ToString());

                Add(key, value);
            }
        }
        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("Count", this.Count);

            var counter = 0;

            foreach (var pair in this)
            {
                writer.Write("Key" + counter.ToString(), pair.Key);
                writer.Write("Value" + counter.ToString(), pair.Value);

                counter++;
            }
        }
    }
}
