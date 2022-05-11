using WpfCustomUtilities.Extensions.Event;

namespace WpfCustomUtilities.IocFramework.WindowManagement.Interface
{
    /// <summary>
    /// Component that plugs into the IIocWindowManager as a view for a dialog window
    /// </summary>
    public interface IIocDialogView
    {
        // Listener Event
        event SimpleEventHandler<IIocDialogView, bool> DialogResultEvent;

        /// <summary>
        /// Should be provided by WPF
        /// </summary>
        object DataContext { get; set; }
    }
}
