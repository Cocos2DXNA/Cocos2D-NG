using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void HashSet64_AddRemoveContains_TracksCount()
        {
            HashSet64 set = new HashSet64(2);

            Assert.IsFalse(set.Add(1), "Expected first add to report new key.");
            Assert.IsTrue(set.Contains(1));
            Assert.AreEqual(1, set.Count);

            Assert.IsTrue(set.Add(1), "Expected second add to report existing key.");
            Assert.AreEqual(1, set.Count);

            for (ulong i = 2; i <= 64; ++i)
            {
                Assert.IsFalse(set.Add(i), $"Expected key {i} to be new.");
            }

            Assert.IsTrue(set.Contains(64));
            Assert.AreEqual(64, set.Count);

            Assert.IsTrue(set.Remove(1));
            Assert.IsFalse(set.Contains(1));
            Assert.AreEqual(63, set.Count);
        }

        [TestMethod]
        public void IdPool_ReusesFreedIds()
        {
            IdPool pool = new IdPool();

            int first = pool.Alloc();
            int second = pool.Alloc();
            int third = pool.Alloc();

            pool.Free(second);
            int reused = pool.Alloc();
            Assert.AreEqual(second, reused);

            Assert.AreEqual(3, pool.Capacity);
            Assert.AreEqual(3, pool.Count);

            pool.Free(first);
            pool.Free(third);
            pool.Free(reused);

            Assert.AreEqual(0, pool.Count);
        }
    }
}
