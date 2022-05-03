using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WpfCustomUtilities.Extensions.NativeIO
{
    /// <summary>
    /// Contains information about the file that is found 
    /// by the FindFirstFile or FindNextFile functions.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    [BestFitMapping(false)]
    internal class System32FindData
    {
        /// <summary> </summary>
        public FileAttributes dwFileAttributes;

        /// <summary> </summary>
        public uint ftCreationTime_dwLowDateTime;

        /// <summary> </summary>
        public uint ftCreationTime_dwHighDateTime;

        /// <summary> </summary>
        public uint ftLastAccessTime_dwLowDateTime;

        /// <summary> </summary>
        public uint ftLastAccessTime_dwHighDateTime;

        /// <summary> </summary>
        public uint ftLastWriteTime_dwLowDateTime;

        /// <summary> </summary>
        public uint ftLastWriteTime_dwHighDateTime;

        /// <summary> </summary>
        public uint nFileSizeHigh;

        /// <summary> </summary>
        public uint nFileSizeLow;

        /// <summary> </summary>
        public int dwReserved0;

        /// <summary> </summary>
        public int dwReserved1;

        /// <summary> </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName = string.Empty;

        /// <summary> </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName = string.Empty;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="object"/>.
        /// </returns>
        public override string ToString() => "File name=" + cFileName;
    }
}
