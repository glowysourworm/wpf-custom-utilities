using System;
using System.Windows;

namespace WpfCustomUtilities.IocFramework.RegionManagement.Interface
{
    /// <summary>
    /// Replacement for IRegionManager for the following reasons:
    /// 
    /// 1) Can't support multiple instances of the same Region / UI branch
    /// 
    /// 2) Issue in the transition presenter somewhere in the libray
    /// 
    /// 3) Want to simplify the implementation and make it specific to region 
    ///    INSTANCE rather than region NAME
    ///    
    /// Using Scoped IRegionManager may have worked if the NEW IRegionManager
    /// was known to the INSTANCE of the control where it was being used. This
    /// would've been essentially the UserControl that had the ContentControl
    /// with the registered prism:RegionManager property.
    /// 
    /// This solution would've made things a lot more coupled because navigation
    /// and management of views would've been handled by some of the views 
    /// themselves. 
    /// 
    /// The way to solve all this is to simplify handling of the region manager in
    /// the application to be centralized. 
    /// 
    /// Instances of IocRegions that are not NAMED can still be loaded by using the
    /// event aggregator. These are passed along by a special event type.
    /// 
    /// </summary>
    public interface IIocRegionManager
    {
        /// <summary>
        /// Returns a IocRegion instance that has been loaded into the IIocRegionManager by using
        /// the RegionName attached property
        /// </summary>
        IocRegion GetRegion(string regionName);

        /// <summary>
        /// Creates a view instance of the specified type in memory and assigns it to
        /// the NAMED region (see IIocRegionManager.LoadNamedInstance). This
        /// will call the public default or importing constructor. Views will otherwise be
        /// instantiated when they're loaded programmatically OR by the ATTACHED PROPERTIES. 
        /// 
        /// THIS DOES NOT LOAD THE VIEW AS CONTENT FOR THE REGION.
        /// </summary>
        /// <param name="regionName">Single instance region with a unique IIocRegionManager.RegionName handle</param>
        /// <param name="viewType">Type of view to pre-register</param>
        void PreRegisterView(string regionName, Type viewType);


        /// <summary>
        /// Loads the IocRegion with the specified view type OR the last view
        /// instance with the same type. Loading is accomplished using the IocContainer
        /// </summary>
        /// <returns>Instance of view</returns>
        FrameworkElement Load(IocRegion region, Type viewType, bool ignoreTransition = false);

        /// <summary>
        /// Loads the default view (instance or type) that has been registered with the 
        /// IocRegion. See attached properties to learn about default views.
        /// </summary>
        FrameworkElement LoadDefaultView(IocRegion region, bool ignoreTransition = false);

        /// <summary>
        /// Finds the NAMED IocRegion instance with the specified name - with the
        /// specified type. Loading is accomplished using the ServiceLocator. The region
        /// name MUST be specified using the IIocRegionManager.RegionName attach property.
        /// </summary>
        /// <returns>Instance of view</returns>
        FrameworkElement LoadNamedInstance(string regionName, Type viewType, bool ignoreTransition = false);

        /// <summary>
        /// Finds the SINGLE UIRegion instance with the specified name - with the
        /// specified type. Loading is accomplished using the ServiceLocator. The region
        /// name MUST be specified using the IRogueRegionManager.RegionName attach property.
        /// </summary>
        /// <returns>Instance of view</returns>
        TView LoadNamedInstance<TView>(string regionName, bool ignoreTransition = false) where TView : FrameworkElement;
    }
}
