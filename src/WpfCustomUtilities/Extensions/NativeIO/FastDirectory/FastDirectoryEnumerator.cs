#region Copyright (C) 2017-2021  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

using JetBrains.Annotations;

using Microsoft.Win32.SafeHandles;

#endregion

namespace WpfCustomUtilities.Extensions.NativeIO.FastDirectory
{
    /// <summary>
    /// A fast enumerator of files in a directory. Use it if you need to get attributes for 
    /// all files in a directory.
    /// <para/>See: <see href="https://www.codeproject.com/Articles/38959/A-Faster-Directory-Enumerator"/>
    /// </summary>
    /// <remarks>
    /// This enumerator is substantially faster than using <see cref="Directory.GetFiles(string)"/>
    /// and then creating a new FileInfo object for each path.  Use this version when you 
    /// will need to look at the attibutes of each file returned (for example, you need
    /// to check each file in a directory to see if it was modified after a specific date).
    /// </remarks>
    public static class FastDirectoryEnumerator
    {
        /// <summary>
        /// Gets <see cref="FileData"/> for all the files in a directory.
        /// </summary>
        /// <param name="Path">The path to search.</param>
        /// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
        /// allows you to enumerate the files in the given directory.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="Path"/> is a null reference (Nothing in VB)
        /// </exception>
        public static IEnumerable<FileData> EnumerateFiles(string Path) => EnumerateFiles(Path, "*");

        /// <summary>
        /// Gets <see cref="FileData"/> for all the files in a directory that match a 
        /// specific filter.
        /// </summary>
        /// <param name="Path">The path to search.</param>
        /// <param name="SearchPattern">The search string to match against files in the path.</param>
        /// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
        /// allows you to enumerate the files in the given directory.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="Path"/> is a null reference (Nothing in VB)
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static IEnumerable<FileData> EnumerateFiles(string Path, string SearchPattern) => EnumerateFiles(Path, SearchPattern, SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets <see cref="FileData"/> for all the files in a directory that 
        /// match a specific filter, optionally including all sub directories.
        /// </summary>
        /// <param name="Path">The path to search.</param>
        /// <param name="SearchPattern">The search string to match against files in the path.</param>
        /// <param name="SearchOption">
        /// One of the SearchOption values that specifies whether the search 
        /// operation should include all subdirectories or only the current directory.
        /// </param>
        /// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
        /// allows you to enumerate the files in the given directory.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="Path"/> is a null reference (Nothing in VB)
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// filter is a null reference (Nothing in VB)
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="SearchOption"/> is not one of the valid values of the
        /// <see cref="SearchOption"/> enumeration.
        /// </exception>
        public static IEnumerable<FileData> EnumerateFiles(string Path, string SearchPattern, SearchOption SearchOption)
        {
            if (Path == null) { throw new ArgumentNullException(nameof(Path)); }

            if (SearchPattern == null) { throw new ArgumentNullException(nameof(SearchPattern)); }

            if (SearchOption != SearchOption.TopDirectoryOnly && SearchOption != SearchOption.AllDirectories) { throw new ArgumentOutOfRangeException(nameof(SearchOption)); }

            string FullPath = System.IO.Path.GetFullPath(Path);

            return new FileEnumerable(FullPath, SearchPattern, SearchOption);
        }

        /// <summary>
        /// Gets <see cref="FileData"/> for all the files in a directory that match a 
        /// specific filter.
        /// </summary>
        /// <param name="Path">The path to search.</param>
        /// <param name="SearchPattern">The search string to match against files in the path.</param>
        /// <param name="SearchOption">SearchOption for the search query</param>
        /// <returns>An object that implements <see cref="IEnumerable{FileData}"/> and 
        /// allows you to enumerate the files in the given directory.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="Path"/> is a null reference (Nothing in VB)
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// filter is a null reference (Nothing in VB)
        /// </exception>
        public static FileData[] GetFiles(string Path, string SearchPattern, SearchOption SearchOption)
        {
            IEnumerable<FileData> E = EnumerateFiles(Path, SearchPattern, SearchOption);
            List<FileData> List = new List<FileData>(E);

            FileData[] Retval = new FileData[List.Count];
            List.CopyTo(Retval);

            return Retval;
        }

        /// <summary>
        /// Provides the implementation of the 
        /// <see cref="T:System.Collections.Generic.IEnumerable`1"/> interface
        /// </summary>
        class FileEnumerable : IEnumerable<FileData>
        {
            /// <summary> </summary>
            readonly string _M_Path;

            /// <summary> </summary>
            readonly string _M_Filter;

            /// <summary> </summary>
            readonly SearchOption _M_SearchOption;

            /// <summary>
            /// Initializes a new instance of the <see cref="FileEnumerable"/> class.
            /// </summary>
            /// <param name="Path">The path to search.</param>
            /// <param name="Filter">The search string to match against files in the path.</param>
            /// <param name="SearchOption">
            /// One of the SearchOption values that specifies whether the search 
            /// operation should include all subdirectories or only the current directory.
            /// </param>
            public FileEnumerable(string Path, string Filter, SearchOption SearchOption)
            {
                _M_Path = Path;
                _M_Filter = Filter;
                _M_SearchOption = SearchOption;
            }

            #region IEnumerable<FileData> Members

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="IEnumerator{T}"/> that can 
            /// be used to iterate through the collection.
            /// </returns>
            public IEnumerator<FileData> GetEnumerator() => new FileEnumerator(_M_Path, _M_Filter, _M_SearchOption);

            #endregion

            #region IEnumerable Members

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>
            /// An <see cref="T:System.Collections.IEnumerator"/> object that can be 
            /// used to iterate through the collection.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator() => new FileEnumerator(_M_Path, _M_Filter, _M_SearchOption);

            #endregion

        }

        /// <summary>
        /// Wraps a FindFirstFile handle.
        /// </summary>
        [UsedImplicitly]
        sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
#pragma warning disable SYSLIB0004 // Type or member is obsolete
#pragma warning disable SYSLIB0003
#pragma warning disable 618

            /// <summary> </summary>
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport("kernel32.dll")]
            static extern bool FindClose(IntPtr Handle);

            /// <summary>
            /// Initializes a new instance of the <see cref="SafeFindHandle"/> class.
            /// </summary>
            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            internal SafeFindHandle()
                : base(true) { }

#pragma warning restore 618
#pragma warning restore SYSLIB0003
#pragma warning restore SYSLIB0004 // Type or member is obsolete

            /// <summary>
            /// When overridden in a derived class, executes the code required to free the handle.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the handle is released successfully; otherwise, in the 
            /// event of a catastrophic failure, <see langword="false"/>. If so, it 
            /// generates a releaseHandleFailed MDA Managed Debugging Assistant.
            /// </returns>
            protected override bool ReleaseHandle() => FindClose(handle);
        }

        /// <summary>
        /// Provides the implementation of the 
        /// <see cref="IEnumerator{T}"/> interface
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        class FileEnumerator : IEnumerator<FileData>
        {
            /// <summary> </summary>
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            static extern SafeFindHandle FindFirstFile(string FileName,
                [In][Out] System32FindData Data);

            /// <summary> </summary>
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            static extern bool FindNextFile(SafeFindHandle HndFindFile,
                [In][Out][MarshalAs(UnmanagedType.LPStruct)]
            System32FindData LpFindFileData);

            /// <summary>
            /// Hold context information about where we current are in the directory search.
            /// </summary>
            class SearchContext
            {
                /// <summary> </summary>
                public readonly string Path;

                /// <summary> </summary>
                public Stack<string>? SubdirectoriesToProcess;

                /// <summary> </summary>
                public SearchContext(string PATH) => Path = PATH;
            }

#pragma warning disable IDE0044 // Add readonly modifier
            // ReSharper disable FieldCanBeMadeReadOnly.Local
            /// <summary> </summary>
            string _M_Path;

            /// <summary> </summary>
            string _M_Filter;

            /// <summary> </summary>
            SearchOption _M_SearchOption;

            /// <summary> </summary>
            Stack<SearchContext> _M_ContextStack;

            /// <summary> </summary>
            SearchContext _M_CurrentContext;

            /// <summary> </summary>
            SafeFindHandle? _M_HndFindFile;

            /// <summary> </summary>
            System32FindData _M_Win_Find_Data = new System32FindData();
            // ReSharper restore FieldCanBeMadeReadOnly.Local
#pragma warning restore IDE0044 // Add readonly modifier

            /// <summary>
            /// Initializes a new instance of the <see cref="FileEnumerator"/> class.
            /// </summary>
            /// <param name="Path">The path to search.</param>
            /// <param name="Filter">The search string to match against files in the path.</param>
            /// <param name="SearchOption">
            /// One of the SearchOption values that specifies whether the search 
            /// operation should include all subdirectories or only the current directory.
            /// </param>
            public FileEnumerator(string Path, string Filter, SearchOption SearchOption)
            {
                _M_Path = Path;
                _M_Filter = Filter;
                _M_SearchOption = SearchOption;
                _M_CurrentContext = new SearchContext(Path);

                _M_HndFindFile = null;
                // ReSharper disable once RedundantSuppressNullableWarningExpression
                _M_ContextStack = (_M_SearchOption == SearchOption.AllDirectories ? new Stack<SearchContext>() : default!)!;
            }

            #region IEnumerator<FileData> Members

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            public FileData Current => new FileData(_M_Path, _M_Win_Find_Data);

            #endregion

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose() => _M_HndFindFile?.Dispose();

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            object IEnumerator.Current => new FileData(_M_Path, _M_Win_Find_Data);

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
            /// <see langword="false"/> if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public bool MoveNext()
            {
                bool Retval = false;

                //If the handle is null, this is first call to MoveNext in the current 
                // directory.  In that case, start a new search.
                if (_M_CurrentContext.SubdirectoriesToProcess is null)
                {
                    if (_M_HndFindFile is null)
                    {

#pragma warning disable SYSLIB0003 // Type or member is obsolete
#pragma warning disable 618
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, _M_Path).Demand();
#pragma warning restore SYSLIB0003 // Type or member is obsolete
#pragma warning restore 618

                        string SearchPath = Path.Combine(_M_Path, _M_Filter);
                        _M_HndFindFile = FindFirstFile(SearchPath, _M_Win_Find_Data);
                        Retval = !_M_HndFindFile.IsInvalid;
                    }
                    else
                    {
                        //Otherwise, find the next item.
                        Retval = FindNextFile(_M_HndFindFile, _M_Win_Find_Data);
                    }
                }

                //If the call to FindNextFile or FindFirstFile succeeded...
                if (Retval)
                {
                    if ((_M_Win_Find_Data.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        //Ignore folders for now.   We call MoveNext recursively here to 
                        // move to the next item that FindNextFile will return.
                        // ReSharper disable once TailRecursiveCall
                        return MoveNext();
                    }
                }
                else if (_M_SearchOption == SearchOption.AllDirectories)
                {
                    //SearchContext context = new SearchContext(m_hndFindFile, m_path);
                    //m_contextStack.Push(context);
                    //m_path = Path.Combine(m_path, m_win_find_data.cFileName);
                    //m_hndFindFile = null;

                    if (_M_CurrentContext.SubdirectoriesToProcess == null)
                    {
                        string[] SubDirectories = Directory.GetDirectories(_M_Path);
                        _M_CurrentContext.SubdirectoriesToProcess = new Stack<string>(SubDirectories);
                    }

                    if (_M_CurrentContext.SubdirectoriesToProcess.Count > 0)
                    {
                        string SubDir = _M_CurrentContext.SubdirectoriesToProcess.Pop();

                        _M_ContextStack.Push(_M_CurrentContext);
                        _M_Path = SubDir;
                        _M_HndFindFile = null;
                        _M_CurrentContext = new SearchContext(_M_Path);
                        // ReSharper disable once TailRecursiveCall
                        return MoveNext();
                    }

                    //If there are no more files in this directory and we are 
                    // in a sub directory, pop back up to the parent directory and
                    // continue the search from there.
                    if (_M_ContextStack.Count > 0)
                    {
                        _M_CurrentContext = _M_ContextStack.Pop();
                        _M_Path = _M_CurrentContext.Path;
                        if (_M_HndFindFile != null)
                        {
                            _M_HndFindFile.Close();
                            _M_HndFindFile = null;
                        }

                        // ReSharper disable once TailRecursiveCall
                        return MoveNext();
                    }
                }

                return Retval;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public void Reset() => _M_HndFindFile = null;

            #endregion

        }
    }
}