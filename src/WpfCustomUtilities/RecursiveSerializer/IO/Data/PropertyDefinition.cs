
using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.RecursiveSerializer.IO.Data
{
    /// <summary>
    /// Used for pre-planning Deserialization
    /// </summary>
    internal class PropertyDefinition
    {
        public static PropertyDefinition RootNode = new PropertyDefinition()
        {
            PropertyName = "ROOT NODE",
            PropertyType = RecursiveSerializerTypeFactory.Build(typeof(object))
        };

        public static PropertyDefinition CollectionElement = new PropertyDefinition()
        {
            PropertyName = "COLLECTION ELEMENT",
            PropertyType = RecursiveSerializerTypeFactory.Build(typeof(object))
        };

        public static PropertyDefinition Empty = new PropertyDefinition()
        {
            PropertyName = "EMPTY",
            PropertyType = RecursiveSerializerTypeFactory.Build(typeof(object))
        };

        internal PropertyDefinition()
        {
        }

        internal string PropertyName { get; set; }
        internal HashedType PropertyType { get; set; }

        public override bool Equals(object obj)
        {
            var definition = obj as PropertyDefinition;

            return definition.PropertyName.Equals(this.PropertyName) &&
                   definition.PropertyType.Equals(this.PropertyType);
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.PropertyName, this.PropertyType);
        }

        public override string ToString()
        {
            return string.Format("Name={0}, Type={1}", this.PropertyName, this.PropertyType);
        }
    }
}
