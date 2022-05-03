using System;

using WpfCustomUtilities.SimpleCollections.Grid.Interface;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    public static class GridRoutine
    {
        public static Compass GetDirectionOfAdjacentLocation(IGridLocator location, IGridLocator adjacentLocation)
        {
            return GetDirectionOfAdjacentLocation(location.Column, location.Row, adjacentLocation.Column, adjacentLocation.Row);
        }

        public static Compass GetDirectionOfAdjacentLocation(int column, int row, int adjacentColumn, int adjacentRow)
        {
            var north = (adjacentRow - row) == -1;
            var south = (adjacentRow - row) == 1;
            var east = (adjacentColumn - column) == 1;
            var west = (adjacentColumn - column) == -1;

            if (north && east) return Compass.NE;
            else if (north && west) return Compass.NW;
            else if (south && east) return Compass.SE;
            else if (south && west) return Compass.SW;
            else if (north) return Compass.N;
            else if (south) return Compass.S;
            else if (east) return Compass.E;
            else if (west) return Compass.W;
            else
                throw new Exception("Invalid adjacent cell GetDirectionOfAdjacentLocation");
        }

        public static CompassConstrained GetConstrainedDirectionOfAdjacentLocation(IGridLocator location, IGridLocator adjacentLocation)
        {
            var north = (adjacentLocation.Row - location.Row) == -1;
            var south = (adjacentLocation.Row - location.Row) == 1;
            var east = (adjacentLocation.Column - location.Column) == 1;
            var west = (adjacentLocation.Column - location.Column) == -1;

            if (north && east) return CompassConstrained.NE;
            else if (north && west) return CompassConstrained.NW;
            else if (south && east) return CompassConstrained.SE;
            else if (south && west) return CompassConstrained.SW;
            else if (north) return CompassConstrained.N;
            else if (south) return CompassConstrained.S;
            else if (east) return CompassConstrained.E;
            else if (west) return CompassConstrained.W;
            else
                throw new Exception("Invalid adjacent cell GetDirectionOfAdjacentLocation");
        }

        public static Compass GetOppositeDirection(Compass direction)
        {
            switch (direction)
            {
                case Compass.N:
                    return Compass.S;
                case Compass.S:
                    return Compass.N;
                case Compass.E:
                    return Compass.W;
                case Compass.W:
                    return Compass.E;
                case Compass.NE:
                    return Compass.SW;
                case Compass.NW:
                    return Compass.SE;
                case Compass.SE:
                    return Compass.SW;
                case Compass.SW:
                    return Compass.SE;
                default:
                    return Compass.Null;
            }
        }

        public static bool IsCardinalDirection(IGridLocator location, IGridLocator adjacentLocation)
        {
            var direction = GetDirectionOfAdjacentLocation(location, adjacentLocation);

            return IsCardinalDirection(direction);
        }

        public static bool IsCardinalDirection(int column, int row, int adjacentColumn, int adjacentRow)
        {
            var direction = GetDirectionOfAdjacentLocation(column, row, adjacentColumn, adjacentRow);

            return IsCardinalDirection(direction);
        }

        public static bool IsCardinalDirection(Compass direction)
        {
            switch (direction)
            {
                case Compass.N:
                case Compass.S:
                case Compass.E:
                case Compass.W:
                    return true;

                case Compass.Null:
                case Compass.NW:
                case Compass.NE:
                case Compass.SE:
                case Compass.SW:
                default:
                    return false;
            }
        }

        public static bool IsCardinalDirection(CompassConstrained direction)
        {
            switch (direction)
            {
                case CompassConstrained.N:
                case CompassConstrained.S:
                case CompassConstrained.E:
                case CompassConstrained.W:
                    return true;

                case CompassConstrained.Null:
                case CompassConstrained.NW:
                case CompassConstrained.NE:
                case CompassConstrained.SE:
                case CompassConstrained.SW:
                default:
                    return false;
            }
        }

        public static Compass GetCardinalDirection(Compass direction, CompassCardinality cardinality)
        {
            switch (direction)
            {
                case Compass.Null:
                case Compass.N:
                case Compass.S:
                case Compass.E:
                case Compass.W:
                    return direction;

                case Compass.NW:
                    if (cardinality == CompassCardinality.NorthSouth)
                        return Compass.N;
                    else
                        return Compass.W;
                case Compass.NE:
                    if (cardinality == CompassCardinality.NorthSouth)
                        return Compass.N;
                    else
                        return Compass.E;
                case Compass.SE:
                    if (cardinality == CompassCardinality.NorthSouth)
                        return Compass.S;
                    else
                        return Compass.E;
                case Compass.SW:
                    if (cardinality == CompassCardinality.NorthSouth)
                        return Compass.S;
                    else
                        return Compass.W;
                default:
                    throw new Exception("Unhandled compass direction GridCalculator.GetCardinalDirection");
            }
        }

        public static CompassCardinality GetCardinality(Compass direction)
        {
            switch (direction)
            {
                case Compass.N:
                case Compass.S:
                    return CompassCardinality.NorthSouth;
                case Compass.E:
                case Compass.W:
                    return CompassCardinality.EastWest;

                case Compass.Null:
                case Compass.NW:
                case Compass.NE:
                case Compass.SE:
                case Compass.SW:
                default:
                    throw new Exception("Trying to get cardinality for off-diagonal or invalid Compass value:  GridCalculator.cs");
            }
        }

        public static CompassCardinality GetOppositeCardinality(CompassCardinality cardinality)
        {
            switch (cardinality)
            {
                case CompassCardinality.NorthSouth:
                    return CompassCardinality.EastWest;
                case CompassCardinality.EastWest:
                    return CompassCardinality.NorthSouth;
                default:
                    throw new Exception("Unhandled compass cardinality GridCalculator.cs");
            }
        }
    }
}
