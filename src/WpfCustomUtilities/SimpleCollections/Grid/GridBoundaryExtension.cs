using System.Collections.Generic;
using System.Linq;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    public static class GridBoundaryExtension
    {
        /// <summary>
        /// ADJUSTS INVALID BOUNDARY to stay either WITHIN or OUTSIDE of the boundary
        /// </summary>
        /// <param name="boundary">This boundary (valid one)</param>
        /// <param name="invalidBoundary">Potentially overlapping boundary</param>
        /// <param name="container">THIS boundary is supposed to contain the other one</param>
        public static bool CalculateValidBoundary(this GridBoundary boundary, GridBoundary invalidBoundary, IEnumerable<GridBoundary> otherMasks, out GridBoundary result)
        {
            // Procedure
            //
            // 1) Create copy of invalid boundary
            // 2) Test each edge 
            //      - Overlap:  a) Shift the test boundary
            //                  b) Test out the other masks
            // 3) If valid, Then update the return value
            // 4) Else, return the best solution with a failure flag
            //

            var testBoundary = new GridBoundary(invalidBoundary);
            var valid = true;

            // Left Overlaps:  Shift Right
            if (testBoundary.Left >= boundary.Left &&
                testBoundary.Left <= boundary.Right &&
                boundary.Overlaps(testBoundary) && valid)
            {
                testBoundary.Set(testBoundary.Column + (boundary.Right - testBoundary.Left) + 1,
                                 testBoundary.Row,
                                 testBoundary.Width,
                                 testBoundary.Height);
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            // Right Overlaps:  Shift Left
            if (testBoundary.Right >= boundary.Left &&
                testBoundary.Right <= boundary.Right &&
                boundary.Overlaps(testBoundary) && valid)
            {
                testBoundary.Set(testBoundary.Column - (testBoundary.Right - boundary.Left) - 1,
                                 testBoundary.Row,
                                 testBoundary.Width,
                                 testBoundary.Height);
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            // Top Overlaps:  Shift Down
            if (testBoundary.Top >= boundary.Top &&
                testBoundary.Top <= boundary.Bottom &&
                boundary.Overlaps(testBoundary) && valid)
            {
                testBoundary.Set(testBoundary.Column,
                                 testBoundary.Row + (boundary.Bottom - testBoundary.Top) + 1,
                                 testBoundary.Width,
                                 testBoundary.Height);
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            // Bottom Overlaps:  Shift Up
            if (testBoundary.Bottom >= boundary.Top &&
                testBoundary.Bottom <= boundary.Bottom &&
                boundary.Overlaps(testBoundary) && valid)
            {
                testBoundary.Set(testBoundary.Column,
                                 testBoundary.Row - (testBoundary.Bottom - boundary.Top) - 1,
                                 testBoundary.Width,
                                 testBoundary.Height);
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            result = new GridBoundary(testBoundary);

            return valid;
        }

        /// <summary>
        /// Adjusts invalid boundary to stay either WITHIN or OUTSIDE of the boundary
        /// </summary>
        /// <param name="boundary">This boundary (valid one)</param>
        /// <param name="invalidBoundary">Potentially overlapping boundary</param>
        /// <param name="container">THIS boundary is supposed to contain the other one</param>
        public static bool CalculateValidInteriorBoundary(this GridBoundary boundary, GridBoundary invalidBoundary, IEnumerable<GridBoundary> otherMasks, bool resize, out GridBoundary result)
        {
            // Procedure
            //
            // 1) Create copy of invalid boundary
            // 2) Test each edge 
            //      - Overlap:  a) Shift the test boundary
            //                  b) Test out the other masks
            // 3) If valid, Then update the return value
            // 4) Else, return the best solution with a failure flag
            //

            var testBoundary = new GridBoundary(invalidBoundary);

            var valid = true;

            // RESIZE DIS-ALLOWED
            if (testBoundary.Right > boundary.Right)
            {
                if (!resize)
                {
                    testBoundary.Set(testBoundary.Column - (testBoundary.Right - boundary.Right),
                                     testBoundary.Row,
                                     testBoundary.Width,
                                     testBoundary.Height);
                }
                else
                    valid = false;
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            if (testBoundary.Left < boundary.Left && valid)
            {
                testBoundary.Set(testBoundary.Column + (boundary.Left - testBoundary.Left),
                                    testBoundary.Row,
                                    testBoundary.Width,
                                    testBoundary.Height);
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            if (testBoundary.Top < boundary.Top && valid)
            {
                testBoundary.Set(testBoundary.Column,
                                    testBoundary.Row + (boundary.Top - testBoundary.Top),
                                    testBoundary.Width,
                                    testBoundary.Height);
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            // RESIZE DIS-ALLOWED
            if (testBoundary.Bottom > boundary.Bottom && valid)
            {
                if (!resize)
                {
                    testBoundary.Set(testBoundary.Column,
                                     testBoundary.Row - (testBoundary.Bottom - boundary.Bottom),
                                     testBoundary.Width,
                                     testBoundary.Height);
                }
                else
                    valid = false;
            }

            // Update failure flag
            if (valid && otherMasks.Any(mask => mask.Overlaps(testBoundary)))
                valid = false;

            // Update the output
            result = new GridBoundary(testBoundary);

            return valid;
        }
    }
}
