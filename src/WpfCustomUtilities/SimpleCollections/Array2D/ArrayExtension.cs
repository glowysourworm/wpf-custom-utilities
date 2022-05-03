﻿using System;

using WpfCustomUtilities.SimpleCollections.Grid;

namespace WpfCustomUtilities.SimpleCollections.Array2D
{
    public static class ArrayExtension
    {
        /// <summary>
        /// Performs a boundary-safe indexing operation to get the cell from the grid. Returns default(T) if out-of-bounds
        /// or no cell was at that location.
        /// </summary>
        public static T Get<T>(this T[,] grid, int column, int row)
        {
            if (column < 0 ||
                column >= grid.GetLength(0) ||
                row < 0 ||
                row >= grid.GetLength(1))
                return default(T);

            return grid[column, row];
        }

        /// <summary>
        /// Special "Get" method to check the specified boundary for the sub-grid within the specified grid. This is used for region grids
        /// that have the same dimensions as their parent. Returns null if outside the region OR grid boundaries.
        /// </summary>
        public static T GetFrom<T>(this T[,] grid, int regionColumn, int regionRow, int regionWidth, int regionHeight, int column, int row)
        {
            // Check region boundaries
            if (column < regionColumn || column > ((regionColumn + regionWidth) - 1))
                return default(T);

            if (row < regionRow || row > ((regionRow + regionHeight) - 1))
                return default(T);

            // Check parent grid boundaries
            return Get(grid, column, row);
        }

        /// <summary>
        /// Returns 4-way adjacent cells - nulls excluded
        /// </summary>
        public static T[] GetCardinalAdjacentElements<T>(this T[,] grid, int column, int row)
        {
            var n = grid.Get(column, row - 1);
            var s = grid.Get(column, row + 1);
            var e = grid.Get(column + 1, row);
            var w = grid.Get(column - 1, row);

            // Need this to be optimized for speed
            var count = 0;
            if (n != null) count++;
            if (s != null) count++;
            if (e != null) count++;
            if (w != null) count++;

            var result = new T[count];
            var index = 0;

            if (n != null) result[index++] = n;
            if (s != null) result[index++] = s;
            if (e != null) result[index++] = e;
            if (w != null) result[index++] = w;

            return result;
        }

        /// <summary>
        /// Returns 8-way adjacent cells - stripping out nulls
        /// </summary>
        public static T[] GetAdjacentElements<T>(this T[,] grid, int column, int row)
        {
            var n = grid.Get(column, row - 1);
            var s = grid.Get(column, row + 1);
            var e = grid.Get(column + 1, row);
            var w = grid.Get(column - 1, row);
            var ne = grid.Get(column + 1, row - 1);
            var nw = grid.Get(column - 1, row - 1);
            var se = grid.Get(column + 1, row + 1);
            var sw = grid.Get(column - 1, row + 1);

            // Need this to be optimized for speed
            var count = 0;
            if (n != null) count++;
            if (s != null) count++;
            if (e != null) count++;
            if (w != null) count++;
            if (ne != null) count++;
            if (nw != null) count++;
            if (se != null) count++;
            if (sw != null) count++;

            var result = new T[count];
            var index = 0;

            if (n != null) result[index++] = n;
            if (s != null) result[index++] = s;
            if (e != null) result[index++] = e;
            if (w != null) result[index++] = w;
            if (ne != null) result[index++] = ne;
            if (nw != null) result[index++] = nw;
            if (se != null) result[index++] = se;
            if (sw != null) result[index++] = sw;

            return result;
        }

        /// <summary>
        /// Returns adjacent elements that are reachable by a cardinal one. Example:  NW is connected if it is
        /// non-null; and N is also non-null.
        /// </summary>
        public static T[] GetAdjacentElementsWithCardinalConnection<T>(this T[,] grid, int column, int row)
        {
            var n = grid.Get(column, row - 1);
            var s = grid.Get(column, row + 1);
            var e = grid.Get(column + 1, row);
            var w = grid.Get(column - 1, row);
            var ne = grid.Get(column + 1, row - 1);
            var nw = grid.Get(column - 1, row - 1);
            var se = grid.Get(column + 1, row + 1);
            var sw = grid.Get(column - 1, row + 1);

            var count = 0;

            if (n != null) count++;
            if (s != null) count++;
            if (e != null) count++;
            if (w != null) count++;
            if (ne != null && (n != null || e != null)) count++;
            if (nw != null && (n != null || w != null)) count++;
            if (se != null && (s != null || e != null)) count++;
            if (sw != null && (s != null || w != null)) count++;

            var result = new T[count];
            var index = 0;

            if (n != null) result[index++] = n;
            if (s != null) result[index++] = s;
            if (e != null) result[index++] = e;
            if (w != null) result[index++] = w;
            if (ne != null && (n != null || e != null)) result[index++] = ne;
            if (nw != null && (n != null || w != null)) result[index++] = nw;
            if (se != null && (s != null || e != null)) result[index++] = se;
            if (sw != null && (s != null || w != null)) result[index++] = sw;

            return result;
        }

        /// <summary>
        /// Returns all elements within the specified distance from the given location (using the Roguian metric). Method does
        /// not strip out null elements; but checks boundaries to prevent exceptions.
        /// </summary>
        public static T[] GetElementsNearUnsafe<T>(this T[,] grid, int column, int row, int distance)
        {
            // Calculate the array result length
            var sumSequence = 0;
            for (int i = 1; i <= distance; i++)
                sumSequence += i;

            // Circumference of a "circle" of "radius = distance" is 8 * distance. So, Area = Sum of Circumferences from 1 -> distance
            //
            var result = new T[8 * sumSequence];
            var index = 0;

            for (int k = 1; k <= distance; k++)
            {
                // Iterate top and bottom
                for (int i = column - k; i <= column + k; i++)
                {
                    // Top
                    result[index++] = grid.Get(i, row - k);

                    // Bottom
                    result[index++] = grid.Get(i, row + k);
                }

                // Iterate left and right (minus the corners)
                for (int j = row - k + 1; j <= row + k - 1; j++)
                {
                    // Left 
                    result[index++] = grid.Get(column - k, j);

                    // Right
                    result[index++] = grid.Get(column + k, j);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns 8-way adjacent cells - leaving nulls; but checking boundaries to prevent exceptions. 
        /// NOTE*** This will return null references for possible element positions ONLY.
        /// </summary>
        public static T[] GetAdjacentElementsUnsafe<T>(this T[,] grid, int column, int row)
        {
            var n = grid.Get(column, row - 1);
            var s = grid.Get(column, row + 1);
            var e = grid.Get(column + 1, row);
            var w = grid.Get(column - 1, row);
            var ne = grid.Get(column + 1, row - 1);
            var nw = grid.Get(column - 1, row - 1);
            var se = grid.Get(column + 1, row + 1);
            var sw = grid.Get(column - 1, row + 1);

            // NW Corner
            if (row - 1 < 0 &&
                column - 1 < 0)
                return new T[] { s, e, se };

            // NE Corner
            if (row - 1 < 0 &&
                column + 1 >= grid.GetLength(0))
                return new T[] { s, w, sw };

            // SE Corner
            if (row + 1 >= grid.GetLength(1) &&
                column + 1 >= grid.GetLength(0))
                return new T[] { n, w, nw };

            // SW Corner
            if (row + 1 >= grid.GetLength(1) &&
                column - 1 < 0)
                return new T[] { n, e, ne };

            // N Boundary
            if (row - 1 < 0)
                return new T[] { s, e, w, se, sw };

            // S Boundary
            if (row + 1 >= grid.GetLength(1))
                return new T[] { n, e, w, ne, nw };

            // E Boundary
            if (column + 1 >= grid.GetLength(0))
                return new T[] { n, s, w, nw, sw };

            // W Boundary
            if (column - 1 < 0)
                return new T[] { n, s, e, ne, se };

            return new T[] { n, s, e, w, ne, nw, se, sw };
        }

        /// <summary>
        /// Returns 4-way adjacent cells - leaving nulls; but checking boundaries to prevent exceptions. 
        /// NOTE*** This will return null references for possible element positions ONLY.
        /// </summary>
        public static T[] GetCardinalAdjacentElementsUnsafe<T>(this T[,] grid, int column, int row)
        {
            var n = grid.Get(column, row - 1);
            var s = grid.Get(column, row + 1);
            var e = grid.Get(column + 1, row);
            var w = grid.Get(column - 1, row);

            return new T[] { n, s, e, w };
        }

        /// <summary>
        /// Returns 1st of 2 off diagonal elements in the specified non-cardinal direction (Example: NE -> N element)
        /// </summary>
        /// <param name="direction">NE, NW, SE, SW</param>
        public static T GetOffDiagonalElement1<T>(this T[,] grid, int column, int row, Compass direction, out Compass cardinalDirection1)
        {
            switch (direction)
            {
                case Compass.NE:
                case Compass.NW:
                    cardinalDirection1 = Compass.N;
                    return grid.Get(column, row - 1);
                case Compass.SE:
                case Compass.SW:
                    cardinalDirection1 = Compass.S;
                    return grid.Get(column, row + 1);
                default:
                    throw new Exception("Off-Diagonal directions don't include " + direction.ToString());
            }
        }

        /// <summary>
        /// Returns 2nd of 2 off diagonal elements in the specified non-cardinal direction (Example: NE -> E element)
        /// </summary>
        /// <param name="direction">NE, NW, SE, SW</param>
        public static T GetOffDiagonalElement2<T>(this T[,] grid, int column, int row, Compass direction, out Compass cardinalDirection2)
        {
            switch (direction)
            {
                case Compass.NE:
                case Compass.SE:
                    cardinalDirection2 = Compass.E;
                    return grid.Get(column + 1, row);
                case Compass.SW:
                case Compass.NW:
                    cardinalDirection2 = Compass.W;
                    return grid.Get(column - 1, row);
                default:
                    throw new Exception("Off-Diagonal directions don't include " + direction.ToString());
            }
        }

        /// <summary>
        /// Returns 1st of 2 off diagonal elements in the specified non-cardinal direction (Example: NE -> N element)
        /// </summary>
        /// <param name="direction">NE, NW, SE, SW</param>
        public static T GetOffDiagonalElement1<T>(this T[,] grid, int column, int row, CompassConstrained direction, out CompassConstrained cardinalDirection1)
        {
            switch (direction)
            {
                case CompassConstrained.NE:
                case CompassConstrained.NW:
                    cardinalDirection1 = CompassConstrained.N;
                    return grid.Get(column, row - 1);
                case CompassConstrained.SE:
                case CompassConstrained.SW:
                    cardinalDirection1 = CompassConstrained.S;
                    return grid.Get(column, row + 1);
                default:
                    throw new Exception("Off-Diagonal directions don't include " + direction.ToString());
            }
        }

        /// <summary>
        /// Returns 2nd of 2 off diagonal elements in the specified non-cardinal direction (Example: NE -> E element)
        /// </summary>
        /// <param name="direction">NE, NW, SE, SW</param>
        public static T GetOffDiagonalElement2<T>(this T[,] grid, int column, int row, CompassConstrained direction, out CompassConstrained cardinalDirection2)
        {
            switch (direction)
            {
                case CompassConstrained.NE:
                case CompassConstrained.SE:
                    cardinalDirection2 = CompassConstrained.E;
                    return grid.Get(column + 1, row);
                case CompassConstrained.SW:
                case CompassConstrained.NW:
                    cardinalDirection2 = CompassConstrained.W;
                    return grid.Get(column - 1, row);
                default:
                    throw new Exception("Off-Diagonal directions don't include " + direction.ToString());
            }
        }

        /// <summary>
        /// Returns adjacent element in the direction specified
        /// </summary>
        /// <typeparam name="T">Type of grid element</typeparam>
        /// <param name="grid">The input 2D array</param>
        /// <param name="column">The column of the location</param>
        /// <param name="row">The row of the location</param>
        /// <param name="direction">The direction to look adjacent to specified location</param>
        public static T GetElementInDirection<T>(this T[,] grid, int column, int row, Compass direction)
        {
            switch (direction)
            {
                case Compass.N:
                    return grid.Get(column, row - 1);
                case Compass.S:
                    return grid.Get(column, row + 1);
                case Compass.E:
                    return grid.Get(column + 1, row);
                case Compass.W:
                    return grid.Get(column - 1, row);
                case Compass.NW:
                    return grid.Get(column - 1, row - 1);
                case Compass.NE:
                    return grid.Get(column + 1, row - 1);
                case Compass.SE:
                    return grid.Get(column + 1, row + 1);
                case Compass.SW:
                    return grid.Get(column - 1, row + 1);
                case Compass.Null:
                default:
                    throw new Exception("Unhandled direction ArrayExtension.GetElementInDirection");
            }
        }
    }
}
