using Microsoft.Win32.SafeHandles;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WpfCustomUtilities.Extensions.NativeIO.FastDirectory
{
    /// <summary>
    /// General extension methods for the <see cref="FastDirectoryEnumerator"/> class.
    /// </summary>
    public static class FastDirectoryEnumeratorExtension
    {
        /// <inheritdoc cref="FastDirectoryEnumerator.EnumerateFiles(string)"/>
        public static IEnumerable<FileData> FastEnumerateFiles(this DirectoryInfo DI) => FastDirectoryEnumerator.EnumerateFiles(DI.FullName);

        /// <inheritdoc cref="FastDirectoryEnumerator.EnumerateFiles(string,string)"/>
        public static IEnumerable<FileData> FastEnumerateFiles(this DirectoryInfo DI, string SearchPattern) => FastDirectoryEnumerator.EnumerateFiles(DI.FullName, SearchPattern);

        /// <inheritdoc cref="FastDirectoryEnumerator.EnumerateFiles(string,string,SearchOption)"/>
        public static IEnumerable<FileData> FastEnumerateFiles(this DirectoryInfo DI, string SearchPattern, SearchOption SearchOption) => FastDirectoryEnumerator.EnumerateFiles(DI.FullName, SearchPattern, SearchOption);

        /// <inheritdoc cref="FastDirectoryEnumerator.GetFiles(string,string,SearchOption)"/>
        public static FileData[] FastGetFiles(this DirectoryInfo DI, string SearchPattern, SearchOption SearchOption) => FastDirectoryEnumerator.GetFiles(DI.FullName, SearchPattern, SearchOption);

        /// <summary>
        /// Creates <see cref="FileInfo"/> instances from the given <see cref="FileData"/> paths.
        /// </summary>
        /// <param name="Files">The files to iterate through.</param>
        /// <returns>An enumerable of <see cref="FileInfo"/> instances.</returns>
        public static IEnumerable<FileInfo> GetInfo(this IEnumerable<FileData> Files)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (FileData FD in Files)
            {
                yield return new FileInfo(FD.Path);
            }
        }

        /// <summary>
        /// Counts the number of files in the given directory.
        /// </summary>
        /// <param name="DI">The parent directory.</param>
        /// <returns>An integer count of files.</returns>
        public static int CountFiles(this DirectoryInfo DI) => FastDirectoryEnumerator.EnumerateFiles(DI.FullName).Count();

        /// <summary>
        /// <see href="http://www.pinvoke.net/default.aspx/shell32/GetFinalPathNameByHandle.html"/>
        /// </summary>
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [SuppressMessage("Globalization", "CA2101:Specify marshalling for P/Invoke string arguments")]
        [SuppressMessage("Performance", "CA1838:Avoid 'StringBuilder' for P/Invokes")]
        static extern uint GetFinalPathNameByHandle(SafeFileHandle HFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder LpszFilePath, uint CchFilePath, uint DWFlags);

        /// <summary> </summary>
        const uint File_Name_Normalized = 0x0;

        /// <summary>
        /// Retrieves the final (case-sensitive) file path by the given handle.
        /// </summary>
        /// <param name="FileHandle">The handle to retrieve the path from.</param>
        /// <returns>The final file path.</returns>
        private static string GetFinalPathNameByHandle(this SafeFileHandle FileHandle)
        {
            StringBuilder OutPath = new StringBuilder(1024);

            uint Size = GetFinalPathNameByHandle(FileHandle, OutPath, (uint)OutPath.Capacity, File_Name_Normalized);
            if (Size == 0 || Size > OutPath.Capacity)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // may be prefixed with \\?\, which we don't want
            return OutPath[0] == '\\' && OutPath[1] == '\\' && OutPath[2] == '?' && OutPath[3] == '\\'
                ? OutPath.ToString(4, OutPath.Length - 4)
                : OutPath.ToString();
        }

        /// <summary/>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [SuppressMessage("Globalization", "CA2101:Specify marshalling for P/Invoke string arguments")]
        static extern SafeFileHandle? CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string Filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess Access,
            [MarshalAs(UnmanagedType.U4)] FileShare Share,
            IntPtr SecurityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode CreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes FlagsAndAttributes,
            IntPtr TemplateFile);

        /// <summary/>
        const uint File_Flag_Backup_Semantics = 0x02000000;

        /// <summary>
        /// Gets the final (case-sensitive) path name from the given path.
        /// </summary>
        /// <param name="DirtyPath">The path to retrieve the final name of.</param>
        /// <returns>The final (case-sensitive) path.</returns>
        private static string GetFinalPathName(string DirtyPath)
        {
            // use 0 for access so we can avoid error on our metadata-only query (see dwDesiredAccess docs on CreateFile)
            // use FILE_FLAG_BACKUP_SEMANTICS for attributes so we can operate on directories (see Directories in remarks section for CreateFile docs)

            using (SafeFileHandle? DirectoryHandle = CreateFile(
                       DirtyPath, 0, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open,
                       (FileAttributes)File_Flag_Backup_Semantics, IntPtr.Zero))
            {
                return DirectoryHandle?.IsInvalid ?? true
                    ? throw new Win32Exception(Marshal.GetLastWin32Error())
                    : GetFinalPathNameByHandle(DirectoryHandle);
            }
        }

        /// <summary>
        /// Gets the final (case-sensitive) path name from the given path.
        /// </summary>
        /// <param name="FSI">The path to retrieve the final name of.</param>
        /// <returns>The final (case-sensitive) path.</returns>
        private static string GetCaseSensitiveFullName(this FileSystemInfo FSI)
        {
            try
            {
                return GetFinalPathName(FSI.FullName);
            }
            catch (Exception E)
            {
                Debug.WriteLine($"Caught {E.GetType().Name} : {E.Message}", "WARNING");
                return FSI.FullName;
            }
        }
    }
}
