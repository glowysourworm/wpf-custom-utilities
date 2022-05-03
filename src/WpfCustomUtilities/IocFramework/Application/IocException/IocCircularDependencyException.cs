using System;
using System.Collections.Generic;

using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.IocFramework.Application.InstanceManagement;

namespace WpfCustomUtilities.IocFramework.Application.IocException
{
    internal class IocCircularDependencyException : FormattedException
    {
        public ExportKey Export { get; set; }

        public IEnumerable<ExportKey> Dependencies { get; set; }

        public IocCircularDependencyException(ExportKey export, IEnumerable<ExportKey> dependencies, string message)
                : base(message)
        {
            this.Export = export;
            this.Dependencies = dependencies;
        }

        public IocCircularDependencyException(ExportKey export, IEnumerable<ExportKey> dependencies, string message, params object[] args)
                : base(message, args)
        {
            this.Export = export;
            this.Dependencies = dependencies;
        }

        public IocCircularDependencyException(ExportKey export, IEnumerable<ExportKey> dependencies, string message, Exception innerException, params object[] args)
                : base(message, innerException, args)
        {
            this.Export = export;
            this.Dependencies = dependencies;
        }
    }
}
