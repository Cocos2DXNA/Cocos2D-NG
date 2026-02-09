using Box2D.Collision;
using Box2D.Collision.Shapes;
using Box2D.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DTests
{
    [TestClass]
    public class CppDistanceTests
    {
        private const float Epsilon = 1e-6f;

        [TestMethod]
        public void ShapeDistanceMatchesCppTest()
        {
            b2PolygonShape box = new b2PolygonShape();
            box.SetAsBox(1.0f, 1.0f);
            box.Radius = 0.0f;

            b2EdgeShape segment = new b2EdgeShape();
            segment.Set(new b2Vec2(2.0f, -1.0f), new b2Vec2(2.0f, 1.0f));
            segment.Radius = 0.0f;

            b2DistanceInput input = b2DistanceInput.Create();
            input.proxyA = b2DistanceProxy.Create(box, 0);
            input.proxyB = b2DistanceProxy.Create(segment, 0);
            input.transformA = b2Transform.Identity;
            input.transformB = b2Transform.Identity;
            input.useRadii = false;

            b2SimplexCache cache = b2SimplexCache.Create();

            b2DistanceOutput output;
            b2Simplex.b2Distance(out output, ref cache, ref input);

            Assert.AreEqual(1.0f, output.distance, Epsilon, "Shape distance should be 1.0.");
        }

        [TestMethod]
        public void TimeOfImpactMatchesCppTest()
        {
            b2PolygonShape box = new b2PolygonShape();
            box.SetAsBox(1.0f, 1.0f);
            box.Radius = 0.0f;

            b2EdgeShape segment = new b2EdgeShape();
            segment.Set(new b2Vec2(2.0f, -1.0f), new b2Vec2(2.0f, 1.0f));
            segment.Radius = 0.0f;

            b2TOIInput input = b2TOIInput.Create();
            input.proxyA = b2DistanceProxy.Create(box, 0);
            input.proxyB = b2DistanceProxy.Create(segment, 0);
            input.sweepA = new b2Sweep
            {
                localCenter = b2Vec2.Zero,
                c0 = b2Vec2.Zero,
                c = b2Vec2.Zero,
                a0 = 0.0f,
                a = 0.0f,
                alpha0 = 0.0f,
            };
            input.sweepB = new b2Sweep
            {
                localCenter = b2Vec2.Zero,
                c0 = b2Vec2.Zero,
                c = new b2Vec2(-2.0f, 0.0f),
                a0 = 0.0f,
                a = 0.0f,
                alpha0 = 0.0f,
            };
            input.tMax = 1.0f;

            b2TOIOutput output;
            b2TimeOfImpact.Compute(out output, ref input);

            Assert.AreEqual(b2ImpactState.e_touching, output.state, "TOI should be touching.");
            Assert.AreEqual(0.5f, output.t, 0.005f, "TOI fraction should be about 0.5.");
        }
    }
}
