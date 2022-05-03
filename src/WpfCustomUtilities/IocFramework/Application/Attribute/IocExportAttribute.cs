using System;

namespace WpfCustomUtilities.IocFramework.Application.Attribute
{
    /// <summary>
    /// Primary class for the Ioc Exports. This will cover most cases except for keyed export. For these,
    /// see the IocExportSpecific attribute class. The default instance policy is ShareGlobal
    /// </summary>
    // [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IocExportAttribute : IocExportBaseAttribute
    {
        /// <summary>
        /// Constructor that sets the instance policy to SHARED
        /// </summary>
        public IocExportAttribute(Type exportType) : base(exportType, InstancePolicy.ShareGlobal)
        {
        }

        /// <summary>
        /// Constructor for explicit settings for the export. (For the export key setting please use the IocExportSpecific attribute)
        /// </summary>
        public IocExportAttribute(Type exportType, InstancePolicy instancePolicy) : base(exportType, instancePolicy)
        {
        }
    }
}
