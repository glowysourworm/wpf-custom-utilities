using System;
using System.Linq;
using System.Windows;

using WpfCustomUtilities.IocFramework.Application;
using WpfCustomUtilities.IocFramework.Application.Attribute;
using WpfCustomUtilities.IocFramework.RegionManagement.Interface;
using WpfCustomUtilities.IocFramework.RegionManagement.IocException;

namespace WpfCustomUtilities.IocFramework.RegionManagement
{
    [IocExport(typeof(IIocRegionManager))]
    public class IocRegionManager : IIocRegionManager
    {
        #region Attached Properties
        public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.RegisterAttached("RegionName", typeof(string), typeof(IocRegion));

        public static readonly DependencyProperty DefaultViewTypeProperty =
            DependencyProperty.RegisterAttached("DefaultViewType", typeof(Type), typeof(IocRegion));

        public static readonly DependencyProperty DefaultViewProperty =
            DependencyProperty.RegisterAttached("DefaultView", typeof(FrameworkElement), typeof(IocRegion));

        public static string GetRegionName(UIElement element)
        {
            return (string)element.GetValue(RegionNameProperty);
        }

        public static void SetRegionName(UIElement element, string regionName)
        {
            var region = element as IocRegion;

            // Validate UIRegion container type
            if (region == null)
                throw new IocRegionException("Region content control must inherit (or be an instance of) IocRegion");

            // Search cache for the region (SAFE SEARCH! But checks for dulpicate IocRegions)
            var existingRegion = IocRegionManager.Cache.IsDefined(regionName) ? IocRegionManager.Cache.GetNamedRegion(regionName) : null;

            // New Entry
            if (existingRegion == null)
                IocRegionManager.Cache.AddNamedRegion(regionName, region);

            // Set Dependency Property Value
            element.SetValue(RegionNameProperty, regionName);
        }

        public static Type GetDefaultViewType(UIElement element)
        {
            return (Type)element.GetValue(DefaultViewTypeProperty);
        }

        public static void SetDefaultViewType(UIElement element, Type type)
        {
            var region = element as IocRegion;

            // Validate IocRegion container type
            if (region == null)
                throw new IocRegionException("Region content control must inherit (or be an instance of) IocRegion");

            // Validate View Type as FrameworkElement
            if (!typeof(FrameworkElement).IsAssignableFrom(type))
                throw new IocRegionException("View type must inherit from FrameworkElement");

            // Create View Instance
            var view = (FrameworkElement)IocContainer.Get(type);

            // Existing Region
            if (IocRegionManager.Cache.IsDefined(region))
            {
                // Existing Region -> New View Type
                if (!IocRegionManager.Cache.GetRegionViews(region).Any(x => x.ViewType == type))
                    IocRegionManager.Cache.AddRegionView(region, type, view, true);

                else
                    throw new IocRegionException("View Type already added to the IocRegion. View instance lookup not currently supported");
            }
            // New Region (NO REGION NAME PROPERTY YET ATTACHED!)
            else
            {
                // First, add the region to the cache
                IocRegionManager.Cache.AddRegion(region);

                // Next, register the view
                IocRegionManager.Cache.AddRegionView(region, type, view, true);
            }

            // Load View - use LoadDefaultView method to re-load default view
            LoadImpl(region, view, true);

            // Set Dependency Property Value
            element.SetValue(DefaultViewTypeProperty, type);
        }

        public static FrameworkElement GetDefaultView(UIElement element)
        {
            return (FrameworkElement)element.GetValue(DefaultViewProperty);
        }

        public static void SetDefaultView(UIElement element, FrameworkElement view)
        {
            var region = element as IocRegion;

            // Validate IocRegion container type
            if (region == null)
                throw new IocRegionException("Region content control must inherit (or be an instance of) IocRegion");

            // Existing Region
            if (IocRegionManager.Cache.IsDefined(region))
            {
                // Existing Region -> New View
                if (!IocRegionManager.Cache.GetRegionViews(region).Any(x => x.View == view))
                    IocRegionManager.Cache.AddRegionView(region, view.GetType(), view, true);

                else
                    throw new Exception("View already added to the IocRegion.");
            }
            // New Region (NO REGION NAME PROPERTY YET ATTACHED!)
            else
            {
                // First, add the region to the cache
                IocRegionManager.Cache.AddRegion(region);

                // Next, register the view
                IocRegionManager.Cache.AddRegionView(region, view.GetType(), view, true);
            }

            // Load View - use LoadDefaultView method to re-load default view
            LoadImpl(region, view, true);

            // Set Dependency Property Value
            element.SetValue(DefaultViewProperty, view);
        }
        #endregion

        /// <summary>
        /// Maintains primary list of view instances (MUST BE STATIC TO FACILITATE ATTACHED PROPERTY DESIGN)
        /// </summary>
        internal static RegionCache Cache { get; private set; }

        static IocRegionManager()
        {
            IocRegionManager.Cache = new RegionCache();
        }

        public IocRegionManager() { }

        public IocRegion GetRegion(string regionName)
        {
            // Allow exception if the region is not found
            return IocRegionManager.Cache.GetNamedRegion(regionName);
        }

        public void PreRegisterView(string regionName, Type viewType)
        {
            // Validate View Type as FrameworkElement
            if (!typeof(FrameworkElement).IsAssignableFrom(viewType))
                throw new IocRegionException("View type must inherit from FrameworkElement");

            // Fetch IocRegion
            var region = IocRegionManager.Cache.GetNamedRegion(regionName);

            // Get RegionView collection for the IocRegion
            var regionViews = IocRegionManager.Cache.GetRegionViews(region);

            if (regionViews.Any(x => x.ViewType == viewType))
                throw new IocRegionException("Specified view type {0} already registered with region {1}", viewType, regionName);

            // Create instance of view
            var view = (FrameworkElement)IocContainer.Get(viewType);

            // Set default if there are no other default views
            var isDefault = !regionViews.Any(x => x.IsDefaultView);

            // Add entry to region views
            IocRegionManager.Cache.AddRegionView(region, viewType, view, isDefault);
        }

        public FrameworkElement Load(IocRegion region, Type viewType, bool ignoreTransition = false)
        {
            // Validate View Type as FrameworkElement
            if (!typeof(FrameworkElement).IsAssignableFrom(viewType))
                throw new ArgumentException("View type must inherit from FrameworkElement");

            // NOTE*** Need to call IocContainer to properly deal with the view instance
            //

            // Existing Region
            if (IocRegionManager.Cache.IsDefined(region))
            {
                var regionView = IocRegionManager.Cache.FindRegionView(region, viewType);

                // Load Existing View
                if (regionView != null)
                {
                    // RESET VIEW FROM IOC CONTAINER: Deals with view type instance handling
                    regionView.View = (FrameworkElement)IocContainer.Get(viewType);

                    LoadImpl(region, regionView.View, ignoreTransition);

                    return regionView.View;
                }

                // Create / Load New View
                else
                {
                    // Create view instance
                    var view = (FrameworkElement)IocContainer.Get(viewType);

                    // Store region view entry
                    IocRegionManager.Cache.AddRegionView(region, viewType, view, false);

                    // Load view
                    return LoadImpl(region, view, ignoreTransition);
                }
            }

            // New Region
            else
                throw new IocRegionException("Trying to call IocRegionManager.Load on un-cached region. This IocRegion must first be registered using either the PreRegister method or by the IocRegionManager attached properties");
        }

        public FrameworkElement LoadDefaultView(IocRegion region, bool ignoreTransition = false)
        {
            if (!IocRegionManager.Cache.IsDefined(region))
                throw new IocRegionException("IocRegion not registered with IIocRegionManager");

            var regionView = IocRegionManager.Cache.GetRegionViews(region)
                                                   .SingleOrDefault(x => x.IsDefaultView);

            if (regionView == null)
                throw new IocRegionException("A single default view is allowed for a IocRegion instance. See attached properties");

            // If default view is instantiated - then just load that one
            if (regionView.View != null)
                return LoadImpl(region, regionView.View, ignoreTransition);

            else
                return Load(region, regionView.ViewType, ignoreTransition);
        }

        public FrameworkElement LoadNamedInstance(string regionName, Type viewType, bool ignoreTransition = false)
        {
            // Validate View Type as FrameworkElement
            if (!typeof(FrameworkElement).IsAssignableFrom(viewType))
                throw new IocRegionException("View type must inherit from FrameworkElement");

            // Get the RegionName attached property
            var region = GetRegion(regionName);

            // NOTE*** This will result in the view type being reloaded from the container. This
            //         takes care of export instance handling.
            //

            // RESET VIEW FROM IOC CONTAINER: Deals with view type instance handling
            var view = (FrameworkElement)IocContainer.Get(viewType);

            // Look for SINGLE view with the specified type
            var regionView = IocRegionManager.Cache.GetRegionViews(region)
                                                   .FirstOrDefault(x => x.ViewType == viewType);

            // New RegionView instance
            if (regionView == null)
            {
                // Add region view to the cache
                IocRegionManager.Cache.AddRegionView(region, viewType, view, false);

                // Retrieve new view from the cache
                regionView = IocRegionManager.Cache.FindRegionView(region, viewType);
            }
            else
                regionView.View = view;

            // Load the RegionView - (Allows for new view type)
            return LoadImpl(region, regionView.View, ignoreTransition);
        }

        public TView LoadNamedInstance<TView>(string regionName, bool ignoreTransition = false) where TView : FrameworkElement
        {
            return (TView)LoadNamedInstance(regionName, typeof(TView), ignoreTransition);
        }

        /// <summary>
        /// Shared (with static Attached Property Design code) Load implementation for the UIRegion
        /// </summary>
        /// <param name="region">IocRegion instance</param>
        /// <param name="view">UI View Content to load</param>
        protected static FrameworkElement LoadImpl(IocRegion region, FrameworkElement view, bool ignoreTransition)
        {
            if (!ignoreTransition)
                region.TransitionTo(view);

            else
                region.Content = view;

            return view;
        }
    }
}
