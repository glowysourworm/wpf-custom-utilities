using NUnit.Framework;

using System.Collections.Generic;

using WpfCustomUtilities.SimpleCollections.Collection;

namespace WpfCustomUtilities.UnitTest.SimpleCollections
{
    public class SimpleOrderedListTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SimpleOrderedList_Basic()
        {
            var items = new float[8] { 2, 1.0f, -12.3f, 4, 1.0f, 5, 5, 5 };

            var list = new SimpleOrderedList<float>(Comparer<float>.Default);

            // Perform the sort
            foreach (var item in items)
                list.Add(item);

            Assert.IsTrue(list[0] == items[2]);
            Assert.IsTrue(list[1] == items[1]);
            Assert.IsTrue(list[2] == items[4]);
            Assert.IsTrue(list[3] == items[0]);
            Assert.IsTrue(list[4] == items[3]);
            Assert.IsTrue(list[5] == items[5]);
            Assert.IsTrue(list[6] == items[6]);
            Assert.IsTrue(list[7] == items[7]);
        }
    }
}