
using System;
using System.Linq;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.Extensions.Collection;
using WpfCustomUtilities.IocFramework.Application.Attribute;
using WpfCustomUtilities.IocFramework.Application.IocException;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.Application.InstanceManagement
{
    internal class ExportCache
    {
        // Cache of exports (shared and non-shared)
        readonly SimpleDictionary<ExportKey, Export> _exports;

        // Cache of shared export containers
        readonly SimpleDictionary<SharedExportKey, SharedExport> _sharedExports;

        internal ExportCache()
        {
            _exports = new SimpleDictionary<ExportKey, Export>();
            _sharedExports = new SimpleDictionary<SharedExportKey, SharedExport>();
        }

        internal bool HasExport(ExportKey exportKey)
        {
            return _exports.ContainsKey(exportKey);
        }

        internal bool IsSharedExport(SharedExportKey exportKey)
        {
            return _sharedExports.ContainsKey(exportKey);
        }

        internal Export GetExport(ExportKey exportKey)
        {
            if (!_exports.ContainsKey(exportKey))
                throw new IocExportException(exportKey, "Error utilizing InstanceCache - tried to get Instance before it was checked");

            return _exports[exportKey];
        }

        /// <summary>
        /// Sets instance of the cache for the supplied export key
        /// </summary>
        /// <exception cref="IocExportException">Throws exception for unintentional overwrites</exception>
        internal void SetExport(ExportKey exportKey, Export export, SharedExportKey sharedExportKey = null)
        {
            if (_exports.ContainsKey(exportKey))
                throw new IocExportException(exportKey, "Trying to overwrite previously set cache value");

            else if (_exports.ContainsKey(exportKey))
                _exports[exportKey] = export;

            else
                _exports.Add(exportKey, export);

            // Setup shared export
            if (exportKey.Policy == InstancePolicy.ShareExportedType ||
                exportKey.Policy == InstancePolicy.ShareGlobal)
            {
                if (sharedExportKey == null)
                    throw new IocExportException(exportKey, "Trying to set export cache with shared export; but SharedExportKey is null!");

                // NEW SHARED EXPORT
                if (!_sharedExports.ContainsKey(sharedExportKey))
                    _sharedExports.Add(sharedExportKey, new SharedExport());

                if (_sharedExports[sharedExportKey].HasExport(export))
                    throw new IocExportException(exportKey, "Trying to overwrite previously set SHARED export");

                else
                    _sharedExports[sharedExportKey].AddExport(export);
            }
        }

        /// <summary>
        /// Search to provide a user-end find for an export. This assumes the user provides the EXPORT type 
        /// and not the REFLECTED type.
        /// </summary>
        /// <exception cref="IocDuplicateExportException">Checks for duplicate exports</exception>
        /// <exception cref="IocExportException">No export found for the export type</exception>
        internal Export FindExport(Type exportType, int exportKey, bool isKeyed)
        {
            // PERFORMANCE TUNING:  TODO, probably best to find a method of fast searching.. maybe a
            //                      BinarySearchTree implementation for some kind of user-end hash code
            //                      using these parameters(?)
            //

            // Search for any exports that have a common export type
            var exports = _exports.Values.Where(export =>
            {
                return export.ExportedType.Equals(exportType) &&
                       export.ExportKey == exportKey &&
                       export.IsExportKeyed == isKeyed;

            }).Actualize();

            // CHECK FOR DUPLICATES! There could be multiple exports for the same export type that are NOT KEYED!
            //
            if (exports.Count() > 1)
                throw new FormattedException("Duplicate Exports for type {0}", exportType);

            if (exports.Count() == 0)
                throw new IocExportException(null, "No export found for type {0}", exportType);

            return exports.First();
        }

        /// <summary>
        /// Search that looks for ALL shared exports for a given REFLECTED, EXPORT type, and INSTANCE POLICY
        /// </summary>
        internal SharedExport FindSharedExport(Type reflectedType, Type exportType, InstancePolicy instancePolicy)
        {
            // Search for any exports that have a common export type
            if (instancePolicy == InstancePolicy.ShareGlobal)
            {
                var result = _sharedExports.Values
                                           .Where(sharedExport => sharedExport.Exports
                                                                              .All(export => export.ReflectedType.Equals(reflectedType) &&
                                                                                             export.Policy == InstancePolicy.ShareGlobal))
                                           .Actualize();

                if (result.Count() != 1)
                    throw new IocExportException(null, "No matching shared global export for type {0}. For InstancePolicy.ShareGlobal, ALL exports must be marked the same.", exportType);

                return result.First();
            }

            else if (instancePolicy == InstancePolicy.ShareExportedType)
            {
                var result = _sharedExports.Values
                                           .Where(sharedExport => sharedExport.Exports
                                                                              .All(export => export.ExportedType.Equals(exportType) &&
                                                                                             export.Policy == InstancePolicy.ShareExportedType))
                                           .Actualize();

                if (result.Count() != 1)
                    throw new IocExportException(null, "No matching shared export for type {0}", exportType);

                return result.First();
            }

            else
                return null;
        }

        public override string ToString()
        {
            return _exports.Count.ToString();
        }
    }
}
