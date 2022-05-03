namespace WpfCustomUtilities.IocFramework.Application
{
    internal class ModuleInstance
    {
        /// <summary>
        /// The instance of the module that was created by the IocBootstrapper
        /// </summary>
        internal ModuleBase Instance { get; private set; }

        /// <summary>
        /// The module definition defined by the IocBootstrapper
        /// </summary>
        internal ModuleDefinition Definition { get; private set; }

        internal ModuleInstance(ModuleBase instance, ModuleDefinition definition)
        {
            this.Instance = instance;
            this.Definition = definition;
        }
    }
}
