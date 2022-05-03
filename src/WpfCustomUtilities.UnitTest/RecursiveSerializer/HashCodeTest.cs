using NUnit.Framework;

using WpfCustomUtilities.RecursiveSerializer.Utility;

namespace WpfCustomUtilities.UnitTest.RecursiveSerializer
{
    public class HashCodeTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void UniquenessSimple()
        {
            var hash1 = RecursiveSerializerHashGenerator.CreateSimpleHash(8);
            var hash2 = RecursiveSerializerHashGenerator.CreateSimpleHash(8);
            var hash3 = RecursiveSerializerHashGenerator.CreateSimpleHash(16);

            Assert.IsTrue(hash1.Equals(hash2));
            Assert.IsTrue(!hash1.Equals(hash3));
            Assert.IsTrue(!hash2.Equals(hash3));
        }

        [Test]
        public void UniquenessStringSimple()
        {
            var hash1 = RecursiveSerializerHashGenerator.CreateSimpleHash(" ");
            var hash2 = RecursiveSerializerHashGenerator.CreateSimpleHash(" ");
            var hash3 = RecursiveSerializerHashGenerator.CreateSimpleHash("  ");

            Assert.IsTrue(hash1.Equals(hash2));
            Assert.IsTrue(!hash1.Equals(hash3));
            Assert.IsTrue(!hash2.Equals(hash3));
        }

        [Test]
        public void OrderingSimple()
        {
            var hash1 = RecursiveSerializerHashGenerator.CreateSimpleHash(1, 2);
            var hash2 = RecursiveSerializerHashGenerator.CreateSimpleHash(1, 2);
            var hash3 = RecursiveSerializerHashGenerator.CreateSimpleHash(2, 1);

            Assert.IsTrue(hash1.Equals(hash2));
            Assert.IsTrue(!hash1.Equals(hash3));
            Assert.IsTrue(!hash2.Equals(hash3));
        }
    }
}