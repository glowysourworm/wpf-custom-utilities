using System.Collections.Generic;

using WpfCustomUtilities.Extensions;

using WpfCustomUtilities.IocFramework.Application.InstanceManagement;

namespace WpfCustomUtilities.IocFramework.Application.IocException
{
    internal class IocDuplicateExportException : FormattedException
    {
        public IEnumerable<ExportKey> DuplicateExports { get; set; }

        public IocDuplicateExportException(IEnumerable<ExportKey> duplicates, string message)
                : base(message)
        {
            this.DuplicateExports = duplicates;
        }

        public IocDuplicateExportException(IEnumerable<ExportKey> duplicates, string message, params object[] args)
                : base(message, args)
        {
            this.DuplicateExports = duplicates;
        }

        public IocDuplicateExportException(IEnumerable<ExportKey> duplicates, string message, System.Exception innerException, params object[] args)
                : base(message, innerException, args)
        {
            this.DuplicateExports = duplicates;
        }
    }
}
