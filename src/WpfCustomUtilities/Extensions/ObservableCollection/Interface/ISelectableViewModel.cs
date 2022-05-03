
using WpfCustomUtilities.Extensions.Event;

namespace WpfCustomUtilities.Extensions.ObservableCollection.Interface
{
    public interface ISelectableViewModel
    {
        /// <summary>
        /// Fires when the IsSelected flag has changed
        /// </summary>
        event SimpleEventHandler<ISelectableViewModel> IsSelectedChanged;

        /// <summary>
        /// Signals that this ISelectedViewModel is selected
        /// </summary>
        bool IsSelected { get; set; }
    }
}
