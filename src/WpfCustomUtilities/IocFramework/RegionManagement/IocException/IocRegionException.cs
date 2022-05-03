using System;

using WpfCustomUtilities.Extensions;

namespace WpfCustomUtilities.IocFramework.RegionManagement.IocException
{
    public class IocRegionException : FormattedException
    {
        public IocRegionException(string message) : base(message)
        {
        }

        public IocRegionException(string message, params object[] args) : base(message, args)
        {
        }

        public IocRegionException(string message, Exception innerException, params object[] args) : base(message, innerException, args)
        {
        }
    }
}
