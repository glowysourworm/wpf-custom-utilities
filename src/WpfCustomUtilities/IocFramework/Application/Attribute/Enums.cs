namespace WpfCustomUtilities.IocFramework.Application.Attribute
{
    /// <summary>
    /// Specifies how this export will be created from the global instance cache
    /// </summary>
    public enum InstancePolicy
    {
        /// <summary>
        /// The export instance will be SHARED with ALL OTHER exports from this class REGARDLESS
        /// of their export settings. This includes IocExportSpecific types - which will then 
        /// export their OWN type with the SHARED instance injected.
        /// </summary>
        ShareGlobal,

        /// <summary>
        /// The export instance will be shared with other exports of the same EXPORT TYPE ONLY.
        /// </summary>
        ShareExportedType,

        /// <summary>
        /// The export instance FOR THIS EXPORT TYPE will be re-created on EACH CALL to the 
        /// IIocContainer (or during injection).
        /// </summary>
        NonShared
    }
}
