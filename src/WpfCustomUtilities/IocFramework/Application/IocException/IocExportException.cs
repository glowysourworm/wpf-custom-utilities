using System;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.IocFramework.Application.InstanceManagement;

namespace WpfCustomUtilities.IocFramework.Application.IocException
{
    internal class IocExportException : FormattedException
    {
        internal ExportKey ExportKey { get; set; }
        internal Export Export { get; set; }
        internal SharedExport SharedExport { get; set; }

        internal IocExportException(ExportKey exportTypeKey,
                                  string message) : base(message)
        {
            this.ExportKey = exportTypeKey;
            this.Export = null;
            this.SharedExport = null;
        }

        internal IocExportException(Export export,
                                  string message) : base(message)
        {
            this.Export = export;
            this.ExportKey = null;
            this.SharedExport = null;
        }
        internal IocExportException(SharedExport export,
                          string message) : base(message)
        {
            this.SharedExport = export;
            this.ExportKey = null;
            this.Export = null;
        }

        internal IocExportException(ExportKey exportTypeKey,
                                  string message,
                                  params object[] args) : base(message, args)
        {
            this.ExportKey = exportTypeKey;
        }

        internal IocExportException(ExportKey exportTypeKey,
                                  string message,
                                  Exception innerException,
                                  params object[] args) : base(message, innerException, args)
        {
            this.ExportKey = exportTypeKey;
        }
    }
}
