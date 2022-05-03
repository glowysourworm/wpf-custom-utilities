using System;

namespace WpfCustomUtilities.IocFramework.Application.Attribute
{
    /// <summary>
    /// Can be placed on a public property or field to import an instance of the type when constructing
    /// </summary>
    public class IocImportAttribute : System.Attribute
    {
        /// <summary>
        /// Export key for the import - SHOULD COME FROM THE EXPORT
        /// </summary>
        internal int ExportKey { get; private set; }

        /// <summary>
        /// Type of export to import - SHOULD BE THE TYPE SPECIFIED BY THE SPEICIFIC EXPORT ATTRIBUTE
        /// </summary>
        internal Type ExportType { get; private set; }

        /// <summary>
        /// Creates an import that is keyed by the supplied key. This key must be supplied when re-calling the instance.
        /// </summary>
        public IocImportAttribute(Type exportType, int exportKey)
        {
            this.ExportType = exportType;
            this.ExportKey = exportKey;
        }
    }
}
