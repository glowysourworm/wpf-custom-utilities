using System;

namespace WpfCustomUtilities.IocFramework.Application.Attribute
{
    /// <summary>
    /// Abstract base class for the Ioc Exports. THIS DEFAULTS THE INSTANCE POLICY TO SHARED
    /// </summary>
    // [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class IocExportBaseAttribute : System.Attribute
    {
        /// <summary>
        /// Returns the export type - which is null for the attribute until it is resolved by the 
        /// export factory.
        /// </summary>
        internal Type? ExportType { get; private set; }

        /// <summary>
        /// Returns the instance policy for the export. This is shared by default.
        /// </summary>
        internal InstancePolicy InstancePolicy { get; private set; }

        /// <summary>
        /// Creates an export that is keyed by the supplied key. This key must be supplied when re-calling the instance.
        /// </summary>
        public IocExportBaseAttribute(Type exportType, InstancePolicy instancePolicy)
        {
            this.ExportType = exportType;
            this.InstancePolicy = instancePolicy;
        }
    }
}
