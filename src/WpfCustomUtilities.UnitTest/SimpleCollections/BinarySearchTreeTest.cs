using NUnit.Framework;

using System.Collections.Generic;
using System.Drawing;

using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.UnitTest.SimpleCollections
{
    public class BinarySearchTreeTest
    {
        public class PointComparer : Comparer<Point>
        {
            public override int Compare(Point point1, Point point2)
            {
                return point1.X.CompareTo(point2.X);
            }
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BinarySearchTreeBasic()
        {
            var tree = new BinarySearchTree<float, float>(Comparer<float>.Default);

            tree.Insert(2.0f, 2.0f);
            tree.Insert(4.0f, 4.0f);
            tree.Insert(3.0f, 3.0f);
            tree.Insert(5.0f, 5.0f);
            tree.Insert(7.0f, 7.0f);
            tree.Insert(10.0f, 10.0f);

            // Count
            Assert.IsTrue(tree.Count == 6);

            // Min / Max
            Assert.IsTrue(tree.Min() == 2.0f);
            Assert.IsTrue(tree.Max() == 10.0f);
            Assert.IsTrue(tree.MinKey() == 2.0f);
            Assert.IsTrue(tree.MaxKey() == 10.0f);

            // Successor
            Assert.IsTrue(tree.Successor(2.0f).Value == 3.0f);
            Assert.IsTrue(tree.Successor(3.0f).Value == 4.0f);
            Assert.IsTrue(tree.Successor(4.0f).Value == 5.0f);
            Assert.IsTrue(tree.Successor(5.0f).Value == 7.0f);
            Assert.IsTrue(tree.Successor(7.0f).Value == 10.0f);
            Assert.IsTrue(tree.Successor(10.0f) == null);

            // Predecessor
            Assert.IsTrue(tree.Predecessor(2.0f) == null);
            Assert.IsTrue(tree.Predecessor(10.0f).Value == 7.0f);
            Assert.IsTrue(tree.Predecessor(7.0f).Value == 5.0f);
            Assert.IsTrue(tree.Predecessor(5.0f).Value == 4.0f);
            Assert.IsTrue(tree.Predecessor(4.0f).Value == 3.0f);
            Assert.IsTrue(tree.Predecessor(3.0f).Value == 2.0f);

            // Search
            Assert.IsTrue(tree.Search(2.0f).Value == 2.0f);
            Assert.IsTrue(tree.Search(3.0f).Value == 3.0f);
            Assert.IsTrue(tree.Search(4.0f).Value == 4.0f);
            Assert.IsTrue(tree.Search(5.0f).Value == 5.0f);
            Assert.IsTrue(tree.Search(7.0f).Value == 7.0f);
            Assert.IsTrue(tree.Search(10.0f).Value == 10.0f);
            Assert.IsTrue(tree.Search(1023.0f).Value == 10.0f);

            // Remove
            Assert.IsTrue(tree.Remove(2.0f));
            Assert.IsTrue(tree.Remove(3.0f));
            Assert.IsTrue(tree.Remove(4.0f));
            Assert.IsTrue(tree.Remove(5.0f));
            Assert.IsTrue(tree.Remove(7.0f));
            Assert.IsTrue(tree.Remove(10.0f));

            Assert.IsTrue(tree.Count == 0);
        }

        [Test]
        public void BinarySearchTreeComparer()
        {
            var tree = new BinarySearchTree<Point, Point>(new PointComparer());

            var point1 = new Point(0, 5);
            var point2 = new Point(1, 3);
            var point3 = new Point(5, 1);
            var point4 = new Point(9, 2);
            var point5 = new Point(120, 0);

            tree.Insert(point1, point1);
            tree.Insert(point2, point2);
            tree.Insert(point3, point3);
            tree.Insert(point4, point4);
            tree.Insert(point5, point5);

            // Count
            Assert.IsTrue(tree.Count == 5);

            // Min / Max
            Assert.IsTrue(tree.Min() == point1);
            Assert.IsTrue(tree.Max() == point5);
            Assert.IsTrue(tree.MinKey() == point1);
            Assert.IsTrue(tree.MaxKey() == point5);

            // Successor
            Assert.IsTrue(tree.Successor(point1).Value == point2);
            Assert.IsTrue(tree.Successor(point2).Value == point3);
            Assert.IsTrue(tree.Successor(point3).Value == point4);
            Assert.IsTrue(tree.Successor(point4).Value == point5);
            Assert.IsTrue(tree.Successor(point5) == null);

            // Predecessor
            Assert.IsTrue(tree.Predecessor(point1) == null);
            Assert.IsTrue(tree.Predecessor(point2).Value == point1);
            Assert.IsTrue(tree.Predecessor(point3).Value == point2);
            Assert.IsTrue(tree.Predecessor(point4).Value == point3);
            Assert.IsTrue(tree.Predecessor(point5).Value == point4);

            // Search
            Assert.IsTrue(tree.Search(point1).Value == point1);
            Assert.IsTrue(tree.Search(point2).Value == point2);
            Assert.IsTrue(tree.Search(point3).Value == point3);
            Assert.IsTrue(tree.Search(point4).Value == point4);
            Assert.IsTrue(tree.Search(point5).Value == point5);
            Assert.IsTrue(tree.Search(new Point(12, 33)).Value == point5);

            // Remove
            Assert.IsTrue(tree.Remove(point1));
            Assert.IsTrue(tree.Remove(point2));
            Assert.IsTrue(tree.Remove(point3));
            Assert.IsTrue(tree.Remove(point4));
            Assert.IsTrue(tree.Remove(point5));

            Assert.IsTrue(tree.Count == 0);
        }
    }
}
