using System;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    /// <summary>
    /// This enum will behave like flags; and the inverted enum ~Compass will behave directionally
    /// as it should. Example:  ~Compass.NW = Compass.SE
    /// </summary>
    public enum Compass : int
    {
        Null = 0,
        N = 1,
        S = 2,
        E = 4,
        W = 8,
        NW = Compass.N | Compass.W,
        NE = Compass.N | Compass.E,
        SE = Compass.S | Compass.E,
        SW = Compass.S | Compass.W
    }

    /// <summary>
    /// Extended compass FLAGS enum used for SPECIFIC DIRECTIONAL CONSTRATINS. FLAGS DO NOT COMPOSE
    /// DIRECTIONS!  Example:  (N | E) != NE.  N.Has(N) != NE.Has(N)
    /// </summary>
    [Flags]
    public enum CompassConstrained : byte
    {
        Null = 0x00,
        N = 0x01,
        S = 0x02,
        E = 0x04,
        W = 0x08,
        NW = 0x10,
        NE = 0x20,
        SE = 0x40,
        SW = 0x80,
        All = 0xFF
    }

    [Flags]
    public enum CompassCardinality : byte
    {
        None = 0,
        NorthSouth = 1,
        EastWest = 2
    }

    [Flags]
    public enum CompassOrdinality : byte
    {
        /// <summary>
        /// Movement not specified
        /// </summary>
        None = 0,

        /// <summary>
        /// Movement should be restricted to cardinal
        /// </summary>
        Cardinal = 1,

        /// <summary>
        /// Movement should be restricted to non-cardinal
        /// </summary>
        Diagonal = 2
    }
}
