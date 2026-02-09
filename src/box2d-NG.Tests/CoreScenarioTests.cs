using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class CoreScenarioTests
    {
        [TestMethod]
        public void BoxStack_InitialOverlapProducesContacts()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, -1f));
            PolygonShape groundShape = new PolygonShape(new[]
            {
                new Vec2(-5f, -0.5f),
                new Vec2(5f, -0.5f),
                new Vec2(5f, 0.5f),
                new Vec2(-5f, 0.5f)
            });
            ground.CreateFixture(new FixtureDef(groundShape).WithDensity(0f));

            for (int i = 0; i < 3; ++i)
            {
                Body box = world.CreateBody(new BodyDef().AsDynamic().At(0f, i * 0.8f - 0.2f));
                PolygonShape boxShape = new PolygonShape(new[]
                {
                    new Vec2(-0.5f, -0.5f),
                    new Vec2(0.5f, -0.5f),
                    new Vec2(0.5f, 0.5f),
                    new Vec2(-0.5f, 0.5f)
                });
                box.CreateFixture(new FixtureDef(boxShape).WithDensity(1f));
            }

            Contact contact = new Contact(ground.Fixtures[0], world.Bodies[1].Fixtures[0]);
            contact.Evaluate();
            Assert.IsTrue(contact.Manifold.PointCount > 0, "Expected contact between ground and first box.");
        }

        [TestMethod]
        public void CircleHitsBox_ProducesContact()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body box = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            PolygonShape boxShape = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.5f),
                new Vec2(0.5f, -0.5f),
                new Vec2(0.5f, 0.5f),
                new Vec2(-0.5f, 0.5f)
            });
            box.CreateFixture(new FixtureDef(boxShape).WithDensity(0f));

            Body ball = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0.2f));
            ball.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            Contact contact = new Contact(box.Fixtures[0], ball.Fixtures[0]);
            contact.Evaluate();
            Assert.IsTrue(contact.Manifold.PointCount > 0, "Expected a contact between circle and box.");
        }

        [TestMethod]
        public void SegmentGround_ProducesContactsInWorldStep()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-5f, 0f), new Vec2(5f, 0f))).WithDensity(0f));

            PolygonShape boxShape = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.5f),
                new Vec2(0.5f, -0.5f),
                new Vec2(0.5f, 0.5f),
                new Vec2(-0.5f, 0.5f)
            });
            Body box = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0.25f));
            box.CreateFixture(new FixtureDef(boxShape).WithDensity(1f));

            world.Step(1f / 60f);

            Assert.IsTrue(world.Contacts.Count > 0, "Expected contacts between segment ground and box after step.");
        }

        [TestMethod]
        public void SegmentGround_ProducesCircleContactsInWorldStep()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-5f, 0f), new Vec2(5f, 0f))).WithDensity(0f));

            Body circleBody = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0.25f));
            circleBody.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.Step(1f / 60f);

            Assert.IsTrue(world.Contacts.Count > 0, "Expected contacts between segment ground and circle after step.");
        }

        [TestMethod]
        public void SegmentGround_PreventsCircleFromFallingThrough()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-5f, 0f), new Vec2(5f, 0f))).WithDensity(0f));

            Body circleBody = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f));
            circleBody.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            for (int i = 0; i < 240; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(circleBody.Transform.P.Y > -1f, "Circle should not fall far below ground.");
        }
    }
}
