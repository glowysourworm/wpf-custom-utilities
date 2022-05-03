
using System;
using System.Collections.Generic;

using WpfCustomUtilities.Extensions;

namespace WpfCustomUtilities.SimpleCollections.Array2D
{
    public static class Array2DExtension
    {
        /// <summary>
        /// Iterates the grid entirely with the provided callback
        /// </summary>
        public static void Iterate<T>(this T[,] grid, GridCallback callback)
        {
            for (int column = 0; column < grid.GetLength(0); column++)
            {
                for (int row = 0; row < grid.GetLength(1); row++)
                {
                    if (callback(column, row) == Array2DCallback.BreakAndReturn)
                        return;
                }
            }
        }

        public static void Populate<T>(this T[,] grid, GridResult<T> itemCallback)
        {
            grid.Iterate((column, row) =>
            {
                grid[column, row] = itemCallback(column, row);

                return Array2DCallback.Continue;
            });
        }


        /// <summary>
        /// Iterates - starting at the provided column and row - outwards to the specified radius - checking the grid
        /// boundary as it goes.
        /// </summary>
        public static void IterateAround<T>(this T[,] grid, int column, int row, int maxRadius, bool euclidean, GridCallback continuationPredicate)
        {
            // Take the MIN of the(max radius, MAX(grid dimensions))
            var safeMaxRadius = Math.Min(maxRadius, Math.Max(grid.GetLength(0), grid.GetLength(1)));

            for (int radius = 0; radius <= safeMaxRadius; radius++)
            {
                var left = (column - radius).Clip(0, grid.GetLength(0) - 1);
                var right = (column + radius).Clip(0, grid.GetLength(0) - 1);

                if (euclidean)
                {
                    // Procedure:  Using the equation of a circle - iterate through the X-dimension
                    //             and calculate all the y values that make the circumference. Try
                    //             filling in the circle to make a cardinally viable region.
                    //

                    for (int columnIndex = left; columnIndex <= right; columnIndex++)
                    {
                        var rowIndex1 = (-1 * System.Math.Sqrt(radius * radius - (columnIndex - column))) + row;
                        var rowIndex2 = System.Math.Sqrt(radius * radius - (columnIndex - column)) + row;

                        rowIndex1 = rowIndex1.Clip(0, grid.GetLength(1) - 1);
                        rowIndex2 = rowIndex2.Clip(0, grid.GetLength(1) - 1);

                        for (int rowIndex = (int)rowIndex1; rowIndex <= (int)rowIndex2; rowIndex++)
                        {
                            if (continuationPredicate(columnIndex, rowIndex) == Array2DCallback.BreakAndReturn)
                                return;
                        }
                    }
                }
                else
                {
                    // CENTER
                    if (radius == 0)
                    {
                        if (continuationPredicate(column, row) == Array2DCallback.BreakAndReturn)
                            return;

                        continue;
                    }

                    // Iterate in a box around the center - using the radius
                    for (int columnIndex = left; columnIndex <= right; columnIndex++)
                    {
                        var top = (row - radius).Clip(0, grid.GetLength(1) - 1);
                        var bottom = (row + radius).Clip(0, grid.GetLength(1) - 1);

                        // LEFT EDGE (or) RIGHT EDGE
                        if (columnIndex == left || columnIndex == right)
                        {
                            for (int rowIndex = top; rowIndex <= bottom; rowIndex++)
                            {
                                if (continuationPredicate(columnIndex, rowIndex) == Array2DCallback.BreakAndReturn)
                                    return;
                            }
                        }
                        // TOP cell (and) BOTTOM cell
                        else
                        {
                            if (continuationPredicate(columnIndex, top) == Array2DCallback.BreakAndReturn)
                                return;

                            if (continuationPredicate(columnIndex, bottom) == Array2DCallback.BreakAndReturn)
                                return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Iterates around a fixed point at a fixed radius - using either euclidean distance measure or rogue measure. Calls back for each cell.
        /// </summary>
        public static void IterateAtFixedRadius<T>(this T[,] grid, int column, int row, int radius, bool euclidean, GridCallback continuationPredicate)
        {
            var left = (column - radius).Clip(0, grid.GetLength(0) - 1);
            var right = (column + radius).Clip(0, grid.GetLength(0) - 1);

            if (euclidean)
            {
                // Procedure:  Using the equation of a circle - iterate through the X-dimension
                //             and calculate all the y values that make the circumference. Try
                //             filling in the circle to make a cardinally viable region.
                //

                for (int columnIndex = left; columnIndex <= right; columnIndex++)
                {
                    var rowIndex1 = (-1 * System.Math.Sqrt(radius * radius - (columnIndex - column))) + row;
                    var rowIndex2 = System.Math.Sqrt(radius * radius - (columnIndex - column)) + row;

                    rowIndex1 = rowIndex1.Clip(0, grid.GetLength(1) - 1);
                    rowIndex2 = rowIndex2.Clip(0, grid.GetLength(1) - 1);

                    // Going to use both the floor and ceiling to try and make this a viable region
                    var index1 = (int)System.Math.Floor(rowIndex1);
                    var index2 = (int)System.Math.Ceiling(rowIndex1);
                    var index3 = (int)System.Math.Floor(rowIndex2);
                    var index4 = (int)System.Math.Ceiling(rowIndex2);

                    // Callback for distinct indices
                    if (continuationPredicate(columnIndex, index1) == Array2DCallback.BreakAndReturn)
                        return;

                    if (index2 != index1)
                    {
                        if (continuationPredicate(columnIndex, index2) == Array2DCallback.BreakAndReturn)
                            return;
                    }

                    if (index3 != index2)
                    {
                        if (continuationPredicate(columnIndex, index3) == Array2DCallback.BreakAndReturn)
                            return;
                    }

                    if (index4 != index3)
                    {
                        if (continuationPredicate(columnIndex, index4) == Array2DCallback.BreakAndReturn)
                            return;
                    }
                }
            }

            else
            {
                // Iterate in a box around the center - using the radius
                for (int columnIndex = left; columnIndex <= right; columnIndex++)
                {
                    var top = (row - radius).Clip(0, grid.GetLength(1) - 1);
                    var bottom = (row + radius).Clip(0, grid.GetLength(1) - 1);

                    // LEFT EDGE (or) RIGHT EDGE
                    if (columnIndex == left || columnIndex == right)
                    {
                        for (int rowIndex = top; rowIndex <= bottom; rowIndex++)
                        {
                            if (continuationPredicate(columnIndex, rowIndex) == Array2DCallback.BreakAndReturn)
                                return;
                        }
                    }
                    // TOP cell (and) BOTTOM cell
                    else
                    {
                        if (continuationPredicate(columnIndex, top) == Array2DCallback.BreakAndReturn)
                            return;

                        if (continuationPredicate(columnIndex, bottom) == Array2DCallback.BreakAndReturn)
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if any items in the grid match the given predicate
        /// </summary>
        public static bool Any<T>(this T[,] grid, Func<T, bool> predicate)
        {
            for (int column = 0; column < grid.GetLength(0); column++)
            {
                for (int row = 0; row < grid.GetLength(1); row++)
                {
                    if (predicate(grid[column, row]))
                        return true;
                }
            }

            return false;
        }

        public static Array2DIndex FirstIndex<T>(this T[,] grid, Func<T, bool> predicate)
        {
            var index = Array2DIndex.Default;

            grid.Iterate((column, row) =>
            {
                if (predicate(grid[column, row]))
                {
                    index = new Array2DIndex(column, row);

                    return Array2DCallback.BreakAndReturn;
                }

                return Array2DCallback.Continue;
            });

            return index;
        }

        /// <summary>
        /// Selects items from the grid matching the given predicate
        /// </summary>
        public static IEnumerable<TResult> Select<T, TResult>(this T[,] grid, Func<T, TResult> selector)
        {
            var list = new List<TResult>();

            grid.Iterate((column, row) =>
            {
                list.Add(selector(grid[column, row]));

                return Array2DCallback.Continue;
            });

            return list;
        }

        /// <summary>
        /// Returns elements of the 2D array that match the given predicate
        /// </summary>
        public static IEnumerable<T> Where<T>(this T[,] grid, GridPredicate<T> predicate)
        {
            var list = new List<T>();

            grid.Iterate((column, row) =>
            {
                if (predicate(column, row, grid[column, row]))
                    list.Add(grid[column, row]);

                return Array2DCallback.Continue;
            });

            return list;
        }

        /// <summary>
        /// Copies over references from the primary grid into a new grid of the same dimensions using the provided copier
        /// </summary>
        public static TResult[,] GridCopy<T, TResult>(this T[,] grid, Func<T, TResult> copier)
        {
            var copy = new TResult[grid.GetLength(0), grid.GetLength(1)];

            grid.Iterate((column, row) =>
            {
                copy[column, row] = copier(grid[column, row]);

                return Array2DCallback.Continue;
            });

            return copy;
        }
    }
}
