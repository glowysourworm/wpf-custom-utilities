
using WpfCustomUtilities.Extensions;
using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.SimpleCollections.Array2D
{
    public struct Array2DRect
    {
        public static Array2DRect Default = new Array2DRect(Array2DIndex.Default, Array2DIndex.Default);

        public Array2DIndex TopLeft { get; set; }
        public Array2DIndex BottomRight { get; set; }

        public int Top { get { return this.TopLeft.Row; } }
        public int Left { get { return this.TopLeft.Column; } }
        public int Right { get { return this.BottomRight.Column; } }
        public int Bottom { get { return this.BottomRight.Row; } }

        public Array2DRect(Array2DIndex topLeft, Array2DIndex bottomRight)
        {
            this.TopLeft = topLeft;
            this.BottomRight = bottomRight;
        }

        public Array2DRect(int column, int row, int columnSpan, int rowSpan)
        {
            this.TopLeft = new Array2DIndex(column, row);
            this.BottomRight = new Array2DIndex(column + columnSpan - 1, row + rowSpan - 1);
        }

        public bool IsCardinallyAdjacent(Array2DRect rect)
        {
            // N
            var north = rect.Bottom == this.Top - 1;

            // S
            var south = rect.Top == this.Bottom + 1;

            // E
            var east = rect.Left == this.Right + 1;

            // W
            var west = rect.Right == this.Left - 1;

            if (north || south)
            {
                return !(rect.Right < this.Left || rect.Left > this.Right);
            }
            else if (east || west)
            {
                return !(rect.Bottom < this.Top || rect.Top > this.Bottom);
            }
            else
                return false;
        }

        public bool IsEquivalent(Array2DRect rect)
        {
            return this.TopLeft.IsEquivalent(rect.TopLeft) &&
                   this.BottomRight.IsEquivalent(rect.BottomRight);
        }

        public static bool operator ==(Array2DRect rect1, Array2DRect rect2)
        {
            return rect1.IsEquivalent(rect2);
        }
        public static bool operator !=(Array2DRect rect1, Array2DRect rect2)
        {
            return !rect1.IsEquivalent(rect2);
        }

        public override bool Equals(object obj)
        {
            var rect = (Array2DRect)obj;

            return this.IsEquivalent(rect);
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.TopLeft, this.BottomRight);
        }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
