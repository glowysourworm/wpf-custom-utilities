using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.RecursiveSerializer.IO.Data;
using WpfCustomUtilities.RecursiveSerializer.Planning;

namespace WpfCustomUtilities.RecursiveSerializer.Manifest
{
    [Serializable]
    public class SerializationManifest
    {
        /// <summary>
        /// Dictionary of all hashed types serialized to file. Each represents a UNIQUE object OF TYPE "Type"
        /// </summary>
        public List<SerializedTypeManifest> IgnoredTypes { get; private set; }

        public SerializationManifest() { }

        internal SerializationManifest(DeserializedHeader header)
        {
            var missingTypes = header.MissingTypeTable
                                     .Values
                                     .Select(hashedType => CreateTypeManifest(hashedType))
                                     .ToList();

            var missingProperties = header.ModifiedSpecificationLookup
                                          .Values
                                          .SelectMany(specification => specification.Definitions)
                                          .Select(definition => CreateTypeManifest(definition.PropertyType))
                                          .ToList();

            this.IgnoredTypes = new List<SerializedTypeManifest>();

            this.IgnoredTypes.AddRange(missingTypes);
            this.IgnoredTypes.AddRange(missingProperties);
        }

        private SerializedTypeManifest CreateTypeManifest(HashedType hashedType)
        {
            return new SerializedTypeManifest()
            {
                DeclaringAssembly = hashedType.Declaring.Assembly,
                DeclaringType = hashedType.Declaring.Type,
                HashCode = hashedType.GetHashCode(),
                ImplementingAssembly = hashedType.Implementing.Assembly,
                ImplementingType = hashedType.Implementing.Type
            };
        }
    }
}
