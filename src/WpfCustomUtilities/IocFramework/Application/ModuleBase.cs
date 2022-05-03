using WpfCustomUtilities.IocFramework.EventAggregation;
using WpfCustomUtilities.IocFramework.RegionManagement.Interface;

namespace WpfCustomUtilities.IocFramework.Application
{
    public abstract class ModuleBase
    {
        /// <summary>
        /// Region manager for the module - injected by the boostrapper directly
        /// </summary>
        public IIocRegionManager RegionManager { get; private set; }

        /// <summary>
        /// Event aggregator for the module - injected by the boostrapper directly
        /// </summary>
        public IIocEventAggregator EventAggregator { get; private set; }

        /// <summary>
        /// Ctor intended to be directly called by the bootstrapper. This happens automatically
        /// by design from the base bootstrapper class.
        /// </summary>
        public ModuleBase(IIocRegionManager regionManager, IIocEventAggregator eventAggregator)
        {
            this.RegionManager = regionManager;
            this.EventAggregator = eventAggregator;
        }

        /// <summary>
        /// Override this to apply your own custom initialization procedures. Typical procedures are
        /// pre-loading region views and any other data resources.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Override this to create an entry point for the module
        /// </summary>
        public virtual void Run()
        {

        }
    }
}
