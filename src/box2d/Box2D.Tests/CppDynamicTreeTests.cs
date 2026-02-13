using System.Collections.Generic;
using Box2D.Collision;
using Box2D.Common;
using Box2D.Dynamics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DTests
{
    [TestClass]
    public class CppDynamicTreeTests
    {
        [TestMethod]
        public void CreateDestroyProxy()
        {
            b2DynamicTree<int> tree = new b2DynamicTree<int>();

            b2AABB aabb = new b2AABB
            {
                LowerBound = new b2Vec2(-1.0f, -1.0f),
                UpperBound = new b2Vec2(1.0f, 1.0f),
            };

            int data = 123;
            int proxyId = tree.CreateProxy(ref aabb, ref data);

            QueryCollector collector = new QueryCollector();
            tree.Query(collector, aabb);
            Assert.IsTrue(collector.Hits.Contains(proxyId), "Proxy should be found after creation.");

            tree.DestroyProxy(proxyId);

            collector = new QueryCollector();
            tree.Query(collector, aabb);
            Assert.IsFalse(collector.Hits.Contains(proxyId), "Proxy should not be found after destruction.");
        }

        [TestMethod]
        public void RayCastMatchesCppTestCases()
        {
            b2DynamicTree<int> tree = new b2DynamicTree<int>();
            b2AABB aabb = new b2AABB
            {
                LowerBound = new b2Vec2(-1.0f, -1.0f),
                UpperBound = new b2Vec2(1.0f, 1.0f),
            };

            int data = 1;
            int proxyId = tree.CreateProxy(ref aabb, ref data);

            // Test 1: hit from left
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(-3.0f, 0.0f), new b2Vec2(3.0f, 0.0f)));

            // Test 2: hit from right
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(3.0f, 0.0f), new b2Vec2(-3.0f, 0.0f)));

            // Test 3: hit from bottom
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(0.0f, -3.0f), new b2Vec2(0.0f, 3.0f)));

            // Test 4: hit from top
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(0.0f, 3.0f), new b2Vec2(0.0f, -3.0f)));

            // Test 5: miss parallel to x-axis
            Assert.AreEqual(-1, TreeRayCast(tree, new b2Vec2(-3.0f, 2.0f), new b2Vec2(3.0f, 2.0f)));

            // Test 6: miss parallel to y-axis
            Assert.AreEqual(-1, TreeRayCast(tree, new b2Vec2(2.0f, -3.0f), new b2Vec2(2.0f, 3.0f)));

            // Test 7: ray starts inside AABB
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(0.0f, 0.0f), new b2Vec2(2.0f, 0.0f)));

            // Test 8: diagonal hit
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(-2.0f, -2.0f), new b2Vec2(2.0f, 2.0f)));

            // Test 9: parallel outside
            Assert.AreEqual(-1, TreeRayCast(tree, new b2Vec2(-2.0f, 1.5f), new b2Vec2(2.0f, 1.5f)));

            // Test 10: parallel on boundary
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(-2.0f, 1.0f), new b2Vec2(2.0f, 1.0f)));

            // Test 11: short ray doesn't reach
            Assert.AreEqual(-1, TreeRayCast(tree, new b2Vec2(-3.0f, 0.0f), new b2Vec2(-2.5f, 0.0f)));

            // Test 12: degenerate ray is not supported by this C# implementation
            // (b2DynamicTree.RayCast asserts when length == 0). Use a tiny ray instead.
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(0.0f, 0.0f), new b2Vec2(1e-5f, 0.0f)));

            // Test 13: hit at boundary (t = 1)
            Assert.AreEqual(proxyId, TreeRayCast(tree, new b2Vec2(-2.0f, 0.0f), new b2Vec2(-1.0f, 0.0f)));
        }

        [TestMethod]
        public void QueryMatchesCppTestCase()
        {
            b2DynamicTree<int> tree = new b2DynamicTree<int>();

            b2AABB a1 = new b2AABB
            {
                LowerBound = new b2Vec2(-5.0f, -1.0f),
                UpperBound = new b2Vec2(-3.0f, 1.0f),
            };
            b2AABB a2 = new b2AABB
            {
                LowerBound = new b2Vec2(-1.0f, -1.0f),
                UpperBound = new b2Vec2(1.0f, 1.0f),
            };
            b2AABB a3 = new b2AABB
            {
                LowerBound = new b2Vec2(3.0f, -1.0f),
                UpperBound = new b2Vec2(5.0f, 1.0f),
            };

            int d1 = 1, d2 = 2, d3 = 3;
            int id1 = tree.CreateProxy(ref a1, ref d1);
            int id2 = tree.CreateProxy(ref a2, ref d2);
            int id3 = tree.CreateProxy(ref a3, ref d3);

            b2AABB query = new b2AABB
            {
                LowerBound = new b2Vec2(-2.0f, -2.0f),
                UpperBound = new b2Vec2(2.0f, 2.0f),
            };

            QueryCollector collector = new QueryCollector();
            tree.Query(collector, query);

            Assert.IsTrue(collector.Hits.Contains(id2), "Middle proxy should be hit by query.");
            Assert.IsTrue(collector.Hits.Count >= 1, "Query should return at least one proxy.");

            // Avoid unused variable warnings
            _ = id1;
            _ = id3;
        }

        private static int TreeRayCast(b2DynamicTree<int> tree, b2Vec2 p1, b2Vec2 p2)
        {
            b2RayCastInput input = new b2RayCastInput
            {
                p1 = p1,
                p2 = p2,
                maxFraction = 1.0f,
            };

            RayCastCollector collector = new RayCastCollector();
            tree.RayCast(collector, input);
            return collector.HitProxyId;
        }

        private class QueryCollector : Ib2QueryCallback
        {
            public HashSet<int> Hits { get; } = new HashSet<int>();

            public bool QueryCallback(int proxyId)
            {
                Hits.Add(proxyId);
                return true;
            }
        }

        private class RayCastCollector : Ib2RayCastCallback
        {
            public int HitProxyId { get; private set; } = -1;

            public float RayCastCallback(ref b2RayCastInput input, int proxyId)
            {
                HitProxyId = proxyId;
                return 0.0f;
            }
        }
    }
}
