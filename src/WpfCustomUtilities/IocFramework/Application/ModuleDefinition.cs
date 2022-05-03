using System;

namespace WpfCustomUtilities.IocFramework.Application
{
    public class ModuleDefinition
    {
        public string Name { get; private set; }
        public Type ModuleType { get; private set; }
        public bool IsEntryPoint { get; private set; }

        /// <summary>
        /// Defines a module with the specified name, type and flag to call Run() (entry point)
        /// </summary>
        /// <param name="moduleType">The type of module - MUST DERIVE FROM ModuleBase</param>
        /// <param name="isEntryPoint">Specifies that the module is the entry point for the IocBootstrapper</param>
        public ModuleDefinition(string name, Type moduleType, bool isEntryPoint)
        {
            this.Name = name;
            this.ModuleType = moduleType;
            this.IsEntryPoint = isEntryPoint;
        }
    }
}
