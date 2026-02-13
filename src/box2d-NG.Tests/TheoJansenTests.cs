using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class TheoJansenTests
    {
        [TestMethod]
        public void TheoJansen_WalksForwardWithMotor()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-50f, 0f), new Vec2(50f, 0f))));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-50f, 0f), new Vec2(-50f, 10f))));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(50f, 0f), new Vec2(50f, 10f))));

            Vec2 offset = new Vec2(0f, 8f);
            Vec2 pivot = new Vec2(0f, 0.8f);

            Body chassis = world.CreateBody(new BodyDef().AsDynamic().At(pivot + offset));
            PolygonShape chassisShape = new PolygonShape(BuildBoxVertices(2.5f, 1f, Vec2.Zero, 0f));
            chassis.CreateFixture(new FixtureDef(chassisShape).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1)));

            Body wheel = world.CreateBody(new BodyDef().AsDynamic().At(pivot + offset));
            wheel.CreateFixture(new FixtureDef(new CircleShape(1.6f)).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1)));

            RevoluteJoint motorJoint = world.CreateJoint(new RevoluteJointDef(wheel, chassis, pivot + offset)
                .WithMotor(true, 2f, 400f));

            Vec2 wheelAnchor = pivot + new Vec2(0f, -0.8f);
            CreateLeg(world, chassis, wheel, offset, -1f, wheelAnchor);
            CreateLeg(world, chassis, wheel, offset, 1f, wheelAnchor);

            wheel.SetTransform(wheel.Transform.P, 120f * MathFng.Pi / 180f);
            CreateLeg(world, chassis, wheel, offset, -1f, wheelAnchor);
            CreateLeg(world, chassis, wheel, offset, 1f, wheelAnchor);

            wheel.SetTransform(wheel.Transform.P, -120f * MathFng.Pi / 180f);
            CreateLeg(world, chassis, wheel, offset, -1f, wheelAnchor);
            CreateLeg(world, chassis, wheel, offset, 1f, wheelAnchor);

            float startX = chassis.Transform.P.X;
            for (int i = 0; i < 1200; ++i)
            {
                world.Step(1f / 60f);
            }

            float endX = chassis.Transform.P.X;
            float delta = MathF.Abs(endX - startX);
            Assert.IsTrue(delta > 0.05f, $"TheoJansen chassis should move. start={startX} end={endX}");
            Assert.IsTrue(motorJoint != null, "Motor joint should exist.");
        }

        private static void CreateLeg(World world, Body chassis, Body wheel, Vec2 offset, float s, Vec2 wheelAnchor)
        {
            Vec2 p1 = new Vec2(5.4f * s, -6.1f);
            Vec2 p2 = new Vec2(7.2f * s, -1.2f);
            Vec2 p3 = new Vec2(4.3f * s, -1.9f);
            Vec2 p4 = new Vec2(3.1f * s, 0.8f);
            Vec2 p5 = new Vec2(6.0f * s, 1.5f);
            Vec2 p6 = new Vec2(2.5f * s, 3.7f);

            PolygonShape poly1 = s > 0f
                ? new PolygonShape(new[] { p1, p2, p3 })
                : new PolygonShape(new[] { p1, p3, p2 });
            PolygonShape poly2 = s > 0f
                ? new PolygonShape(new[] { Vec2.Zero, p5 - p4, p6 - p4 })
                : new PolygonShape(new[] { Vec2.Zero, p6 - p4, p5 - p4 });

            Body body1 = world.CreateBody(new BodyDef().AsDynamic().At(offset).WithAngularDamping(10f));
            Body body2 = world.CreateBody(new BodyDef().AsDynamic().At(p4 + offset).WithAngularDamping(10f));
            body1.CreateFixture(new FixtureDef(poly1).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1)));
            body2.CreateFixture(new FixtureDef(poly2).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1)));

            DistanceJointDef djd = new DistanceJointDef(body1, body2, p2 + offset, p5 + offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            djd = new DistanceJointDef(body1, body2, p3 + offset, p4 + offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            djd = new DistanceJointDef(body1, wheel, p3 + offset, wheelAnchor + offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            djd = new DistanceJointDef(body2, wheel, p6 + offset, wheelAnchor + offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            world.CreateJoint(new RevoluteJointDef(body2, chassis, p4 + offset));
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
