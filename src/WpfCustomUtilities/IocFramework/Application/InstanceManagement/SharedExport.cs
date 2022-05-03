using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.IocFramework.Application.IocException;
using WpfCustomUtilities.RecursiveSerializer.Utility;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.IocFramework.Application.InstanceManagement
{
    internal class SharedExport
    {
        /// <summary>
        /// Exports that share the same REFLECTED type and possibly different EXPORT types.
        /// </summary>
        internal IEnumerable<Export> Exports { get { return _exportDict.Values; } }

        readonly SimpleDictionary<Export, Export> _exportDict;

        internal SharedExport()
        {
            _exportDict = new SimpleDictionary<Export, Export>();
        }

        internal void AddExport(Export export)
        {
            if (_exportDict.ContainsKey(export))
                throw new IocDuplicateExportException(null, "Duplicate export found for type:  " + export.ReflectedType);

            if (!_exportDict.Values.All(x =>
            {
                return x.ReflectedType == export.ReflectedType &&
                       x.Policy == export.Policy;
            }))
                throw new IocExportException(export, "Mis-matching reflected types for shared export");

            _exportDict.Add(export, export);
        }

        internal bool HasExport(Export export)
        {
            return _exportDict.ContainsKey(export);
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.Exports);
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
    }
}
