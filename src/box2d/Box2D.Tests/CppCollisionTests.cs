using System;
using Box2D.Collision;
using Box2D.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DTests
{
    [TestClass]
    public class CppCollisionTests
    {
        private const float RayTol = 1e-5f;

        [TestMethod]
        public void AabbValidityAndOverlap()
        {
            b2AABB a = new b2AABB
            {
                LowerBound = new b2Vec2(-1.0f, -1.0f),
                UpperBound = new b2Vec2(-2.0f, -2.0f),
            };

            Assert.IsFalse(a.IsValid(), "AABB should be invalid when upper < lower.");

            a.UpperBound = new b2Vec2(1.0f, 1.0f);
            Assert.IsTrue(a.IsValid(), "AABB should be valid after fixing bounds.");

            b2AABB b = new b2AABB
            {
                LowerBound = new b2Vec2(2.0f, 2.0f),
                UpperBound = new b2Vec2(4.0f, 4.0f),
            };

            Assert.IsFalse(b2Collision.b2TestOverlap(ref a, ref b), "AABBs should not overlap.");
            Assert.IsFalse(a.Contains(ref b), "AABB a should not contain b.");
        }

        [TestMethod]
        public void AabbRayCastCases()
        {
            b2AABB aabb = new b2AABB
            {
                LowerBound = new b2Vec2(-1.0f, -1.0f),
                UpperBound = new b2Vec2(1.0f, 1.0f),
            };

            // Test 1: hit from left
            AssertRayCast(aabb, new b2Vec2(-3.0f, 0.0f), new b2Vec2(3.0f, 0.0f),
                expectHit: true,
                expectedFraction: 1.0f / 3.0f,
                expectedNormal: new b2Vec2(-1.0f, 0.0f),
                expectedPoint: new b2Vec2(-1.0f, 0.0f));

            // Test 2: hit from right
            AssertRayCast(aabb, new b2Vec2(3.0f, 0.0f), new b2Vec2(-3.0f, 0.0f),
                expectHit: true,
                expectedFraction: 1.0f / 3.0f,
                expectedNormal: new b2Vec2(1.0f, 0.0f),
                expectedPoint: new b2Vec2(1.0f, 0.0f));

            // Test 3: hit from bottom
            AssertRayCast(aabb, new b2Vec2(0.0f, -3.0f), new b2Vec2(0.0f, 3.0f),
                expectHit: true,
                expectedFraction: 1.0f / 3.0f,
                expectedNormal: new b2Vec2(0.0f, -1.0f),
                expectedPoint: new b2Vec2(0.0f, -1.0f));

            // Test 4: hit from top
            AssertRayCast(aabb, new b2Vec2(0.0f, 3.0f), new b2Vec2(0.0f, -3.0f),
                expectHit: true,
                expectedFraction: 1.0f / 3.0f,
                expectedNormal: new b2Vec2(0.0f, 1.0f),
                expectedPoint: new b2Vec2(0.0f, 1.0f));

            // Test 5: miss parallel to x-axis
            AssertRayCast(aabb, new b2Vec2(-3.0f, 2.0f), new b2Vec2(3.0f, 2.0f), expectHit: false);

            // Test 6: miss parallel to y-axis
            AssertRayCast(aabb, new b2Vec2(2.0f, -3.0f), new b2Vec2(2.0f, 3.0f), expectHit: false);

            // Test 7: start inside -> no hit in this implementation
            AssertRayCast(aabb, new b2Vec2(0.0f, 0.0f), new b2Vec2(2.0f, 0.0f), expectHit: false);

            // Test 8: diagonal hit at corner
            {
                b2RayCastOutput output = RayCast(aabb, new b2Vec2(-2.0f, -2.0f), new b2Vec2(2.0f, 2.0f), out bool hit);
                Assert.IsTrue(hit, "Diagonal ray should hit.");
                Assert.AreEqual(0.25f, output.fraction, RayTol, "Unexpected fraction.");
                bool normalOk = (Math.Abs(output.normal.x + 1.0f) <= RayTol && Math.Abs(output.normal.y) <= RayTol) ||
                                (Math.Abs(output.normal.x) <= RayTol && Math.Abs(output.normal.y + 1.0f) <= RayTol);
                Assert.IsTrue(normalOk, "Diagonal hit normal should be (-1,0) or (0,-1).");
            }

            // Test 9: parallel outside
            AssertRayCast(aabb, new b2Vec2(-2.0f, 1.5f), new b2Vec2(2.0f, 1.5f), expectHit: false);

            // Test 10: parallel on boundary
            AssertRayCast(aabb, new b2Vec2(-2.0f, 1.0f), new b2Vec2(2.0f, 1.0f),
                expectHit: true,
                expectedFraction: 0.25f,
                expectedNormal: new b2Vec2(-1.0f, 0.0f));

            // Test 11: short ray doesn't reach
            AssertRayCast(aabb, new b2Vec2(-3.0f, 0.0f), new b2Vec2(-2.5f, 0.0f), expectHit: false);

            // Test 12: zero-length ray
            AssertRayCast(aabb, new b2Vec2(0.0f, 0.0f), new b2Vec2(0.0f, 0.0f), expectHit: false);

            // Test 13: hit at boundary (t = 1)
            AssertRayCast(aabb, new b2Vec2(-2.0f, 0.0f), new b2Vec2(-1.0f, 0.0f),
                expectHit: true,
                expectedFraction: 1.0f,
                expectedNormal: new b2Vec2(-1.0f, 0.0f));

            // Test 14: offset AABB
            b2AABB offsetAabb = new b2AABB
            {
                LowerBound = new b2Vec2(2.0f, 3.0f),
                UpperBound = new b2Vec2(4.0f, 5.0f),
            };
            AssertRayCast(offsetAabb, new b2Vec2(0.0f, 4.0f), new b2Vec2(6.0f, 4.0f),
                expectHit: true,
                expectedFraction: 1.0f / 3.0f,
                expectedNormal: new b2Vec2(-1.0f, 0.0f),
                expectedPoint: new b2Vec2(2.0f, 4.0f));
        }

        private static b2RayCastOutput RayCast(b2AABB aabb, b2Vec2 p1, b2Vec2 p2, out bool hit)
        {
            b2RayCastInput input = new b2RayCastInput
            {
                p1 = p1,
                p2 = p2,
                maxFraction = 1.0f,
            };

            b2RayCastOutput output;
            hit = aabb.RayCast(out output, input);
            return output;
        }

        private static void AssertRayCast(
            b2AABB aabb,
            b2Vec2 p1,
            b2Vec2 p2,
            bool expectHit,
            float? expectedFraction = null,
            b2Vec2? expectedNormal = null,
            b2Vec2? expectedPoint = null)
        {
            b2RayCastOutput output = RayCast(aabb, p1, p2, out bool hit);
            Assert.AreEqual(expectHit, hit, "Ray cast hit mismatch.");

            if (!expectHit)
            {
                return;
            }

            if (expectedFraction.HasValue)
            {
                Assert.AreEqual(expectedFraction.Value, output.fraction, RayTol, "Unexpected fraction.");
            }

            if (expectedNormal.HasValue)
            {
                b2Vec2 n = expectedNormal.Value;
                Assert.AreEqual(n.x, output.normal.x, RayTol, "Unexpected normal.x.");
                Assert.AreEqual(n.y, output.normal.y, RayTol, "Unexpected normal.y.");
            }

            if (expectedPoint.HasValue)
            {
                b2Vec2 point = p1 + output.fraction * (p2 - p1);
                b2Vec2 p = expectedPoint.Value;
                Assert.AreEqual(p.x, point.x, RayTol, "Unexpected hit point.x.");
                Assert.AreEqual(p.y, point.y, RayTol, "Unexpected hit point.y.");
            }
        }
    }
}
