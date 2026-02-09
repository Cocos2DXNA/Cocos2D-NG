using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class WorldQueryTests
    {
        [TestMethod]
        public void RayCastClosest_HitsFixture()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body body = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            PolygonShape box = new PolygonShape(new[]
            {
                new Vec2(-1f, -1f),
                new Vec2(1f, -1f),
                new Vec2(1f, 1f),
                new Vec2(-1f, 1f)
            });
            body.CreateFixture(new FixtureDef(box));

            RayCastInput input = new RayCastInput(new Vec2(0f, 3f), new Vec2(0f, -6f), 1f);
            bool hit = world.RayCast(input, out World.RayCastHit hitInfo);

            Assert.IsTrue(hit, "Expected ray cast to hit the box.");
            Assert.IsTrue(hitInfo.Fraction < 1f);
            Assert.IsTrue(MathF.Abs(hitInfo.Point.Y - 1f) < 0.2f, $"Unexpected hit point: {hitInfo.Point}");
        }
    }
}
