using System.Collections.Generic;

namespace WpfCustomUtilities.RecursiveSerializer.Shared.Data
{
    /// <summary>
    /// Class that describes a serialized type
    /// </summary>
    public class SerializedTypeSpecification
    {
        public TypeInfo DeclaredType { get; set; }
        public TypeInfo ImplementingType { get; set; }

        /// <summary>
        /// Resolved definition collection. These are resolved against loaded assemblies.
        /// </summary>
        public IEnumerable<PropertyInfo> ResolvedDefinitions { get; private set; }

        /// <summary>
        /// Extra property definitions - Not found in the user's entries.
        /// </summary>
        public IEnumerable<PropertyInfo> ExtraDefinitions { get; private set; }

        /// <summary>
        /// Extra property definitions sent by the constructor. These were found on deserializing
        /// </summary>
        public IEnumerable<PropertyInfo> MissingDefinitions { get; private set; }

        public SerializedTypeSpecification(TypeInfo declaring, TypeInfo implementing,
                                           IEnumerable<PropertyInfo> resolvedDefinitions,
                                           IEnumerable<PropertyInfo> extraDefinitions,
                                           IEnumerable<PropertyInfo> missingDefinitions)
        {
            this.DeclaredType = declaring;
            this.ImplementingType = implementing;
            this.ResolvedDefinitions = resolvedDefinitions;
            this.ExtraDefinitions = extraDefinitions;
            this.MissingDefinitions = missingDefinitions;
        }
    }
}
