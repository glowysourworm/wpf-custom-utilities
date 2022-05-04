using System.Linq;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.IO.Interface;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Streaming
{
    internal class HeaderSerializer
    {
        readonly ISerializationStreamWriter _writer;

        internal HeaderSerializer(ISerializationStreamWriter writer)
        {
            _writer = writer;
        }

        internal void Serialize(SerializedHeader header)
        {
            // ROOT TYPE
            _writer.Write(header.SerializedType);

            // TYPE TABLE COUNT
            _writer.Write<int>(header.TypeTable.Count);

            // TYPE TABLE
            foreach (var type in header.TypeTable.Values)
                _writer.Write<HashedType>(type);

            // PROPERTY TABLE
            _writer.Write<int>(header.PropertySpecificationGroups.Count);

            foreach (var element in header.PropertySpecificationGroups)
            {
                // PROPERTY SPECIFICATION { Count, PropertyDefinition[] }
                _writer.Write<int>(element.Key.Definitions.Count());

                // PROPERTY SPECIFICATION TYPE (HASH CODE ONLY)
                _writer.Write<int>(element.Key.ObjectType.GetHashCode());

                // PROPERTY SPECIFICATION -> IS USER DEFINED
                _writer.Write<bool>(element.Key.IsUserDefined);

                foreach (var definition in element.Key.Definitions)
                {
                    // PROPERTY NAME
                    _writer.Write<string>(definition.PropertyName);

                    // PROPERTY TYPE (HASH CODE)
                    _writer.Write<int>(definition.PropertyType.GetHashCode());
                }

                // OBJECT REFERENCES
                _writer.Write<int>(element.Value.Count);

                foreach (var objectId in element.Value)
                {
                    // OBJECT ID
                    _writer.Write<int>(objectId);
                }
            }
        }
    }
}
