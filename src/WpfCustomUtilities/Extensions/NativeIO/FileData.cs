using System;
using System.IO;

namespace WpfCustomUtilities.Extensions.NativeIO
{
    /// <summary>
    /// Contains information about a file returned by the 
    /// <see cref="FastDirectoryEnumerator"/> class.
    /// </summary>
    [Serializable]
    public class FileData
    {
        /// <summary>
        /// Attributes of the file.
        /// </summary>
        public readonly FileAttributes Attributes;

        /// <summary> <c>File</c> creation time. </summary>
        public DateTime CreationTime => CreationTimeUtc.ToLocalTime();

        /// <summary>
        /// <c>File</c> creation time in UTC.
        /// </summary>
        public readonly DateTime CreationTimeUtc;

        /// <summary>
        /// Gets the last access time in local time.
        /// </summary>
        public DateTime LastAccessTime => LastAccessTimeUtc.ToLocalTime();

        /// <summary>
        /// <c>File</c> last access time in UTC.
        /// </summary>
        public readonly DateTime LastAccessTimeUtc;

        /// <summary>
        /// Gets the last access time in local time.
        /// </summary>
        public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

        /// <summary>
        /// <c>File</c> last write time in UTC
        /// </summary>
        public readonly DateTime LastWriteTimeUtc;

        /// <summary>
        /// Size of the file in bytes
        /// </summary>
        public readonly long Size;

        /// <summary>
        /// Name of the file
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Full path to the file.
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="object"/>.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileData"/> class.
        /// </summary>
        /// <param name="Dir">The directory that the file is stored at</param>
        /// <param name="FindData">System32FindData structure that this
        /// object wraps.</param>
        internal FileData(string Dir, System32FindData FindData)
        {
            Attributes = FindData.dwFileAttributes;


            CreationTimeUtc = ConvertDateTime(FindData.ftCreationTime_dwHighDateTime,
                FindData.ftCreationTime_dwLowDateTime);

            LastAccessTimeUtc = ConvertDateTime(FindData.ftLastAccessTime_dwHighDateTime,
                FindData.ftLastAccessTime_dwLowDateTime);

            LastWriteTimeUtc = ConvertDateTime(FindData.ftLastWriteTime_dwHighDateTime,
                FindData.ftLastWriteTime_dwLowDateTime);

            Size = CombineHighLowInts(FindData.nFileSizeHigh, FindData.nFileSizeLow);

            Name = FindData.cFileName;
            Path = System.IO.Path.Combine(Dir, FindData.cFileName);
        }

        /// <summary> </summary>
        static long CombineHighLowInts(uint High, uint Low) => ((long)High << 0x20) | Low;

        /// <summary> </summary>
        static DateTime ConvertDateTime(uint High, uint Low)
        {
            long FileTime = CombineHighLowInts(High, Low);
            return DateTime.FromFileTimeUtc(FileTime);
        }
    }
}
