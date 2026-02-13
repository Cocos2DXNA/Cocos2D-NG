using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class CollisionFilteringSampleTests
    {
        private const int SmallGroup = 1;
        private const int LargeGroup = -1;

        private const ulong DefaultCategory = 0x0001;
        private const ulong TriangleCategory = 0x0002;
        private const ulong BoxCategory = 0x0004;
        private const ulong CircleCategory = 0x0008;

        private const ulong TriangleMask = 0xFFFF;
        private const ulong BoxMask = 0xFFFF ^ TriangleCategory;
        private const ulong CircleMask = 0xFFFF;

        [TestMethod]
        public void CollisionFilteringSample_GeneratesContactsAgainstGround()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f)))
                .WithFriction(0.3f));

            Vec2[] vertices =
            {
                new Vec2(-1f, 0f),
                new Vec2(1f, 0f),
                new Vec2(0f, 2f)
            };
            PolygonShape polygon = new PolygonShape(vertices);

            FixtureDef triangleDef = new FixtureDef(polygon).WithDensity(1f)
                .WithFilter(new Filter(TriangleCategory, TriangleMask, SmallGroup));
            world.CreateBody(new BodyDef().AsDynamic().At(-5f, 2f)).CreateFixture(triangleDef);

            Vec2[] bigVertices =
            {
                vertices[0] * 2f,
                vertices[1] * 2f,
                vertices[2] * 2f
            };
            polygon = new PolygonShape(bigVertices);
            FixtureDef bigTriangleDef = new FixtureDef(polygon).WithDensity(1f)
                .WithFilter(new Filter(TriangleCategory, TriangleMask, LargeGroup));
            Body body2 = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 6f).LockRotation(true));
            body2.CreateFixture(bigTriangleDef);

            Body body = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 10f));
            PolygonShape p = new PolygonShape(BuildBoxVertices(0.5f, 1f, Vec2.Zero, 0f));
            body.CreateFixture(new FixtureDef(p).WithDensity(1f));

            PrismaticJointDef jd = new PrismaticJointDef(body2, body, body2.Transform.P + new Vec2(0f, 4f), new Vec2(0f, 1f))
                .WithLimit(-1f, 1f);
            world.CreateJoint(jd);

            PolygonShape box = new PolygonShape(BuildBoxVertices(1f, 0.5f, Vec2.Zero, 0f));
            FixtureDef boxDef = new FixtureDef(box).WithDensity(1f).WithRestitution(0.1f)
                .WithFilter(new Filter(BoxCategory, BoxMask, SmallGroup));
            world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f)).CreateFixture(boxDef);

            PolygonShape bigBox = new PolygonShape(BuildBoxVertices(2f, 1f, Vec2.Zero, 0f));
            FixtureDef bigBoxDef = new FixtureDef(bigBox).WithDensity(1f).WithRestitution(0.1f)
                .WithFilter(new Filter(BoxCategory, BoxMask, LargeGroup));
            world.CreateBody(new BodyDef().AsDynamic().At(0f, 6f)).CreateFixture(bigBoxDef);

            CircleShape circle = new CircleShape(1f);
            FixtureDef circleDef = new FixtureDef(circle).WithDensity(1f)
                .WithFilter(new Filter(CircleCategory, CircleMask, SmallGroup));
            world.CreateBody(new BodyDef().AsDynamic().At(5f, 2f)).CreateFixture(circleDef);

            CircleShape bigCircle = new CircleShape(2f);
            FixtureDef bigCircleDef = new FixtureDef(bigCircle).WithDensity(1f)
                .WithFilter(new Filter(CircleCategory, CircleMask, LargeGroup));
            world.CreateBody(new BodyDef().AsDynamic().At(5f, 6f)).CreateFixture(bigCircleDef);

            int maxContacts = 0;
            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
                maxContacts = Math.Max(maxContacts, world.Contacts.Count);
            }
            Assert.IsTrue(maxContacts > 0, "Expected contacts in collision filtering sample (ground should collide).");
        }

        private static Vec2[] BuildBoxVertices(float hx, float hy, Vec2 center, float angle)
        {
            Vec2[] verts =
            {
                new Vec2(-hx, -hy),
                new Vec2(hx, -hy),
                new Vec2(hx, hy),
                new Vec2(-hx, hy)
            };
            if (angle == 0f)
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    verts[i] = verts[i] + center;
                }
                return verts;
            }

            Rot rot = new Rot(angle);
            for (int i = 0; i < verts.Length; ++i)
            {
                verts[i] = Rot.Mul(rot, verts[i]) + center;
            }
            return verts;
        }
    }
}
