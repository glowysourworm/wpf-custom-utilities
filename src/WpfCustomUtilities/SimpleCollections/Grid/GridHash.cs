
using System;
using System.Collections;
using System.Collections.Generic;

using WpfCustomUtilities.SimpleCollections.Array2D;
using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    /// <summary>
    /// Grid with hash table lookup support. Allows lookup for two items associated with a grid location.
    /// </summary>
    public class GridHash<T> : IEnumerable<T>
    {
        Grid<T> _grid;
        SimpleDictionary<T, T> _dictionary;

        public GridBoundary ParentBoundary { get { return _grid.GetParentBoundary(); } }
        public GridBoundary RelativeBoundary { get { return _grid.GetRelativeBoundary(); } }

        public T this[int column, int row]
        {
            get { return _grid[column, row]; }
        }

        public GridHash(GridBoundary parentBoundary, GridBoundary relativeBoundary)
        {
            _grid = new Grid<T>(parentBoundary, relativeBoundary);
            _dictionary = new SimpleDictionary<T, T>();
        }

        public bool IsDefined(int column, int row)
        {
            return _grid.IsDefined(column, row);
        }

        public bool Contains(T item)
        {
            return _dictionary.ContainsKey(item);
        }

        public void Iterate(GridCallback callback)
        {
            _grid.Iterate(callback);
        }
        public void Set(int column, int row, int width, int height, T item)
        {
            for (int columnIndex = column; columnIndex < column + width; columnIndex++)
            {
                for (int rowIndex = row; rowIndex < row + height; rowIndex++)
                {
                    if (_grid.IsDefined(columnIndex, rowIndex))
                        throw new Exception("Trying to overwrite GridHash<T>");

                    _grid[columnIndex, rowIndex] = item;
                }
            }

            _dictionary.Add(item, item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }
    }
}
