using System;
using System.Collections.Generic;
using System.Reflection;

using WpfCustomUtilities.IocFramework.Application.Attribute;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.IocFramework.Application.InstanceManagement
{
    /// <summary>
    /// Constructs and maintains the reference to the export instance
    /// </summary>
    internal class Export
    {
        /// <summary>
        /// Exported type for the export - this MAY have been set differently to the reflected type
        /// </summary>
        internal Type ExportedType { get; private set; }

        /// <summary>
        /// Reflected type for the export - this should be the actual type constructed
        /// </summary>
        internal Type ReflectedType { get; private set; }

        /// <summary>
        /// Instance policy saved from the export attribute
        /// </summary>
        internal InstancePolicy Policy { get; private set; }

        /// <summary>
        /// The export key saved from the SPECIFIC export attribute
        /// </summary>
        internal int ExportKey { get; private set; }

        /// <summary>
        /// Says whether or not the export was keyed using the SPEIFIC export attribute
        /// </summary>
        internal bool IsExportKeyed { get; private set; }

        /// <summary>
        /// The importing constructor determined by the InstanceFactory for creating the actual object
        /// from the dependencies.
        /// </summary>
        internal ConstructorInfo ImportingCtor { get; private set; }

        /// <summary>
        /// List of dependencies used to create the instance (these are injected to the importing constructor).
        /// NOTE*** THESE ARE PRE-BUILT TO BE INTER-DEPENDENT THE WAY THEY'RE SUPPOSED TO BE IN THE GRAPH.
        /// </summary>
        internal IEnumerable<Export> Dependencies { get; private set; }

        internal Export(ExportKey exportTypeKey, ConstructorInfo importingCtor, IEnumerable<Export> dependencies)
        {
            this.ReflectedType = exportTypeKey.ReflectedType;
            this.ImportingCtor = importingCtor;
            this.Dependencies = dependencies;
            this.ExportedType = exportTypeKey.ExportedType;     // Default export type to the reflected type
            this.ExportKey = exportTypeKey.Key;
            this.IsExportKeyed = exportTypeKey.IsKeyed;
            this.Policy = exportTypeKey.Policy;
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            // Create a base hash code by using the recursive serializer to gather value types from the System.Type
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.ExportedType,
                                                                     this.ReflectedType,
                                                                     this.Policy,
                                                                     this.IsExportKeyed,
                                                                     this.IsExportKeyed ? this.ExportKey : 0);
        }
    }
}
