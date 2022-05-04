using System.Windows.Controls;
using System.Windows.Media;

using WpfCustomUtilities.RecursiveSerializer.Compare.ViewModel;

namespace WpfCustomUtilities.RecursiveSerializer.Compare.View
{
    public class TypeTextBlock : TextBlock
    {
        readonly static Brush _missingBackground;
        readonly static Brush _missingForeground;
        readonly static Brush _modifiedBackground;
        readonly static Brush _modifiedForeground;

        static TypeTextBlock()
        {
            _missingBackground = new SolidColorBrush(Color.FromScRgb(0.3f, 1, 0, 0));
            _missingForeground = Brushes.Black;
            _modifiedBackground = new SolidColorBrush(Color.FromScRgb(0.3f, Colors.Beige.ScR, Colors.Beige.ScG, Colors.Beige.ScB));
            _modifiedForeground = Brushes.Black;

            _missingBackground.Freeze();
            _modifiedBackground.Freeze();
        }

        public TypeTextBlock()
        {
            this.DataContextChanged += (sender, e) =>
            {
                if (e.NewValue != null &&
                    e.NewValue is TypeViewModel)
                {
                    var viewModel = (TypeViewModel)e.NewValue;

                    this.Text = string.Format("{0}{2} ({1})", viewModel.Name,
                                                              viewModel.Assembly,
                                                              viewModel.Resolved ? " R" :
                                                              viewModel.Missing ? " M" :
                                                              viewModel.Modified ? " Mo" : "");

                    if (viewModel.Missing)
                    {
                        this.Background = _missingBackground;
                        this.Foreground = _missingForeground;
                    }
                    else if (viewModel.Modified)
                    {
                        this.Background = _modifiedBackground;
                        this.Foreground = _modifiedForeground;
                    }
                    else
                    {
                        this.Background = Brushes.Transparent;
                        this.Foreground = Brushes.Black;
                    }
                }
            };
        }
    }
}
