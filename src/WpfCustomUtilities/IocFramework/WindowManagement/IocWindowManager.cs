using System.Windows;
using System.Windows.Media;

using WpfCustomUtilities.IocFramework.Application;
using WpfCustomUtilities.IocFramework.Application.Attribute;
using WpfCustomUtilities.IocFramework.WindowManagement.Interface;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.WindowManagement
{
    [IocExport(typeof(IIocWindowManager), InstancePolicy.ShareGlobal)]
    public class IocWindowManager : IIocWindowManager
    {
        // Windows are prepared and held over the lifetime of the user's view
        //
        SimpleDictionary<IIocDialogView, WindowDialogState> _windows;

        protected class WindowDialogState
        {
            public Window Window;
            public bool Showing;
            public bool DialogResult;
        }

        public IocWindowManager()
        {
            _windows = new SimpleDictionary<IIocDialogView, WindowDialogState>();
        }

        public bool ShowDialog<T>(object dataContext) where T : IIocDialogView
        {
            var dialogView = IocContainer.Get<T>();

            dialogView.DataContext = dataContext;

            return ShowDialogImpl<T>(dialogView);
        }

        public bool ShowDialog<T>(T dialogView) where T : IIocDialogView
        {
            return ShowDialogImpl<T>(dialogView);
        }

        private bool ShowDialogImpl<T>(T dialogView) where T : IIocDialogView
        {
            // Existing Window (Use "new" instance)
            if (_windows.ContainsKey(dialogView))
            {
                // Close the current window
                _windows[dialogView].Window.Close();

                // Remove the entry
                _windows.Remove(dialogView);
            }

            dialogView.DialogResultEvent -= OnDialogResult;
            dialogView.DialogResultEvent += OnDialogResult;

            var window = CreateWindow();

            window.Content = dialogView;

            _windows.Add(dialogView, new WindowDialogState()
            {
                Window = window,
                Showing = true
            });

            // SEEMS TO BE OK!  No need for synchronous waiter. This is better as long as 
            // nested windows can be handled
            return (bool)window.ShowDialog();
        }

        private void OnDialogResult(IIocDialogView dialogView, bool result)
        {
            // Finished with the view for now
            dialogView.DialogResultEvent -= OnDialogResult;

            _windows[dialogView].Window.DialogResult = result;
        }

        private Window CreateWindow()
        {
            var window = new Window();

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowStyle = WindowStyle.None;
            window.ShowInTaskbar = false;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            window.AllowsTransparency = true;
            window.Background = Brushes.Transparent;

            return window;
        }
    }
}
