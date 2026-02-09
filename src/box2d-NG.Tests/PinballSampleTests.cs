using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Box2DNG.Tests
{
    [TestClass]
    public class PinballSampleTests
    {
        [TestMethod]
        public void PinballBallHitsFlipper()
        {
            WorldDef def = new WorldDef()
                .WithGravity(new Vec2(0f, -10f))
                .EnableContinuousCollision(true)
                .WithMaxSubSteps(16)
                .WithVelocityIterations(20)
                .WithPositionIterations(8);

            World world = new World(def);
            BuildPinball(world, out Body ball, out RevoluteJoint leftJoint, out RevoluteJoint rightJoint);
            ball.SetTransform(new Vec2(-2f, 2f), 0f);

            float minY = ball.GetWorldCenter().Y;
            for (int i = 0; i < 60; ++i)
            {
                leftJoint.SetMotorSpeed(-10f);
                rightJoint.SetMotorSpeed(10f);
                world.Step(1f / 60f);
                float y = ball.GetWorldCenter().Y;
                if (y < minY)
                {
                    minY = y;
                }
            }

            Assert.IsTrue(minY > -2.5f, $"Expected ball to collide with flippers, min y={minY}.");
        }

        [TestMethod]
        public void PinballBallHitsFastFlippers()
        {
            WorldDef def = new WorldDef()
                .WithGravity(new Vec2(0f, -10f))
                .EnableContinuousCollision(true)
                .WithMaxSubSteps(16)
                .WithVelocityIterations(20)
                .WithPositionIterations(8);

            World world = new World(def);
            BuildPinball(world, out Body ball, out RevoluteJoint leftJoint, out RevoluteJoint rightJoint);
            ball.SetTransform(new Vec2(-2f, 2f), 0f);

            float minY = ball.GetWorldCenter().Y;
            bool hadContact = false;
            for (int i = 0; i < 120; ++i)
            {
                leftJoint.SetMotorSpeed(20f);
                rightJoint.SetMotorSpeed(-20f);
                world.Step(1f / 60f);
                float y = ball.GetWorldCenter().Y;
                if (y < minY)
                {
                    minY = y;
                }

                for (int c = 0; c < world.Contacts.Count; ++c)
                {
                    Contact contact = world.Contacts[c];
                    if (contact.FixtureA == null || contact.FixtureB == null)
                    {
                        continue;
                    }

                    if (contact.Manifold.PointCount == 0)
                    {
                        continue;
                    }

                    if (contact.FixtureA.Body == ball || contact.FixtureB.Body == ball)
                    {
                        hadContact = true;
                    }
                }
            }

            Assert.IsTrue(minY > -2.5f, $"Expected ball to collide with fast flippers, min y={minY}.");
            Assert.IsTrue(hadContact, "Expected ball to register at least one contact.");
        }

        private static void BuildPinball(World world, out Body ball, out RevoluteJoint leftJoint, out RevoluteJoint rightJoint)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            Vec2[] loop =
            {
                new Vec2(0f, -2f),
                new Vec2(8f, 6f),
                new Vec2(8f, 20f),
                new Vec2(-8f, 20f),
                new Vec2(-8f, 6f)
            };

            for (int i = 0; i < loop.Length; ++i)
            {
                int j = (i + 1) % loop.Length;
                Vec2 p1 = loop[i];
                Vec2 p2 = loop[j];
                Vec2 ghost1 = loop[i == 0 ? loop.Length - 1 : i - 1];
                Vec2 ghost2 = loop[(j + 1) % loop.Length];
                ChainSegmentShape seg = new ChainSegmentShape(p1, p2, ghost1, ghost2);
                ground.CreateFixture(new FixtureDef(seg));
            }

            Vec2 pLeft = new Vec2(-2f, 0f);
            Vec2 pRight = new Vec2(2f, 0f);

            Body leftFlipper = world.CreateBody(new BodyDef().AsDynamic().At(pLeft));
            Body rightFlipper = world.CreateBody(new BodyDef().AsDynamic().At(pRight));

            PolygonShape flipperShape = new PolygonShape(BuildBoxVertices(1.75f, 0.1f, Vec2.Zero, 0f));
            FixtureDef fd = new FixtureDef(flipperShape).WithDensity(1f);
            leftFlipper.CreateFixture(fd);
            rightFlipper.CreateFixture(fd);

            leftJoint = world.CreateJoint(new RevoluteJointDef(ground, leftFlipper, pLeft)
                .WithMotor(true, 0f, 1000f)
                .WithLimit(-30f * MathFng.Pi / 180f, 5f * MathFng.Pi / 180f));

            rightJoint = world.CreateJoint(new RevoluteJointDef(ground, rightFlipper, pRight)
                .WithMotor(true, 0f, 1000f)
                .WithLimit(-5f * MathFng.Pi / 180f, 30f * MathFng.Pi / 180f));

            ball = world.CreateBody(new BodyDef().AsDynamic().At(1f, 15f).IsBullet(true));
            ball.CreateFixture(new FixtureDef(new CircleShape(0.2f)).WithDensity(1f));
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
