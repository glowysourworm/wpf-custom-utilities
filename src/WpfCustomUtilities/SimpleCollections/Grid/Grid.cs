
using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Interface;
using WpfCustomUtilities.RecursiveSerializer.Shared;
using WpfCustomUtilities.SimpleCollections.Array2D;
using WpfCustomUtilities.SimpleCollections.Collection;
using WpfCustomUtilities.SimpleCollections.Grid.Interface;

using static WpfCustomUtilities.SimpleCollections.Array2D.Array2DExtension;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    /// <summary>
    /// 2D array component built on a parent 2D array with minimized storage and offset capability. Does 
    /// automatic index math using its indexer; and serializes offsets from the parent 2D array. Also, 
    /// serializes the data.
    /// </summary>
    [Serializable]
    public class Grid<T> : IRecursiveSerializable
    {
        readonly protected T[,] Array2D;
        readonly int _offsetColumn;
        readonly int _offsetRow;
        readonly int _width;
        readonly int _height;
        readonly int _parentWidth;
        readonly int _parentHeight;

        // Flag that may be set after populating the grid
        bool _isReadOnly;

        /// <summary>
        /// Indexer to the 2D array based on the PARENT COLUMN AND ROW. Allows indexing over 
        /// full parent index space - but returns default(T) for indices out of bounds of the
        /// grid. For setting, throws an exception if the parent indexer is outside the bounds
        /// of the grid.
        /// </summary>
        public T this[int parentColumn, int parentRow]
        {
            get
            {
                if (parentColumn < 0 ||
                    parentColumn >= _parentWidth ||
                    parentRow < 0 ||
                    parentRow >= _parentHeight)
                    throw new Exception("Trying to index outside the PARENT boundary:  Grid.this[]");

                var column = parentColumn - _offsetColumn;
                var row = parentRow - _offsetRow;

                if (column < 0 ||
                    column >= _width ||
                    row < 0 ||
                    row >= _height)
                    return default(T);

                return this.Array2D[column, row];
            }
            set
            {
                var column = parentColumn - _offsetColumn;
                var row = parentRow - _offsetRow;

                if (column < 0 ||
                    column >= _width ||
                    row < 0 ||
                    row >= _height)
                    throw new Exception("Trying to set Grid<> outside of its bounds");

                if (_isReadOnly)
                    throw new Exception("Trying to set Grid<> marked read-only");

                this.Array2D[column, row] = value;
            }
        }

        /// <summary>
        /// Indexer to the 2D array based on the PARENT COLUMN AND ROW. Allows indexing over 
        /// full parent index space - but returns default(T) for indices out of bounds of the
        /// grid. For setting, throws an exception if the parent indexer is outside the bounds
        /// of the grid.
        /// </summary>
        public T this[IGridLocator parentLocator]
        {
            get { return this[parentLocator.Column, parentLocator.Row]; }
            set { this[parentLocator.Column, parentLocator.Row] = value; }
        }

        /// <summary>
        /// Returns the region boundary for the sub-grid that this Grid instance represents
        /// </summary>
        public GridBoundary GetRelativeBoundary()
        {
            return new GridBoundary(_offsetColumn, _offsetRow, _width, _height);
        }

        public GridBoundary GetParentBoundary()
        {
            return new GridBoundary(0, 0, _parentWidth, _parentHeight);
        }

        public bool IsReadOnly()
        {
            return _isReadOnly;
        }

        /// <summary>
        /// Returns true if the grid is defined in the provided indices
        /// </summary>
        public bool IsDefined(int parentColumn, int parentRow)
        {
            if (parentColumn < 0 ||
                parentColumn >= _parentWidth ||
                parentRow < 0 ||
                parentRow >= _parentHeight)
                return false;

            // Using indexer to provide the rest of the offset math
            return this[parentColumn, parentRow] != null;
        }

        /// <summary>
        /// Returns true if the grid is defined in the provided indices - checking the relative boundary
        /// </summary>
        public bool BoundaryContains(int parentColumn, int parentRow)
        {
            // First, check absolute boundary
            if (parentColumn < 0 ||
                parentColumn >= _parentWidth ||
                parentRow < 0 ||
                parentRow >= _parentHeight)
                return false;

            var column = parentColumn - _offsetColumn;
            var row = parentRow - _offsetRow;

            // Next, check relative boundary
            if (column < 0 ||
                column >= _width ||
                row < 0 ||
                row >= _height)
                return false;

            return true;
        }

        /// <summary>
        /// Returns default if not defined at the location.
        /// </summary>
        public T GetSafe(int parentColumn, int parentRow)
        {
            if (!IsDefined(parentColumn, parentRow))
                return default(T);

            return this[parentColumn, parentRow];
        }

        /// <summary>
        /// Creates a default grid locator if there is none in the grid that satisfy the interface at this
        /// location
        /// </summary>
        public IGridLocator GetLocator(int parentColumn, int parentRow)
        {
            if (IsDefined(parentColumn, parentRow) && typeof(IGridLocator).IsAssignableFrom(typeof(T)))
                return this[parentColumn, parentRow] as IGridLocator;

            else
                return new GridLocation(parentColumn, parentRow);
        }

        /// <summary>
        /// Sets a flag to prohibit writing of the grid for the rest of its lifetime. This may not be
        /// unset.
        /// </summary>
        public void SetReadOnly()
        {
            _isReadOnly = true;
        }

        public Grid(GridBoundary parentBoundary, GridBoundary boundary)
        {
            if (parentBoundary.Left != 0 ||
                parentBoundary.Top != 0)
                throw new Exception("Trying to find offset parent boundaries Grid.cs");

            _offsetColumn = boundary.Left;
            _offsetRow = boundary.Top;
            _width = boundary.Width;
            _height = boundary.Height;
            _parentWidth = parentBoundary.Width;
            _parentHeight = parentBoundary.Height;
            _isReadOnly = false;

            this.Array2D = new T[_width, _height];
        }

        public Grid(IPropertyReader reader)
        {
            _offsetColumn = reader.Read<int>("OffsetColumn");
            _offsetRow = reader.Read<int>("OffsetRow");
            _width = reader.Read<int>("Width");
            _height = reader.Read<int>("Height");
            _parentWidth = reader.Read<int>("ParentWidth");
            _parentHeight = reader.Read<int>("ParentHeight");
            _isReadOnly = reader.Read<bool>("IsReadOnly");

            this.Array2D = new T[_width, _height];

            var count = reader.Read<int>("Count");

            for (int index = 0; index < count; index++)
            {
                var item = reader.Read<T>("Item" + index);
                var column = reader.Read<int>("Column" + index);
                var row = reader.Read<int>("Row" + index);

                this.Array2D[column, row] = item;
            }
        }

        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("OffsetColumn", _offsetColumn);
            writer.Write("OffsetRow", _offsetRow);
            writer.Write("Width", _width);
            writer.Write("Height", _height);
            writer.Write("ParentWidth", _parentWidth);
            writer.Write("ParentHeight", _parentHeight);
            writer.Write("IsReadOnly", _isReadOnly);

            var dict = new SimpleDictionary<(int, int), T>();

            // Gather the item data from the grid
            this.Array2D.Iterate((column, row) =>
            {
                if (ReferenceEquals(this.Array2D[column, row], null))
                    return Array2DCallback.Continue;

                dict.Add((column, row), this.Array2D[column, row]);

                return Array2DCallback.Continue;
            });

            // Serialize the item data
            writer.Write("Count", dict.Count);

            var counter = 0;

            foreach (var element in dict)
            {
                writer.Write("Item" + counter, element.Value);
                writer.Write("Column" + counter, element.Key.Item1);
                writer.Write("Row" + counter, element.Key.Item2);

                counter++;
            }
        }

        // Array Extension
        public IEnumerable<T> GetAdjacentElements(int column, int row)
        {
            return this.Array2D.GetAdjacentElements(column - _offsetColumn, row - _offsetRow);
        }
        public IEnumerable<T> GetCardinalAdjacentElements(int column, int row)
        {
            return this.Array2D.GetCardinalAdjacentElements(column - _offsetColumn, row - _offsetRow);
        }
        public IEnumerable<T> GetAdjacentElementsUnsafe(int column, int row)
        {
            return this.Array2D.GetAdjacentElementsUnsafe(column - _offsetColumn, row - _offsetRow);
        }
        public T GetOffDiagonalElement1(int column, int row, Compass direction, out Compass cardinalDirection1)
        {
            return this.Array2D.GetOffDiagonalElement1(column - _offsetColumn, row - _offsetRow, direction, out cardinalDirection1);
        }
        public T GetOffDiagonalElement2(int column, int row, Compass direction, out Compass cardinalDirection2)
        {
            return this.Array2D.GetOffDiagonalElement2(column - _offsetColumn, row - _offsetRow, direction, out cardinalDirection2);
        }
        public T GetOffDiagonalElement1(int column, int row, CompassConstrained direction, out CompassConstrained cardinalDirection1)
        {
            return this.Array2D.GetOffDiagonalElement1(column - _offsetColumn, row - _offsetRow, direction, out cardinalDirection1);
        }
        public T GetOffDiagonalElement2(int column, int row, CompassConstrained direction, out CompassConstrained cardinalDirection2)
        {
            return this.Array2D.GetOffDiagonalElement2(column - _offsetColumn, row - _offsetRow, direction, out cardinalDirection2);
        }
        public T GetElementInDirection(int column, int row, Compass direction)
        {
            return this.Array2D.GetElementInDirection(column - _offsetColumn, row - _offsetRow, direction);
        }
        public void IterateAround(int column, int row, int range, bool euclidean, GridCallback callback)
        {
            this.Array2D.IterateAround(column - _offsetColumn, row - _offsetRow, range, euclidean, (absoluteColumn, absoluteRow) =>
            {
                return callback(absoluteColumn + _offsetColumn, absoluteRow + _offsetRow);
            });
        }
        public void IterateAtFixedRadius(int column, int row, int range, bool euclidean, GridCallback callback)
        {
            this.Array2D.IterateAtFixedRadius(column - _offsetColumn, row - _offsetRow, range, euclidean, (absoluteColumn, absoluteRow) =>
            {
                return callback(absoluteColumn + _offsetColumn, absoluteRow + _offsetRow);
            });
        }
        public void Iterate(GridCallback callback)
        {
            this.Array2D.Iterate((column, row) =>
            {
                return callback(column + _offsetColumn, row + _offsetRow);
            });
        }

        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            var baseHash = RecursiveSerializerHashGenerator.CreateSimpleHash(_width, _height, _parentWidth, _parentHeight, _offsetColumn, _offsetRow, _isReadOnly);

            for (int column = 0; column < this.Array2D.GetLength(0); column++)
            {
                for (int row = 0; row < this.Array2D.GetLength(1); row++)
                {
                    if (ReferenceEquals(this.Array2D[column, row], null))
                        continue;

                    baseHash = RecursiveSerializerHashGenerator.CreateSimpleHash(baseHash, this.Array2D[column, row]);
                }
            }

            return baseHash;
        }
    }
}
