using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class ContinuousCollisionTests
    {
        [TestMethod]
        public void BulletDoesNotTunnelThroughWall()
        {
            WorldDef def = new WorldDef()
                .WithGravity(Vec2.Zero)
                .EnableContinuousCollision(true)
                .WithMaxSubSteps(8);

            World world = new World(def);

            Body wall = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Vec2[] wallVerts =
            {
                new Vec2(-0.1f, -5f),
                new Vec2(0.1f, -5f),
                new Vec2(0.1f, 5f),
                new Vec2(-0.1f, 5f)
            };
            wall.CreateFixture(new FixtureDef(new PolygonShape(wallVerts)));

            Body bullet = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 0f).IsBullet(true));
            bullet.CreateFixture(new FixtureDef(new CircleShape(0.25f)).WithDensity(1f));
            bullet.LinearVelocity = new Vec2(1000f, 0f);

            world.Step(1f / 60f);

            float bulletX = bullet.GetWorldCenter().X;
            Assert.IsTrue(bulletX <= 0.2f, $"Expected bullet to stop at wall, got x={bulletX}.");
        }

        [TestMethod]
        public void BulletHitsWallEvenWithoutPriorContact()
        {
            WorldDef def = new WorldDef()
                .WithGravity(Vec2.Zero)
                .EnableContinuousCollision(true)
                .WithMaxSubSteps(8);

            World world = new World(def);

            Body wall = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Vec2[] wallVerts =
            {
                new Vec2(-0.1f, -5f),
                new Vec2(0.1f, -5f),
                new Vec2(0.1f, 5f),
                new Vec2(-0.1f, 5f)
            };
            wall.CreateFixture(new FixtureDef(new PolygonShape(wallVerts)));

            Body bullet = world.CreateBody(new BodyDef().AsDynamic().At(-10f, 0f).IsBullet(true));
            bullet.CreateFixture(new FixtureDef(new CircleShape(0.25f)).WithDensity(1f));
            bullet.LinearVelocity = new Vec2(500f, 0f);

            world.Step(1f / 60f);

            float bulletX = bullet.GetWorldCenter().X;
            Assert.IsTrue(bulletX <= 0.2f, $"Expected bullet to stop at wall without prior contact, got x={bulletX}.");
        }
    }
}
