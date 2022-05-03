using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using WpfCustomUtilities.IocFramework.Application.Attribute;
using WpfCustomUtilities.IocFramework.Application.InstanceManagement;
using WpfCustomUtilities.IocFramework.Application.IocException;

namespace WpfCustomUtilities.IocFramework.Application
{
    public static class IocContainer
    {
        /// <summary>
        /// NOTE*** This can't be used until the modules are reflected into the bootstrapper; and the export factory is called.
        /// </summary>
        internal static ExportCache ExportCache { get; private set; }
        internal static InstanceCache InstanceCache { get; private set; }

        /// <summary>
        /// Initializes the container by building the export cache
        /// </summary>
        /// <param name="assemblies">Assemblies to be supported by the container</param>
        public static void Initialize(IEnumerable<Assembly> assemblies)
        {
            IocContainer.ExportCache = ExportFactory.CreateCache(assemblies);
            IocContainer.InstanceCache = new InstanceCache();
        }

        /// <summary>
        /// NOTE*** The parameters for the export must be provided AS DEFINED IN THE EXPORT ATTRIBUTE.
        /// </summary>
        public static T Get<T>()
        {
            return (T)Get(typeof(T), default(int), false);
        }

        /// <summary>
        /// NOTE*** The parameters for the export must be provided AS DEFINED IN THE EXPORT ATTRIBUTE.
        /// </summary>
        public static T Get<T>(int exportKey)
        {
            return (T)Get(typeof(T), exportKey, true);
        }

        /// <summary>
        /// NOTE*** The parameters for the export must be provided AS DEFINED IN THE EXPORT ATTRIBUTE.
        /// </summary>
        public static object Get(Type exportType)
        {
            return Get(exportType, default(int), false);
        }

        /// <summary>
        /// NOTE*** The parameters for the export must be provided AS DEFINED IN THE EXPORT ATTRIBUTE.
        /// </summary>
        public static object Get(Type exportType, int exportKey)
        {
            return Get(exportType, exportKey, true);
        }

        /// <summary>
        /// NOTE*** The parameters for the export are from the USER END. So, this export must be located
        ///         by using the export parameters (somehow)
        /// </summary>
        public static object Get(Type exportType, int exportKey, bool isKeyed)
        {
            // Fetch export for the instance
            var export = IocContainer.ExportCache.FindExport(exportType, exportKey, isKeyed);

            // Check cache (NON-SHARED)
            if (IocContainer.InstanceCache.HasInstance(export) &&
                IocContainer.InstanceCache[export].IsReady())
                return IocContainer.InstanceCache[export].Current;

            // Check shared exports (for same instance policy!)
            var sharedExport = IocContainer.ExportCache.FindSharedExport(export.ReflectedType, export.ExportedType, export.Policy);

            // Check cache (SHARED)
            if (sharedExport != null &&
                IocContainer.InstanceCache.HasInstance(sharedExport) &&
                IocContainer.InstanceCache[sharedExport].IsReady())
                return IocContainer.InstanceCache[sharedExport].Current;

            // --- BUILD INSTANCE --- 

            // Ensure it's ready (see implementation)
            var instance = sharedExport != null ? BuildInstance(export, sharedExport) : BuildInstance(export);

            // Cache instance
            //
            // SHARED
            if (sharedExport != null)
                IocContainer.InstanceCache.UpdateInstance(sharedExport, instance, true);

            // NON-SHARED
            else
                IocContainer.InstanceCache.UpdateInstance(export, instance, true);

            return instance;
        }

        private static object BuildInstance(Export export, SharedExport sharedExport)
        {
            // Procedure (SHARED)
            //
            // 0) Check the build rules + IsReady flag to determine if instance is constructed already
            // 1) First, ensure dependencies are constructed (RECURSIVELY)
            //      - HAVE TO RE-CHECK THE BUILD RULE!
            // 2) Construct THIS instance (based on the rules)
            //

            // Check SHARED instance for readiness based on the build rules
            if (IocContainer.InstanceCache.HasInstance(sharedExport) &&
                IocContainer.InstanceCache[sharedExport].IsReady() &&
                !ShouldRebuild(sharedExport))
                return IocContainer.InstanceCache[sharedExport].Current;

            // EXPORT Dependencies
            foreach (var dependency in export.Dependencies)
            {
                // Check for SHARED EXPORTS (for same instance policy!)
                var dependencyShared = IocContainer.ExportCache.FindSharedExport(dependency.ReflectedType, dependency.ExportedType, dependency.Policy);

                // NOT READY (OR) FAILS REBUILD RULE
                if (!IocContainer.InstanceCache.HasInstance(dependency) || !IocContainer.InstanceCache[dependency].IsReady() || ShouldRebuild(dependency))
                {
                    // SHARED
                    if (dependencyShared != null)
                        IocContainer.InstanceCache.UpdateInstance(dependencyShared, BuildInstance(dependency, dependencyShared), true);

                    // NON-SHARED
                    else
                        IocContainer.InstanceCache.UpdateInstance(dependency, BuildInstance(dependency), true);
                }
            }

            // Finally, rebuild THIS instance
            return Construct(export);
        }

        private static object BuildInstance(Export export)
        {
            // Procedure (NON-SHARED)
            //
            // 0) Check the build rules + IsReady flag to determine if instance is constructed already
            // 1) First, ensure dependencies are constructed (RECURSIVELY) (1st generation cycles have been validated out!)
            //      - HAVE TO RE-CHECK THE BUILD RULE!
            // 2) Construct THIS instance (based on the rules)

            // Check NON-SHARED instance for readiness based on the build rules
            if (IocContainer.InstanceCache.HasInstance(export) &&
                IocContainer.InstanceCache[export].IsReady()
                && !ShouldRebuild(export))
                return IocContainer.InstanceCache[export].Current;

            // Dependencies
            foreach (var dependency in export.Dependencies)
            {
                // Check shared exports (for same instance policy!)
                var dependencyShared = IocContainer.ExportCache.FindSharedExport(dependency.ReflectedType, dependency.ExportedType, dependency.Policy);

                // NOT READY (OR) FAILS REBUILD RULE
                if (!IocContainer.InstanceCache.HasInstance(dependency) || !IocContainer.InstanceCache[dependency].IsReady() || ShouldRebuild(dependency))
                {
                    // SHARED
                    if (dependencyShared != null)
                        IocContainer.InstanceCache.UpdateInstance(dependencyShared, BuildInstance(dependency, dependencyShared), true);

                    // NON-SHARED
                    else
                        IocContainer.InstanceCache.UpdateInstance(dependency, BuildInstance(dependency), true);
                }
            }

            // Finally, rebuild THIS instance
            return Construct(export);
        }

        private static bool ShouldRebuild(Export export)
        {
            // Export Rules (RECURSIVE): Rebuild if the export SHOULD be rebuilt
            //
            // 1) If THIS export is shared - 
            //

            if (!IocContainer.InstanceCache.HasInstance(export))
                throw new IocExportException(export, "No instance found for export");

            switch (export.Policy)
            {
                case InstancePolicy.ShareGlobal:
                {
                    return !InstanceCache[export].IsReady();
                }
                case InstancePolicy.ShareExportedType:
                {
                    return !InstanceCache[export].IsReady();
                }
                case InstancePolicy.NonShared:
                {
                    return true;
                }
                default:
                    throw new Exception("Unhandled InstancePolicy Export.cs");
            }
        }

        private static bool ShouldRebuild(SharedExport sharedExport)
        {
            // Export Rules (RECURSIVE): Rebuild if the export SHOULD be rebuilt
            //
            // 1) If THIS export is shared - 
            //

            if (!IocContainer.InstanceCache.HasInstance(sharedExport))
                throw new IocExportException(sharedExport, "No instance found for export");

            switch (sharedExport.Exports.First().Policy)
            {
                case InstancePolicy.ShareGlobal:
                {
                    return !InstanceCache[sharedExport].IsReady();
                }
                case InstancePolicy.ShareExportedType:
                {
                    return !InstanceCache[sharedExport].IsReady();
                }
                case InstancePolicy.NonShared:
                {
                    return true;
                }
                default:
                    throw new Exception("Unhandled InstancePolicy Export.cs");
            }
        }

        private static object Construct(Export export)
        {
            // Construct THIS instance
            try
            {
                // Select the dependency values
                var dependencies = export.Dependencies
                                         .Select(dependency =>
                                         {
                                             var sharedExport = IocContainer.ExportCache.FindSharedExport(dependency.ReflectedType, dependency.ExportedType, dependency.Policy);

                                             // SHARED
                                             if (sharedExport != null)
                                                 return IocContainer.InstanceCache[sharedExport].Current;

                                             // NON-SHARED
                                             else
                                                 return IocContainer.InstanceCache[dependency].Current;

                                         }).ToArray();

                // Construct this instance
                return export.ImportingCtor.Invoke(dependencies);
            }
            catch (Exception ex)
            {
                throw new IocExportException(new ExportKey(export.ReflectedType, export.ExportedType, export.Policy, export.ExportKey, export.IsExportKeyed),
                                               "Error constructing instance of type {0}", ex, export.ExportedType);
            }
        }
    }
}
