using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class SliderCrankSampleTests
    {
        [TestMethod]
        public void SliderCrank_CrankRotatesAndSliderStaysCentered()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Filter connectedFilter = new Filter(1, ulong.MaxValue, -1);

            Body prevBody = ground;
            Body crankBody;
            Body pistonBody;

            // Crank
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 2f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 7f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 5f))
                    .WithMotor(true, MathFng.Pi, 10000f);
                world.CreateJoint(rjd);

                prevBody = body;
                crankBody = body;
            }

            // Follower
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 4f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 13f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 9f));
                world.CreateJoint(rjd);

                prevBody = body;
            }

            // Piston
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 17f).LockRotation(true));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 17f));
                world.CreateJoint(rjd);

                PrismaticJointDef pjd = new PrismaticJointDef(ground, body, new Vec2(0f, 17f), new Vec2(0f, 1f))
                    .WithMotor(0f, 1000f);
                world.CreateJoint(pjd);

                pistonBody = body;
            }

            // Payload
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 23f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f));
            }

            float accumulatedRotation = 0f;
            float maxAbsX = 0f;
            for (int i = 0; i < 240; ++i)
            {
                world.Step(1f / 60f);
                accumulatedRotation += crankBody.AngularVelocity * (1f / 60f);
                float absX = MathF.Abs(pistonBody.Transform.P.X);
                if (absX > maxAbsX)
                {
                    maxAbsX = absX;
                }
            }

            Assert.IsTrue(accumulatedRotation > 6f, $"Expected crank rotation, got {accumulatedRotation} rad.");
            Assert.IsTrue(maxAbsX < 0.2f, $"Piston drifted off axis: {maxAbsX}");
        }

        [TestMethod]
        public void SliderCrank_PayloadStaysOnPiston()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Filter connectedFilter = new Filter(1, ulong.MaxValue, -1);

            Body prevBody = ground;
            Body pistonBody;
            Body payloadBody;

            // Crank
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 2f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 7f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 5f))
                    .WithMotor(true, MathFng.Pi, 10000f);
                world.CreateJoint(rjd);

                prevBody = body;
            }

            // Follower
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 4f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 13f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 9f));
                world.CreateJoint(rjd);

                prevBody = body;
            }

            // Piston
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 17f).LockRotation(true));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 17f));
                world.CreateJoint(rjd);

                PrismaticJointDef pjd = new PrismaticJointDef(ground, body, new Vec2(0f, 17f), new Vec2(0f, 1f))
                    .WithMotor(0f, 1000f);
                world.CreateJoint(pjd);

                pistonBody = body;
            }

            // Payload
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 23f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f));
                payloadBody = body;
            }

            float minGap = float.MaxValue;
            float offsetAtMinGap = 0f;
            for (int i = 0; i < 240; ++i)
            {
                world.Step(1f / 60f);
                float verticalGap = payloadBody.Transform.P.Y - pistonBody.Transform.P.Y;
                if (verticalGap < minGap)
                {
                    minGap = verticalGap;
                    offsetAtMinGap = MathF.Abs(payloadBody.Transform.P.X - pistonBody.Transform.P.X);
                }
            }

            Assert.IsTrue(minGap < 4f, $"Expected payload to reach piston vicinity. minGap={minGap}");
            Assert.IsTrue(offsetAtMinGap < 0.35f, $"Payload slid off piston: {offsetAtMinGap}");
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
