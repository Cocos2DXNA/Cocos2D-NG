using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class CollisionCastTests
    {
        [TestMethod]
        public void RayCastCircle_HitsAtExpectedFraction()
        {
            Circle circle = new Circle(Vec2.Zero, 1f);
            RayCastInput input = new RayCastInput(new Vec2(-2f, 0f), new Vec2(4f, 0f), 1f);

            CastOutput output = Collision.RayCastCircle(circle, input);

            Assert.IsTrue(output.Hit, "Expected ray to hit the circle.");
            Assert.IsTrue(MathF.Abs(output.Fraction - 0.25f) < 0.05f, $"Unexpected fraction: {output.Fraction}");
        }

        [TestMethod]
        public void ShapeCastCircle_HitsFixedCircle()
        {
            ShapeProxy moving = ShapeProxyFactory.FromCircle(new Circle(new Vec2(-2f, 0f), 1f));
            ShapeCastInput input = new ShapeCastInput(moving, new Vec2(4f, 0f), 1f, true);
            Circle fixedCircle = new Circle(Vec2.Zero, 1f);

            CastOutput output = Collision.ShapeCastCircle(fixedCircle, input);

            Assert.IsTrue(output.Hit, "Expected shape cast to hit the fixed circle.");
            Assert.IsTrue(output.Fraction >= 0f && output.Fraction <= 1f);
        }

        [TestMethod]
        public void RayCastPolygon_HitsAtExpectedFraction()
        {
            PolygonShape shape = new PolygonShape(new[]
            {
                new Vec2(-1f, -1f),
                new Vec2(1f, -1f),
                new Vec2(0f, 1f)
            });
            Polygon polygon = ShapeGeometry.ToPolygon(shape);

            RayCastInput input = new RayCastInput(new Vec2(0f, 2f), new Vec2(0f, -4f), 1f);
            CastOutput output = Collision.RayCastPolygon(polygon, input);

            Assert.IsTrue(output.Hit, "Expected ray to hit the polygon.");
            Assert.IsTrue(MathF.Abs(output.Fraction - 0.25f) < 0.05f, $"Unexpected fraction: {output.Fraction}");
        }
    }
}
