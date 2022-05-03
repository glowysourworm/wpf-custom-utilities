using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace WpfCustomUtilities.IocFramework.RegionManagement.Wpf
{
    public class FadeTransition : Transition
    {
        public Duration Time { get; set; }

        public FadeTransition()
        {
            this.Time = new Duration(new TimeSpan(0, 0, 0, 0, 300));
        }

        public override void BeginTransition(IocRegion transitionElement, FrameworkElement oldContent, FrameworkElement newContent)
        {
            var fadeOutAnimation = new DoubleAnimation(1.0, 0.0, this.Time);

            fadeOutAnimation.Completed += (obj, e) =>
            {
                oldContent.BeginAnimation(FrameworkElement.OpacityProperty, null);

                EndTransition(transitionElement, oldContent, newContent);
            };

            oldContent.BeginAnimation(FrameworkElement.OpacityProperty, fadeOutAnimation);

        }
        public override void EndTransition(IocRegion transitionElement, FrameworkElement oldContent, FrameworkElement newContent)
        {
            transitionElement.Content = newContent;

            oldContent.Opacity = 1.0;
        }
    }
}
