using System.Windows.Input;

namespace WpfCustomUtilities.Extensions.Command
{
    public interface ISimpleAsyncCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
