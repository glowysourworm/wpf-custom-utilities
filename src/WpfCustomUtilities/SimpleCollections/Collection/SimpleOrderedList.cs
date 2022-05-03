using System;
using System.Collections;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Interface;

namespace WpfCustomUtilities.SimpleCollections.Collection
{
    /// <summary>
    /// A simple ordered list implementation - sorts items when inserted and removed
    /// </summary>
    [Serializable]
    public class SimpleOrderedList<T> : IList<T>, IList, IRecursiveSerializable
    {
        /// <summary>
        /// Protected list of items
        /// </summary>
        protected List<T> ItemList { get; private set; }

        /// <summary>
        /// Equality comparer for sorting
        /// </summary>
        protected Comparer<T> ItemComparer { get; private set; }

        const int UNSUCCESSFUL_SEARCH = -1;

        public SimpleOrderedList()
        {
            this.ItemList = new List<T>();
            this.ItemComparer = Comparer<T>.Default;
        }

        public SimpleOrderedList(Comparer<T> comparer)
        {
            this.ItemList = new List<T>();
            this.ItemComparer = comparer;
        }

        public SimpleOrderedList(List<T> itemList, Comparer<T> itemComparer)
        {
            this.ItemList = itemList;
            this.ItemComparer = itemComparer;
        }

        public SimpleOrderedList(IPropertyReader reader) : this(reader.Read<List<T>>("List"), reader.Read<Comparer<T>>("Comparer"))
        {
        }

        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("List", this.ItemList);
            writer.Write("Comparer", this.ItemComparer);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
                Add(item);
        }

        #region (public) IList<T> IList

        public T this[int index]
        {
            get { return this.ItemList[index]; }
            set { throw new Exception("Manual insert not supported for SimpleOrderedList<>"); }
        }

        public int Count
        {
            get { return this.ItemList.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        object IList.this[int index]
        {
            get { return this.ItemList[index]; }
            set { throw new Exception("Manual insert not supported for SimpleOrderedList<>"); }
        }

        // O(log n)
        public void Add(T item)
        {
            var index = GetInsertIndex(item);

            this.ItemList.Insert(index, item);
        }

        public void Clear()
        {
            this.ItemList.Clear();
        }

        // O(log n)
        public bool Contains(T item)
        {
            return GetInsertIndex(item) != UNSUCCESSFUL_SEARCH;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.ItemList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.ItemList.GetEnumerator();
        }

        // O(log n)
        public int IndexOf(T item)
        {
            return GetInsertIndex(item);
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Manual insertion not allowed for SimpleOrderedList<>");
        }

        // O(log n)
        public bool Remove(T item)
        {
            var index = GetInsertIndex(item);

            if (index == UNSUCCESSFUL_SEARCH)
                throw new Exception("Item not found in collection SimpleOrderedList.cs");

            this.ItemList.RemoveAt(index);

            return true;
        }

        public void RemoveAt(int index)
        {
            this.ItemList.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ItemList.GetEnumerator();
        }

        // O(log n)
        public int Add(object value)
        {
            if (!(value is T))
                throw new Exception("Trying to insert non-template type:  SimpleOrderedList");

            var index = GetInsertIndex((T)value);

            this.ItemList.Insert(index, (T)value);

            return index;
        }

        // O(log n)
        public bool Contains(object value)
        {
            if (!(value is T))
                throw new Exception("Trying to operate on non-template type:  SimpleOrderedList");

            return GetInsertIndex((T)value) != UNSUCCESSFUL_SEARCH;
        }

        // O(log n)
        public int IndexOf(object value)
        {
            if (!(value is T))
                throw new Exception("Trying to operate on non-template type:  SimpleOrderedList");

            return GetInsertIndex((T)value);
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException("Manual insertion not allowed for SimpleOrderedList<>");
        }

        // O(log n)
        public void Remove(object value)
        {
            if (!(value is T))
                throw new Exception("Trying to operate on non-template type:  SimpleOrderedList");

            var index = GetInsertIndex((T)value);

            if (index == UNSUCCESSFUL_SEARCH)
                throw new Exception("Item not found in collection SimpleOrderedList.cs");

            this.ItemList.RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a copy of the item list that detaches the internal array
        /// linearly - creating a list copy of references. Does not copy the comparer - so
        /// that will share a reference
        /// </summary>
        public SimpleOrderedList<T> CreateCopy()
        {
            var listCopy = new List<T>(this.ItemList.Capacity);

            foreach (var item in this.ItemList)
                listCopy.Add(item);

            return new SimpleOrderedList<T>(listCopy, this.ItemComparer);
        }

        #endregion

        #region (private) Binary Search Implementation

        private int BinarySearch(T searchItem, out int insertIndex)
        {
            /*
                function binary_search(A, n, T) is
                    L := 0
                    R := n − 1
                    while L ≤ R do
                        m := floor((L + R) / 2)
                        if A[m] < T then
                            L := m + 1
                        else if A[m] > T then
                            R := m − 1
                        else:
                            return m
                    return unsuccessful
             */

            var leftIndex = 0;
            var rightIndex = this.ItemList.Count - 1;

            // Initialize insert index to be the left index
            insertIndex = leftIndex;

            while (leftIndex <= rightIndex)
            {
                var middleIndex = (int)Math.Floor((leftIndex + rightIndex) / 2.0D);
                var item = this.ItemList[middleIndex];

                // Set insert index
                insertIndex = middleIndex;

                // Item's value is LESS THAN search value
                if (this.ItemComparer.Compare(item, searchItem) < 0)
                {
                    leftIndex = middleIndex + 1;

                    // Set insert index for catching final iteration
                    insertIndex = leftIndex;
                }

                // GREATER THAN
                else if (this.ItemComparer.Compare(item, searchItem) > 0)
                    rightIndex = middleIndex - 1;

                else
                    return middleIndex;
            }

            return UNSUCCESSFUL_SEARCH;
        }

        private int GetInsertIndex(T item)
        {
            var insertIndex = UNSUCCESSFUL_SEARCH;
            var searchIndex = BinarySearch(item, out insertIndex);

            // NOT FOUND
            if (searchIndex == UNSUCCESSFUL_SEARCH)
            {
                return insertIndex;
            }
            else
            {
                return searchIndex;
            }
        }
        #endregion
    }
}
