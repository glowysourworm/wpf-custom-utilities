using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.IocFramework.Application.IocException;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.Application.InstanceManagement
{
    internal class InstanceCache
    {
        // Primary instance cache for non-shared exports
        SimpleDictionary<Export, Instance> _exports;

        // Primary instance cache for shared exports
        SimpleDictionary<SharedExport, Instance> _sharedExports;

        internal Instance this[Export exportKey]
        {
            get { return _exports[exportKey]; }
        }

        internal Instance this[SharedExport sharedExport]
        {
            get { return _sharedExports[sharedExport]; }
        }

        internal InstanceCache()
        {
            _exports = new SimpleDictionary<Export, Instance>();
            _sharedExports = new SimpleDictionary<SharedExport, Instance>();
        }

        internal bool HasInstance(Export export)
        {
            return _exports.ContainsKey(export);
        }

        internal bool HasInstance(SharedExport sharedExport)
        {
            return _sharedExports.ContainsKey(sharedExport);
        }

        /// <summary>
        /// Sets instance of the cache for the supplied export key
        /// </summary>
        /// <exception cref="IocExportException">Throws exception for unintentional overwrites</exception>
        internal void UpdateInstance(Export export, object instance, bool overwrite = false)
        {
            if (_exports.ContainsKey(export) && !overwrite)
                throw new IocExportException(export, "Trying to overwrite previously set cache value");

            else if (_exports.ContainsKey(export) && overwrite)
                _exports[export].Update(instance, overwrite);

            else
                _exports.Add(export, new Instance(instance));
        }

        /// <summary>
        /// Sets instance of the cache for the supplied SHARED export key
        /// </summary>
        /// <exception cref="IocExportException">Throws exception for unintentional overwrites</exception>
        internal void UpdateInstance(SharedExport sharedExport, object instance, bool overwrite = false)
        {
            if (_sharedExports.ContainsKey(sharedExport) && !overwrite)
                throw new FormattedException("Trying to overwrite previously set cache value: InstanceCache.cs");

            else if (_sharedExports.ContainsKey(sharedExport) && overwrite)
                _sharedExports[sharedExport].Update(instance, overwrite);

            else
                _sharedExports.Add(sharedExport, new Instance(instance));
        }
    }
}
