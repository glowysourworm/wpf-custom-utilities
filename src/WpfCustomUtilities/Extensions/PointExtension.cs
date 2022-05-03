using System.Windows;

namespace WpfCustomUtilities.Extensions
{
    public static class PointExtension
    {
        public static Point SnapToGrid(this Point point, double cellWidth, double cellHeight)
        {
            return new Point(point.X.ToNearest(cellWidth, false),
                             point.Y.ToNearest(cellHeight, false));
        }
    }
}
