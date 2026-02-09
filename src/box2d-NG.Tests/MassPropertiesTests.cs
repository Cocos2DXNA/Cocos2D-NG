using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class MassPropertiesTests
    {
        [TestMethod]
        public void CircleMass_IsPositive()
        {
            CircleShape circle = new CircleShape(0.5f);
            MassData md = MassProperties.Compute(circle, 2f);
            Assert.IsTrue(md.Mass > 0f);
            Assert.IsTrue(md.Inertia > 0f);
        }

        [TestMethod]
        public void PolygonMass_IsPositive()
        {
            PolygonShape square = new PolygonShape(new[]
            {
                new Vec2(-1f, -1f),
                new Vec2(1f, -1f),
                new Vec2(1f, 1f),
                new Vec2(-1f, 1f)
            });

            MassData md = MassProperties.Compute(square, 1f);
            Assert.IsTrue(md.Mass > 0f);
            Assert.IsTrue(md.Inertia >= 0f);
        }
    }
}
