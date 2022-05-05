
using System;
using System.Collections.Generic;

using WpfCustomUtilities.SimpleCollections.Array2D;
using WpfCustomUtilities.SimpleCollections.Grid.Interface;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    public static class GridExtension
    {
        /// <summary>
        /// Performs a boundary-safe indexing operation to get the cell from the grid. Returns default(T) if out-of-bounds
        /// or no cell was at that location.
        /// </summary>
        private static T Get<T>(this Grid<T> grid, int column, int row)
        {
            if (!grid.IsDefined(column, row))
                return default(T);

            return grid[column, row];
        }

        /// <summary>
        /// Special "Get" method to check the specified boundary for the sub-grid within the specified grid. This is used for region grids
        /// that have the same dimensions as their parent. Returns null if outside the region OR grid boundaries.
        /// </summary>
        private static T GetFrom<T>(this Grid<T> grid, int regionColumn, int regionRow, int regionWidth, int regionHeight, int column, int row)
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
        /// Checks for grid adjacency using an AND mask with the provided compass constrained direction.
        /// </summary>
        public static bool IsAdjacencyDefined<T>(this Grid<T> grid, int column, int row, CompassConstrained directionMask)
        {
            var result = true;

            if (directionMask == CompassConstrained.Null)
                throw new Exception("Invalid compass constrained direction:  GridExtension.IsDefinedMasked");

            if (directionMask.HasFlag(CompassConstrained.N))
                result &= grid.IsDefined(column, row - 1);

            if (directionMask.HasFlag(CompassConstrained.S))
                result &= grid.IsDefined(column, row + 1);

            if (directionMask.HasFlag(CompassConstrained.E))
                result &= grid.IsDefined(column + 1, row);

            if (directionMask.HasFlag(CompassConstrained.W))
                result &= grid.IsDefined(column - 1, row);

            if (directionMask.HasFlag(CompassConstrained.NE))
                result &= grid.IsDefined(column + 1, row - 1);

            if (directionMask.HasFlag(CompassConstrained.NW))
                result &= grid.IsDefined(column - 1, row - 1);

            if (directionMask.HasFlag(CompassConstrained.SE))
                result &= grid.IsDefined(column + 1, row + 1);

            if (directionMask.HasFlag(CompassConstrained.SW))
                result &= grid.IsDefined(column - 1, row + 1);

            return result;
        }

        public static T GetAdjacentElement<T>(this Grid<T> grid, int column, int row, Compass direction)
        {
            switch (direction)
            {
                case Compass.Null:
                    return grid[column, row];
                case Compass.N:
                    return grid.IsDefined(column, row - 1) ? grid[column, row - 1] : default(T);
                case Compass.S:
                    return grid.IsDefined(column, row + 1) ? grid[column, row + 1] : default(T);
                case Compass.E:
                    return grid.IsDefined(column + 1, row) ? grid[column + 1, row] : default(T);
                case Compass.W:
                    return grid.IsDefined(column - 1, row) ? grid[column - 1, row] : default(T);
                case Compass.NW:
                    return grid.IsDefined(column - 1, row - 1) ? grid[column - 1, row - 1] : default(T);
                case Compass.NE:
                    return grid.IsDefined(column + 1, row - 1) ? grid[column + 1, row - 1] : default(T);
                case Compass.SE:
                    return grid.IsDefined(column + 1, row + 1) ? grid[column + 1, row + 1] : default(T);
                case Compass.SW:
                    return grid.IsDefined(column - 1, row + 1) ? grid[column - 1, row + 1] : default(T);
                default:
                    throw new Exception("Unhandled compass direction:  GridExtension.cs");
            }
        }

        public static IGridLocator GetAdjacentElementLocatorUnsafe<T>(this Grid<T> grid, int column, int row, Compass direction)
        {
            switch (direction)
            {
                case Compass.Null:
                    return grid.GetLocator(column, row);
                case Compass.N:
                {
                    if (grid.BoundaryContains(column, row - 1))
                    {
                        return grid.GetLocator(column, row - 1);
                    }
                    else
                        return null;
                }
                case Compass.S:
                {
                    if (grid.BoundaryContains(column, row + 1))
                    {
                        return grid.GetLocator(column, row + 1);
                    }
                    else
                        return null;
                }
                case Compass.E:
                {
                    if (grid.BoundaryContains(column + 1, row))
                    {
                        return grid.GetLocator(column + 1, row);
                    }
                    else
                        return null;
                }
                case Compass.W:
                {
                    if (grid.BoundaryContains(column - 1, row))
                    {
                        return grid.GetLocator(column - 1, row);
                    }
                    else
                        return null;
                }
                case Compass.NW:
                {
                    if (grid.BoundaryContains(column - 1, row - 1))
                    {
                        return grid.GetLocator(column - 1, row - 1);
                    }
                    else
                        return null;
                }
                case Compass.NE:
                {
                    if (grid.BoundaryContains(column + 1, row - 1))
                    {
                        return grid.GetLocator(column + 1, row - 1);
                    }
                    else
                        return null;
                }
                case Compass.SE:
                {
                    if (grid.BoundaryContains(column + 1, row + 1))
                    {
                        return grid.GetLocator(column + 1, row + 1);
                    }
                    else
                        return null;
                }
                case Compass.SW:
                {
                    if (grid.BoundaryContains(column - 1, row + 1))
                    {
                        return grid.GetLocator(column - 1, row + 1);
                    }
                    else
                        return null;
                }
                default:
                    throw new Exception("Unhandled compass direction:  GridExtension.cs");
            }
        }

        /// <summary>
        /// Returns locators for the adjacent elements whose values are non-null; and in bounds
        /// </summary>
        public static IGridLocator[] GetAdjacentElementLocators<T>(this Grid<T> grid, int column, int row)
        {
            var n = grid.GetSafe(column, row - 1);
            var s = grid.GetSafe(column, row + 1);
            var e = grid.GetSafe(column + 1, row);
            var w = grid.GetSafe(column - 1, row);
            var ne = grid.GetSafe(column + 1, row - 1);
            var nw = grid.GetSafe(column - 1, row - 1);
            var se = grid.GetSafe(column + 1, row + 1);
            var sw = grid.GetSafe(column - 1, row + 1);

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

            var result = new IGridLocator[count];
            var index = 0;

            if (n != null) result[index++] = grid.GetLocator(column, row - 1);
            if (s != null) result[index++] = grid.GetLocator(column, row + 1);
            if (e != null) result[index++] = grid.GetLocator(column + 1, row);
            if (w != null) result[index++] = grid.GetLocator(column - 1, row);
            if (ne != null) result[index++] = grid.GetLocator(column + 1, row - 1);
            if (nw != null) result[index++] = grid.GetLocator(column - 1, row - 1);
            if (se != null) result[index++] = grid.GetLocator(column + 1, row + 1);
            if (sw != null) result[index++] = grid.GetLocator(column - 1, row + 1);

            return result;
        }

        /// <summary>
        /// Returns locators for the adjacent elements - checking grid boundary; but leaving in nulls.
        /// </summary>
        public static IGridLocator[] GetAdjacentElementLocatorsUnsafe<T>(this Grid<T> grid, int column, int row)
        {
            var count = 0;
            var locators = new IGridLocator[8];

            // N
            if (grid.BoundaryContains(column, row - 1))
            {
                locators[count++] = grid.GetLocator(column, row - 1);
            }
            // S
            if (grid.BoundaryContains(column, row + 1))
            {
                locators[count++] = grid.GetLocator(column, row + 1);
            }
            // E
            if (grid.BoundaryContains(column + 1, row))
            {
                locators[count++] = grid.GetLocator(column + 1, row);
            }
            // W
            if (grid.BoundaryContains(column - 1, row))
            {
                locators[count++] = grid.GetLocator(column - 1, row);
            }
            // NW
            if (grid.BoundaryContains(column - 1, row - 1))
            {
                locators[count++] = grid.GetLocator(column - 1, row - 1);
            }
            // NE
            if (grid.BoundaryContains(column + 1, row - 1))
            {
                locators[count++] = grid.GetLocator(column + 1, row - 1);
            }
            // SE
            if (grid.BoundaryContains(column + 1, row + 1))
            {
                locators[count++] = grid.GetLocator(column + 1, row + 1);
            }
            // SW
            if (grid.BoundaryContains(column - 1, row + 1))
            {
                locators[count++] = grid.GetLocator(column - 1, row + 1);
            }

            var result = new IGridLocator[count];

            for (int index = 0; index < count; index++)
                result[index] = locators[index];

            return result;
        }

        /// <summary>
        /// Returns locators for the adjacent elements whose values are non-null; and in bounds
        /// </summary>
        public static IGridLocator[] GetCardinalAdjacentElementLocators<T>(this Grid<T> grid, int column, int row)
        {
            var n = grid.GetSafe(column, row - 1);
            var s = grid.GetSafe(column, row + 1);
            var e = grid.GetSafe(column + 1, row);
            var w = grid.GetSafe(column - 1, row);

            // Need this to be optimized for speed
            var count = 0;
            if (n != null) count++;
            if (s != null) count++;
            if (e != null) count++;
            if (w != null) count++;

            var result = new IGridLocator[count];
            var index = 0;

            if (n != null) result[index++] = grid.GetLocator(column, row - 1);
            if (s != null) result[index++] = grid.GetLocator(column, row + 1);
            if (e != null) result[index++] = grid.GetLocator(column + 1, row);
            if (w != null) result[index++] = grid.GetLocator(column - 1, row);

            return result;
        }

        /// <summary>
        /// Returns locators for the adjacent elements - staying in bounds; but leaving null valued locations in the result.
        /// </summary>
        public static IGridLocator[] GetCardinalAdjacentElementLocatorsUnsafe<T>(this Grid<T> grid, int column, int row)
        {
            var count = 0;
            var locators = new IGridLocator[4];

            // N
            if (grid.BoundaryContains(column, row - 1))
            {
                locators[count++] = grid.GetLocator(column, row - 1);
            }
            // S
            if (grid.BoundaryContains(column, row + 1))
            {
                locators[count++] = grid.GetLocator(column, row + 1);
            }
            // E
            if (grid.BoundaryContains(column + 1, row))
            {
                locators[count++] = grid.GetLocator(column + 1, row);
            }
            // W
            if (grid.BoundaryContains(column - 1, row))
            {
                locators[count++] = grid.GetLocator(column - 1, row);
            }

            var result = new IGridLocator[count];

            for (int index = 0; index < count; index++)
                result[index] = locators[index];

            return result;
        }

        public static bool AreAdjacentElements<T>(this Grid<T> grid, IGridLocator location, IGridLocator otherLocation)
        {
            if (!grid.IsDefined(location.Column, location.Row))
                return false;

            if (!grid.IsDefined(otherLocation.Column, otherLocation.Row))
                return false;

            if (Math.Abs(otherLocation.Column - location.Column) > 1)
                return false;

            if (Math.Abs(otherLocation.Row - location.Row) > 1)
                return false;

            return true;
        }

        public static void Populate<T>(this Grid<T> grid, GridResult<T> itemCallback)
        {
            if (grid.IsReadOnly())
                throw new Exception("Trying to call Populate() on a Grid<> marked read-only");

            grid.Iterate((column, row) =>
            {
                grid[column, row] = itemCallback(column, row);

                return Array2DCallback.Continue;
            });
        }

        public static IEnumerable<V> Select<T, V>(this Grid<T> grid, GridPredicate<T> predicate, GridSelector<T, V> selector)
        {
            var result = new List<V>();

            grid.Iterate((column, row) =>
            {
                var item = grid[column, row];

                if (predicate(column, row, item))
                    result.Add(selector(column, row, item));

                return Array2DCallback.Continue;
            });

            return result;
        }

        public static Grid<V> SelectGrid<T, V>(this Grid<T> grid, GridPredicate<T> predicate, GridSelector<T, V> selector)
        {
            var result = new Grid<V>(grid.GetParentBoundary(), grid.GetRelativeBoundary());

            grid.Iterate((column, row) =>
            {
                var item = grid[column, row];

                if (predicate(column, row, item))
                    result[column, row] = selector(column, row, item);

                return Array2DCallback.Continue;
            });

            return result;
        }

        /// <summary>
        /// Returns true if any adjacent elements are negative with respect to the provided predicate OR are 
        /// out of bounds OR are null.
        /// </summary>
        public static bool IsEdgeElement<T>(this Grid<T> grid, int column, int row, GridPredicate<T> predicate)
        {
            var north = grid.Get(column, row - 1);
            var south = grid.Get(column, row + 1);
            var east = grid.Get(column + 1, row);
            var west = grid.Get(column - 1, row);
            var northEast = grid.Get(column + 1, row - 1);
            var northWest = grid.Get(column - 1, row - 1);
            var southEast = grid.Get(column + 1, row + 1);
            var southWest = grid.Get(column - 1, row + 1);

            return (north == null || (north != null && !predicate(column, row - 1, north))) ||
                   (south == null || (south != null && !predicate(column, row + 1, south))) ||
                   (east == null || (east != null && !predicate(column + 1, row, east))) ||
                   (west == null || (west != null && !predicate(column - 1, row, west))) ||
                   (northEast == null || (northEast != null && !predicate(column + 1, row - 1, northEast))) ||
                   (northWest == null || (northWest != null && !predicate(column - 1, row - 1, northWest))) ||
                   (southEast == null || (southEast != null && !predicate(column + 1, row + 1, southEast))) ||
                   (southWest == null || (southWest != null && !predicate(column - 1, row + 1, southWest)));
        }

        /// <summary>
        /// Returns true if any adjacent elements are positive with respect to the provided predicate AND SUB-BOUNDARY OR are 
        /// out of bounds OR are null.
        /// </summary>
        public static bool IsEdgeElement<T>(this Grid<T> grid, int column, int row, int regionColumn, int regionRow, int regionWidth, int regionHeight, GridPredicate<T> predicate)
        {
            var north = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column, row - 1);
            var south = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column, row + 1);
            var east = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column + 1, row);
            var west = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column - 1, row);
            var northEast = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column + 1, row - 1);
            var northWest = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column - 1, row - 1);
            var southEast = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column + 1, row + 1);
            var southWest = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column - 1, row + 1);

            return (north == null || (north != null && !predicate(column, row - 1, north))) ||
                   (south == null || (south != null && !predicate(column, row + 1, south))) ||
                   (east == null || (east != null && !predicate(column + 1, row, east))) ||
                   (west == null || (west != null && !predicate(column - 1, row, west))) ||
                   (northEast == null || (northEast != null && !predicate(column + 1, row - 1, northEast))) ||
                   (northWest == null || (northWest != null && !predicate(column - 1, row - 1, northWest))) ||
                   (southEast == null || (southEast != null && !predicate(column + 1, row + 1, southEast))) ||
                   (southWest == null || (southWest != null && !predicate(column - 1, row + 1, southWest)));
        }

        /// <summary>
        /// Returns true if the adjacent element is positive with respect to the provided predicate OR is
        /// out of bounds OR is null FOR the provided direction.
        /// </summary>
        /// <param name="direction">Compass direction treated with DIRECT EQUALITY! (DOESN'T USE FLAGS)</param>
        public static bool IsExposedEdgeElement<T>(this Grid<T> grid, int column, int row, Compass direction, GridPredicate<T> predicate)
        {
            var north = grid.Get(column, row - 1);
            var south = grid.Get(column, row + 1);
            var east = grid.Get(column + 1, row);
            var west = grid.Get(column - 1, row);

            if (direction == Compass.N)
                return north == null || (north != null && !predicate(column, row - 1, north));

            else if (direction == Compass.S)
                return south == null || (south != null && !predicate(column, row + 1, south));

            else if (direction == Compass.E)
                return east == null || (east != null && !predicate(column + 1, row, east));

            else if (direction == Compass.W)
                return west == null || (west != null && !predicate(column - 1, row, west));

            else
                throw new Exception("Invalid use of direction parameter:  ArrayExtension.IsExposedEdgeElement");
        }

        /// <summary>
        /// Returns true if the adjacent element is positive with respect to the provided predicate AND SUB-BOUNDARY OR is
        /// out of bounds OR is null FOR the provided direction.
        /// </summary>
        /// <param name="direction">Compass direction treated with DIRECT EQUALITY! (DOESN'T USE FLAGS)</param>
        public static bool IsExposedEdgeElement<T>(this Grid<T> grid, int column, int row, Compass direction, int regionColumn, int regionRow, int regionWidth, int regionHeight, GridPredicate<T> predicate)
        {
            var north = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column, row - 1);
            var south = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column, row + 1);
            var east = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column + 1, row);
            var west = grid.GetFrom(regionColumn, regionRow, regionWidth, regionHeight, column - 1, row);

            if (direction == Compass.N)
                return north == null || (north != null && !predicate(column, row - 1, north));

            else if (direction == Compass.S)
                return south == null || (south != null && !predicate(column, row + 1, south));

            else if (direction == Compass.E)
                return east == null || (east != null && !predicate(column + 1, row, east));

            else if (direction == Compass.W)
                return west == null || (west != null && !predicate(column - 1, row, west));

            else
                throw new Exception("Invalid use of direction parameter:  ArrayExtension.IsExposedEdgeElement");
        }

        /// <summary>
        /// Returns true if the adjacent element is positive with respect to the provided predicate OR is
        /// out of bounds OR is null FOR the provided NON-CARDINAL direction.
        /// </summary>
        /// <param name="direction">Compass direction treated with DIRECT EQUALITY! (DOESN'T USE FLAGS)</param>
        public static bool IsExposedCornerElement<T>(this Grid<T> grid, int column, int row, Compass direction, GridPredicate<T> predicate)
        {
            if (direction == Compass.NW)
                return IsExposedEdgeElement(grid, column, row, Compass.N, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.W, predicate);

            else if (direction == Compass.NE)
                return IsExposedEdgeElement(grid, column, row, Compass.N, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.E, predicate);

            else if (direction == Compass.SE)
                return IsExposedEdgeElement(grid, column, row, Compass.S, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.E, predicate);

            else if (direction == Compass.SW)
                return IsExposedEdgeElement(grid, column, row, Compass.S, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.W, predicate);

            else
                throw new Exception("Invalid use of direction parameter:  ArrayExtension.IsExposedCornerElement");
        }

        /// <summary>
        /// Returns true if the adjacent element is positive with respect to the provided predicate AND SUB-BOUNDARY OR is
        /// out of bounds OR is null FOR the provided NON-CARDINAL direction.
        /// </summary>
        /// <param name="direction">Compass direction treated with DIRECT EQUALITY! (DOESN'T USE FLAGS)</param>
        public static bool IsExposedCornerElement<T>(this Grid<T> grid, int column, int row, Compass direction, int regionColumn, int regionRow, int regionWidth, int regionHeight, GridPredicate<T> predicate)
        {
            if (direction == Compass.NW)
                return IsExposedEdgeElement(grid, column, row, Compass.N, regionColumn, regionRow, regionWidth, regionHeight, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.W, regionColumn, regionRow, regionWidth, regionHeight, predicate);

            else if (direction == Compass.NE)
                return IsExposedEdgeElement(grid, column, row, Compass.N, regionColumn, regionRow, regionWidth, regionHeight, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.E, regionColumn, regionRow, regionWidth, regionHeight, predicate);

            else if (direction == Compass.SE)
                return IsExposedEdgeElement(grid, column, row, Compass.S, regionColumn, regionRow, regionWidth, regionHeight, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.E, regionColumn, regionRow, regionWidth, regionHeight, predicate);

            else if (direction == Compass.SW)
                return IsExposedEdgeElement(grid, column, row, Compass.S, regionColumn, regionRow, regionWidth, regionHeight, predicate) &&
                       IsExposedEdgeElement(grid, column, row, Compass.W, regionColumn, regionRow, regionWidth, regionHeight, predicate);

            else
                throw new Exception("Invalid use of direction parameter:  ArrayExtension.IsExposedCornerElement");
        }
    }
}
