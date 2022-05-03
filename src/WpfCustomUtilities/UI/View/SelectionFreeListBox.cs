using System.Windows.Controls;

namespace WpfCustomUtilities.UI.View
{
    public class SelectionFreeListBox : ListBox
    {
        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
