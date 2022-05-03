using System;
using System.Windows;

namespace WpfCustomUtilities.IocFramework.RegionManagement
{
    /// <summary>
    /// class for maintaining region view instances
    /// </summary>
    internal class RegionView
    {
        internal FrameworkElement View { get; set; }
        internal Type ViewType { get; set; }
        internal bool IsDefaultView { get; set; }

        public override string ToString()
        {
            return "[" + this.View?.GetHashCode().ToString() + "] " + this.ViewType.ToString();
        }

        internal RegionView(Type viewType, FrameworkElement view, bool isDefaultView = false)
        {
            this.ViewType = viewType;
            this.View = view;
            this.IsDefaultView = isDefaultView;
        }
    }
}
