using System.Windows;
using System.Windows.Controls;

using WpfCustomUtilities.IocFramework.RegionManagement.Wpf;

namespace WpfCustomUtilities.IocFramework.RegionManagement
{
    /// <summary>
    /// Base class to identify a region for the region manager to collect and manage. This has been sealed to
    /// preserve the hash code handling for the IIocRegionManager.
    /// </summary>
    public sealed class IocRegion : ContentControl
    {
        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register("Transition", typeof(Transition), typeof(IocRegion));

        public Transition Transition
        {
            get { return (Transition)GetValue(TransitionProperty); }
            set { SetValue(TransitionProperty, value); }
        }

        public void TransitionTo(FrameworkElement content)
        {
            if (this.Content == content)
                return;

            if (this.Content == null ||
                this.Transition == null)
                this.Content = content;

            else
                this.Transition.BeginTransition(this, this.Content as FrameworkElement, content);
        }

        public override string ToString()
        {
            return this.Name ?? "" + " " + this.GetHashCode().ToString();
        }
    }
}
