using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class ContactTests
    {
        [TestMethod]
        public void CircleCircleContact_CreatesManifold()
        {
            World world = new World(new WorldDef());
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.8f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)));

            Contact contact = new Contact(bodyA.Fixtures[0], bodyB.Fixtures[0]);
            contact.Evaluate();

            Assert.IsTrue(contact.Manifold.PointCount > 0, "Expected a manifold point.");
        }

        [TestMethod]
        public void PolygonPolygonContact_CreatesManifold()
        {
            World world = new World(new WorldDef());
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.9f, 0f));

            PolygonShape squareA = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.5f),
                new Vec2(0.5f, -0.5f),
                new Vec2(0.5f, 0.5f),
                new Vec2(-0.5f, 0.5f)
            });

            PolygonShape squareB = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.5f),
                new Vec2(0.5f, -0.5f),
                new Vec2(0.5f, 0.5f),
                new Vec2(-0.5f, 0.5f)
            });

            bodyA.CreateFixture(new FixtureDef(squareA));
            bodyB.CreateFixture(new FixtureDef(squareB));

            Contact contact = new Contact(bodyA.Fixtures[0], bodyB.Fixtures[0]);
            contact.Evaluate();

            Assert.IsTrue(contact.Manifold.PointCount > 0, "Expected a manifold point.");
        }

        [TestMethod]
        public void CapsuleCircleContact_CreatesManifold()
        {
            World world = new World(new WorldDef());
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            CapsuleShape capsule = new CapsuleShape(new Vec2(-0.5f, 0f), new Vec2(0.5f, 0f), 0.25f);
            bodyA.CreateFixture(new FixtureDef(capsule));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.25f)));

            Contact contact = new Contact(bodyA.Fixtures[0], bodyB.Fixtures[0]);
            contact.Evaluate();

            Assert.IsTrue(contact.Manifold.PointCount > 0, "Expected a manifold point.");
        }

        [TestMethod]
        public void SegmentCircleContact_CreatesManifold()
        {
            World world = new World(new WorldDef());
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body circleBody = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            SegmentShape segment = new SegmentShape(new Vec2(-2f, 0f), new Vec2(2f, 0f));
            CircleShape circle = new CircleShape(0.5f);

            ground.CreateFixture(new FixtureDef(segment));
            circleBody.CreateFixture(new FixtureDef(circle));

            Contact contact = new Contact(ground.Fixtures[0], circleBody.Fixtures[0]);
            contact.Evaluate();

            Assert.IsTrue(contact.Manifold.PointCount > 0, "Expected segment-circle manifold at origin.");
        }

        [TestMethod]
        public void SegmentPolygonContact_CreatesManifold()
        {
            World world = new World(new WorldDef());
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body boxBody = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            SegmentShape segment = new SegmentShape(new Vec2(-2f, 0f), new Vec2(2f, 0f));
            PolygonShape box = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.5f),
                new Vec2(0.5f, -0.5f),
                new Vec2(0.5f, 0.5f),
                new Vec2(-0.5f, 0.5f)
            });

            ground.CreateFixture(new FixtureDef(segment));
            boxBody.CreateFixture(new FixtureDef(box));

            Contact contact = new Contact(ground.Fixtures[0], boxBody.Fixtures[0]);
            contact.Evaluate();

            Assert.IsTrue(contact.Manifold.PointCount > 0, "Expected segment-polygon manifold at origin.");
        }
    }
}
