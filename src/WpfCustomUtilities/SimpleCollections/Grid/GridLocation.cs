using System;

using WpfCustomUtilities.RecursiveSerializer.Component.Interface;
using WpfCustomUtilities.RecursiveSerializer.Utility;
using WpfCustomUtilities.SimpleCollections.Collection;
using WpfCustomUtilities.SimpleCollections.Collection.Interface;
using WpfCustomUtilities.SimpleCollections.Graph.Interface;
using WpfCustomUtilities.SimpleCollections.Grid.Interface;

namespace WpfCustomUtilities.SimpleCollections.Grid
{
    [Serializable]
    public class GridLocation : IGridLocator, IGraphNode, INotifyDictionaryKey
    {
        public int Row { get; set; }
        public int Column { get; set; }

        #region (public) IGraphNode
        public double X { get { return this.Column; } }
        public double Y { get { return this.Row; } }
        #endregion

        public GridLocation()
        {
        }
        public GridLocation(GridLocation copy)
        {
            this.Row = copy.Row;
            this.Column = copy.Column;
        }
        public GridLocation(IGridLocator locator)
        {
            this.Row = locator.Row;
            this.Column = locator.Column;
        }
        public GridLocation(int column, int row)
        {
            Row = row;
            Column = column;
        }

        public GridLocation(IPropertyReader reader)
        {
            this.Column = reader.Read<int>("Column");
            this.Row = reader.Read<int>("Row");
        }

        // Not used
        public event NotifyHashCodeChanged HashCodeChangedEvent;

        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("Column", this.Column);
            writer.Write("Row", this.Row);
        }
        public bool IsLocationEquivalent(int column, int row)
        {
            return this.Column == column &&
                   this.Row == row;
        }
        public bool IsLocationEquivalent(IGridLocator locator)
        {
            return this.Column == locator.Column &&
                   this.Row == locator.Row;
        }
        public static bool operator ==(GridLocation gridLocation1, GridLocation gridLocation2)
        {
            if (ReferenceEquals(gridLocation1, null))
                return ReferenceEquals(gridLocation2, null);

            else if (ReferenceEquals(gridLocation2, null))
                return false;

            else
                return gridLocation1.Equals(gridLocation2);
        }
        public static bool operator !=(GridLocation gridLocation1, GridLocation gridLocation2)
        {
            return !(gridLocation1 == gridLocation2);
        }
        public override bool Equals(object obj)
        {
            return this.GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.Column, this.Row);
        }

        public override string ToString()
        {
            return "Column=" + Column.ToString() + " Row=" + Row.ToString();
        }
    }
}
