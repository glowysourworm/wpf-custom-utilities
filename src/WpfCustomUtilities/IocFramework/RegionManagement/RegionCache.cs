using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using WpfCustomUtilities.IocFramework.RegionManagement.IocException;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.RegionManagement
{
    /// <summary>
    /// Primary region cache - contains list of IocRegions and their lists of RegionView instances
    /// </summary>
    internal class RegionCache
    {
        /// <summary>
        /// Maintains primary list of NAMED ioc regions (MUST BE STATIC TO FACILITATE ATTACHED PROPERTY DESIGN).
        /// NOTE*** The string key is the IocRegionManager.RegionName attached property. This must be assigned
        ///         by the IocRegionManager.
        /// </summary>
        private static SimpleDictionary<string, IocRegion> NamedIocRegions;

        /// <summary>
        /// Maintains primary list of view instances (MUST BE STATIC TO FACILITATE ATTACHED PROPERTY DESIGN)
        /// </summary>
        private static SimpleDictionary<IocRegion, List<RegionView>> IocRegionViews;

        static RegionCache()
        {
            NamedIocRegions = new SimpleDictionary<string, IocRegion>();
            IocRegionViews = new SimpleDictionary<IocRegion, List<RegionView>>();
        }

        internal RegionCache()
        { }

        internal IocRegion GetNamedRegion(string regionName)
        {
            // IocRegion must exist!
            if (!NamedIocRegions.ContainsKey(regionName))
                throw new IocRegionException("Missing region with IocRegionManager.RegionName = {0}", regionName);

            return NamedIocRegions[regionName];
        }

        internal IEnumerable<RegionView> GetRegionViews(IocRegion region)
        {
            // IocRegion must be cached!
            if (!IocRegionViews.ContainsKey(region))
                throw new IocRegionException("Missing ioc region with IocRegion.Name = {0}", region.Name);

            return IocRegionViews[region];
        }

        /// <summary>
        /// Returns true if NAMED region is defined in the cache
        /// </summary>
        internal bool IsDefined(string regionName)
        {
            return NamedIocRegions.ContainsKey(regionName);
        }

        /// <summary>
        /// Returns true if region is defined in the cache
        /// </summary>
        internal bool IsDefined(IocRegion region)
        {
            return IocRegionViews.ContainsKey(region);
        }

        /// <summary>
        /// Searches for the first-or-default region view of the provided type
        /// </summary>
        /// <exception cref="IocRegionException">Throws an exception for a missing cached IocRegion</exception>
        internal RegionView FindRegionView(IocRegion region, Type viewType)
        {
            // IocRegion must exist in the cache!
            if (!IocRegionViews.ContainsKey(region))
                throw new IocRegionException("Missing IocRegion for view type {0}", viewType);

            return IocRegionViews[region].FirstOrDefault(x => x.ViewType == viewType);
        }

        internal void AddRegion(IocRegion region)
        {
            // Check for existing entry
            if (IocRegionViews.ContainsKey(region))
                throw new IocRegionException("IocRegion already registered with RegionCache");

            IocRegionViews.Add(region, new List<RegionView>());
        }

        internal void AddNamedRegion(string regionName, IocRegion region)
        {
            // Check for existing entry
            if (NamedIocRegions.ContainsKey(regionName))
                throw new IocRegionException("Duplicate IocRegion instance found for RegionName = {0}.", regionName);

            // Check for existing entry
            if (IocRegionViews.ContainsKey(region))
                throw new IocRegionException("Duplicate IocRegion instance found for region with Name = {0}", region.Name);

            // Add NAMED Region
            NamedIocRegions.Add(regionName, region);

            // Also, add region to full list
            IocRegionViews.Add(region, new List<RegionView>());
        }

        /// <summary>
        /// Adds to the cache of region views for the specified region. If none exists, Then add a new entry for the
        /// IocRegion.
        /// </summary>
        internal void AddRegionView(IocRegion region, Type viewType, FrameworkElement view, bool isDefault)
        {
            // Existing Region
            if (IocRegionViews.ContainsKey(region))
            {
                // Existing Entry -> Existing View (or) View Type
                if (IocRegionViews[region].Any(x => x.ViewType == viewType || x.View == view))
                    throw new IocRegionException("Specified view type {0} already registered with region", viewType);
            }

            // New Region
            else
                throw new IocRegionException("Trying to add a view to a region that hasn't been registered with the RegionCache");

            // Add the view to the region
            IocRegionViews[region].Add(new RegionView(viewType, view, isDefault));
        }
    }
}
