using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.SimpleCollections.Array2D
{
    public struct Array2DIndex
    {
        public static Array2DIndex Default = new Array2DIndex(-1, -1);

        public int Column { get; set; }
        public int Row { get; set; }

        public Array2DIndex(int column, int row)
        {
            this.Column = column;
            this.Row = row;
        }
        public Array2DIndex(Array2DIndex index)
        {
            this.Column = index.Column;
            this.Row = index.Row;
        }

        public bool IsEquivalent(int column, int row)
        {
            return this.Column == column &&
                   this.Row == row;
        }

        public bool IsEquivalent(Array2DIndex index)
        {
            return this.Column == index.Column &&
                   this.Row == index.Row;
        }

        public static int CalculateHashCode(int column, int row)
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(column, row);
        }

        public static bool operator ==(Array2DIndex index1, Array2DIndex index2)
        {
            return index1.IsEquivalent(index2);
        }
        public static bool operator !=(Array2DIndex index1, Array2DIndex index2)
        {
            return !index1.IsEquivalent(index2);
        }

        public override bool Equals(object obj)
        {
            var index = (Array2DIndex)obj;

            return this.IsEquivalent(index);
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.Column, this.Row);
        }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
