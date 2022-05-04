using System;
using System.Collections.Generic;
using System.Linq;

using WpfCustomUtilities.Extensions;

using WpfCustomUtilities.IocFramework.Application.Attribute;
using WpfCustomUtilities.RecursiveSerializer.Shared;

namespace WpfCustomUtilities.IocFramework.Application.InstanceManagement
{
    /// <summary>
    /// Creates an instance key based on values of the export attribute
    /// </summary>
    public class SharedExportKey
    {
        readonly int _calculatedHashCode;

        /// <summary>
        /// A collection of all exported types that will share the instance
        /// </summary>
        public IEnumerable<ExportKey> ExportedTypes { get; private set; }

        public Type ReflectedType { get; private set; }
        public InstancePolicy Policy { get; private set; }

        internal SharedExportKey(Type reflectedType, InstancePolicy instancePolicy, IEnumerable<ExportKey> exportedTypeKeys)
        {
            if (!exportedTypeKeys.All(exportKey => exportKey.ReflectedType.Equals(reflectedType)))
                throw new FormattedException("SharedExportKey MUST have ALL REFLECTED TYPES MATCH:  {0}", reflectedType);

            if (!exportedTypeKeys.All(exportKey => exportKey.Policy == instancePolicy))
                throw new FormattedException("SharedExportKey MUST have ALL INSTANCE POLICIES MATCH:  {0}", reflectedType);

            this.ReflectedType = reflectedType;
            this.ExportedTypes = exportedTypeKeys;
            this.Policy = instancePolicy;

            // DON'T NEED THE REFLECTED TYPE FOR THE HASH CODE
            _calculatedHashCode = RecursiveSerializerHashGenerator.CreateSimpleHash(instancePolicy, exportedTypeKeys);
        }

        public override int GetHashCode()
        {
            if (_calculatedHashCode == default(int))
                throw new System.Exception("InstanceKey hash code has not been calculated");

            return _calculatedHashCode;
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == this.GetHashCode();
        }

        public override string ToString()
        {
            return this.ReflectedType.ToString();
        }
    }
}
