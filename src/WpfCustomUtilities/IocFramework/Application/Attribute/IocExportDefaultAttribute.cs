namespace WpfCustomUtilities.IocFramework.Application.Attribute
{
    /// <summary>
    /// Default Ioc Export attribute. This sets the instance policy to SHARED; and the EXPORT TYPE to the
    /// declaring class type.
    /// </summary>
    // [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IocExportDefaultAttribute : IocExportBaseAttribute
    {
        public IocExportDefaultAttribute() : base(null, InstancePolicy.ShareGlobal)
        {

        }
    }
}
