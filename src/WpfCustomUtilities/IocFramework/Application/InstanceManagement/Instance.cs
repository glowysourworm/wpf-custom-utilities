using WpfCustomUtilities.IocFramework.Application.IocException;

namespace WpfCustomUtilities.IocFramework.Application.InstanceManagement
{
    internal class Instance
    {
        /// <summary>
        /// Current instance object for the export
        /// </summary>
        internal object Current { get; private set; }

        internal Instance(object current)
        {
            this.Current = current;
        }

        internal bool IsReady()
        {
            return !ReferenceEquals(this.Current, null);
        }

        internal void Update(object instance, bool overwrite)
        {
            if (ReferenceEquals(this.Current, null) && !overwrite)
                throw new IocInitializationException("Trying to un-intentionally overwrite export:  Instance.cs");

            this.Current = instance;
        }
    }
}
