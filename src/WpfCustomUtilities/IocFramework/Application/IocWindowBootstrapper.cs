using System;
using System.Windows;

using WpfCustomUtilities.IocFramework.Application.IocException;

namespace WpfCustomUtilities.IocFramework.Application
{
    public abstract class IocWindowBootstrapper : IocBootstrapper
    {
        /// <summary>
        /// Defines type for the shell window to be created
        /// </summary>
        public abstract Type DefineShell();

        protected override void UserPreModuleInitialize()
        {
            // Get the type for the user shell to be created
            var shellType = DefineShell();

            if (!typeof(Window).IsAssignableFrom(shellType))
                throw new IocInitializationException("Improper Shell Type {0}. All module types must inherit from Window", shellType.FullName);

            var shell = (Window)IocContainer.Get(shellType);

            // SET SHELL THE MAIN WINDOW OF THE APPLICATION
            System.Windows.Application.Current.MainWindow = shell;
        }

        public override void Run()
        {
            base.Run();

            System.Windows.Application.Current.MainWindow.Show();
        }
    }
}
