
using System;
using System.Collections.Generic;

using WpfCustomUtilities.RecursiveSerializer.Utility;
using WpfCustomUtilities.SimpleCollections.Array2D;
using WpfCustomUtilities.SimpleCollections.Grid.Interface;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    public class GridBoundary
    {
        public int Column { get; private set; }
        public int Row { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        public int Left { get { return this.Column; } }
        public int Right { get { return (this.Column + this.Width) - 1; } }
        public int Top { get { return this.Row; } }
        public int Bottom { get { return (this.Row + this.Height) - 1; } }

        public GridBoundary() : this(0, 0, 0, 0)
        {
        }
        public GridBoundary(GridBoundary boundary)
                : this(boundary.Column, boundary.Row, boundary.Width, boundary.Height)
        {
        }
        public GridBoundary(IGridLocator location, int cellwidth, int cellheight)
                : this(location.Column, location.Row, cellwidth, cellheight)
        {
        }
        public GridBoundary(int column, int row, int cellwidth, int cellheight)
        {
            if (column < 0 ||
                row < 0 ||
                cellwidth < 0 ||
                cellheight < 0)
                throw new ArgumentException("Trying to create illegal GridBoundary");

            this.Column = column;
            this.Row = row;
            this.Height = cellheight;
            this.Width = cellwidth;
        }

        /// <summary>
        /// Sets all grid boundary values
        /// </summary>
        public void Set(int column, int row, int cellwidth, int cellheight)
        {
            if (column < 0 ||
                row < 0 ||
                cellwidth < 0 ||
                cellheight < 0)
                throw new ArgumentException("Trying to create illegal GridBoundary");

            this.Column = column;
            this.Row = row;
            this.Width = cellwidth;
            this.Height = cellheight;
        }

        /// <summary>
        /// Sets all grid boundary values
        /// </summary>
        public void Set(GridBoundary boundary)
        {
            this.Column = boundary.Column;
            this.Row = boundary.Row;
            this.Width = boundary.Width;
            this.Height = boundary.Height;
        }

        public static GridBoundary Create(int left, int top, int right, int bottom)
        {
            return new GridBoundary(left, top, (right - left) + 1, (bottom - top) + 1);
        }

        /// <summary>
        /// Iterates over the index space of this region including { Left, Top, Right, Bottom }
        /// </summary>
        public void Iterate(GridBoundaryIterator iterator)
        {
            for (int column = this.Left; column <= this.Right; column++)
            {
                for (int row = this.Top; row <= this.Bottom; row++)
                {
                    if (iterator(column, row) == Array2DCallback.BreakAndReturn)
                        return;
                }
            }
        }

        public GridLocation GetCenter()
        {
            return new GridLocation(this.Left + (int)(this.Width / 2.0D),
                                    this.Top + (int)(this.Height / 2.0D));
        }

        public IEnumerable<GridLocation> GetCorners()
        {
            return new GridLocation[]
            {
                new GridLocation(this.Left, this.Top),
                new GridLocation(this.Right, this.Top),
                new GridLocation(this.Left, this.Bottom),
                new GridLocation(this.Right, this.Bottom)
            };
        }

        /// <summary>
        /// Includes the edge
        /// </summary>
        public bool Contains(IGridLocator location)
        {
            return Contains(location.Column, location.Row);
        }

        /// <summary>
        /// Includes Boundary
        /// </summary>
        public bool Contains(int column, int row)
        {
            if (column < this.Left)
                return false;

            if (column > this.Right)
                return false;

            if (row < this.Top)
                return false;

            if (row > this.Bottom)
                return false;

            return true;
        }
        public bool Contains(GridBoundary cellRectangle)
        {
            if (cellRectangle.Right > this.Right)
                return false;

            if (cellRectangle.Left < this.Left)
                return false;

            if (cellRectangle.Top < this.Top)
                return false;

            if (cellRectangle.Bottom > this.Bottom)
                return false;

            return true;
        }
        public bool Overlaps(GridBoundary boundary)
        {
            if (boundary.Left > this.Right)
                return false;

            if (boundary.Right < this.Left)
                return false;

            if (boundary.Top > this.Bottom)
                return false;

            if (boundary.Bottom < this.Top)
                return false;

            return true;
        }

        public bool Touches(GridBoundary boundary)
        {
            if (Overlaps(boundary))
                return true;

            // Expand by one cell and re-calculate overlaps
            //
            var topLeft = new GridLocation(this.Left - 1, this.Top - 1);
            var biggerBoundary = new GridBoundary(topLeft, this.Width + 2, this.Height + 2);

            return biggerBoundary.Overlaps(boundary);
        }

        /// <summary>
        /// Expands GridBoundary to include the specified region boundary INCLUSIVELY (boundaries
        /// at least match)
        /// </summary>
        public void Expand(GridBoundary boundary)
        {
            var left = System.Math.Min(this.Left, boundary.Left);
            var top = System.Math.Min(this.Top, boundary.Top);
            var right = System.Math.Max(this.Right, boundary.Right);
            var bottom = System.Math.Max(this.Bottom, boundary.Bottom);

            if (left < 0 || top < 0)
                throw new Exception("Trying to expand GridBoundary to invalid space");

            if (left < this.Left)
                this.Column = left;

            if (right > this.Right)
                this.Width += right - this.Right;

            if (top < this.Top)
                this.Row = top;

            if (bottom > this.Bottom)
                this.Height += bottom - this.Bottom;
        }

        /// <summary>
        /// Expands GridBoundary to include specified location
        /// </summary>
        public void Expand(int column, int row)
        {
            var left = System.Math.Min(this.Left, column);
            var top = System.Math.Min(this.Top, row);
            var right = System.Math.Max(this.Right, column);
            var bottom = System.Math.Max(this.Bottom, row);

            if (left < 0 || top < 0)
                throw new Exception("Trying to expand GridBoundary to invalid space");

            if (left < this.Left)
                this.Column = left;

            if (right > this.Right)
                this.Width += right - this.Right;

            if (top < this.Top)
                this.Row = top;

            if (bottom > this.Bottom)
                this.Height += bottom - this.Bottom;
        }

        /// <summary>
        /// Expands GridBoundary by the specified amount
        /// </summary>
        public GridBoundary Expand(int padding)
        {
            var left = this.Left - padding;
            var top = this.Top - padding;
            var right = this.Right + padding;
            var bottom = this.Bottom + padding;

            if (left < 0 || top < 0)
                throw new Exception("Trying to expand GridBoundary to invalid space");

            return new GridBoundary(left, top, (right - left) + 1, (bottom - top) + 1);
        }

        public bool IsLeftOf(GridBoundary boundary)
        {
            return this.Right < boundary.Left;
        }
        public bool IsRightOf(GridBoundary boundary)
        {
            return this.Left > boundary.Right;
        }
        public bool IsBelow(GridBoundary boundary)
        {
            return this.Top > boundary.Bottom;
        }
        public bool IsAbove(GridBoundary boundary)
        {
            return this.Bottom < boundary.Top;
        }
        public static bool operator ==(GridBoundary regionBoundary1, GridBoundary regionBoundary2)
        {
            if (ReferenceEquals(regionBoundary1, null))
                return ReferenceEquals(regionBoundary2, null);

            else if (ReferenceEquals(regionBoundary2, null))
                return false;

            else
                return regionBoundary1.Equals(regionBoundary2);
        }
        public static bool operator !=(GridBoundary regionBoundary1, GridBoundary regionBoundary2)
        {
            return !(regionBoundary1 == regionBoundary2);
        }
        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.Column,
                                                                       this.Row,
                                                                       this.Height,
                                                                       this.Width);
        }
        public override string ToString()
        {
            return "X=" + Column + " Y=" + Row + " Width=" + Width + " Height=" + Height;
        }
    }
}
