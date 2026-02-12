using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class SolverDeterminismParityTests
    {
        [TestMethod]
        public void SolverPipeline_MixedScene_IsDeterministicFrameByFrame()
        {
            World worldA = CreateMixedWorld(out PrismaticJoint sliderA, out RevoluteJoint rotorA);
            World worldB = CreateMixedWorld(out PrismaticJoint sliderB, out RevoluteJoint rotorB);

            FrameEventCounter eventsA = new FrameEventCounter(worldA);
            FrameEventCounter eventsB = new FrameEventCounter(worldB);

            const float dt = 1f / 60f;
            const int steps = 240;
            for (int i = 0; i < steps; ++i)
            {
                ApplyInputs(i, sliderA, rotorA);
                ApplyInputs(i, sliderB, rotorB);

                eventsA.Reset();
                eventsB.Reset();

                worldA.Step(dt);
                worldB.Step(dt);

                AssertEventParity(i, eventsA, eventsB);
                AssertWorldParity(i, worldA, worldB);
            }
        }

        private static World CreateMixedWorld(out PrismaticJoint slider, out RevoluteJoint rotor)
        {
            WorldDef def = new WorldDef()
                .WithGravity(new Vec2(0f, -10f))
                .EnableSleeping(false)
                .EnableContinuousCollision(true)
                .WithMaxSubSteps(8);
            World world = new World(def);

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body crank = world.CreateBody(new BodyDef().AsDynamic().At(-8f, 4f));
            crank.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(0.4f, 1.8f, Vec2.Zero, 0f))).WithDensity(1.5f));
            rotor = world.CreateJoint(new RevoluteJointDef(ground, crank, new Vec2(-8f, 4f)).WithMotor(true, 2f, 2000f));

            Body sliderBody = world.CreateBody(new BodyDef().AsDynamic().At(-4f, 2f).LockRotation(true));
            sliderBody.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(0.8f, 0.8f, Vec2.Zero, 0f))).WithDensity(1.0f));
            slider = world.CreateJoint(new PrismaticJointDef(ground, sliderBody, new Vec2(-4f, 2f), new Vec2(1f, 0f))
                .WithLimit(-3f, 3f)
                .WithMotor(2f, 2000f));

            Body payload = world.CreateBody(new BodyDef().AsDynamic().At(-4f, 6f));
            payload.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(0.9f, 0.9f, Vec2.Zero, 0f))).WithDensity(1f));

            Body stack1 = world.CreateBody(new BodyDef().AsDynamic().At(2f, 2f));
            Body stack2 = world.CreateBody(new BodyDef().AsDynamic().At(2f, 4.2f));
            Body stack3 = world.CreateBody(new BodyDef().AsDynamic().At(2f, 6.4f));
            PolygonShape box = new PolygonShape(BuildBoxVertices(1f, 1f, Vec2.Zero, 0f));
            stack1.CreateFixture(new FixtureDef(box).WithDensity(1f));
            stack2.CreateFixture(new FixtureDef(box).WithDensity(1f));
            stack3.CreateFixture(new FixtureDef(box).WithDensity(1f));

            Body sensorBody = world.CreateBody(new BodyDef().AsStatic().At(10f, 2.2f));
            sensorBody.CreateFixture(new FixtureDef(new CircleShape(1.0f)).AsSensor().WithUserData("sensor"));

            Body fastMover = world.CreateBody(new BodyDef().AsDynamic().At(-15f, 2.2f).IsBullet(false));
            fastMover.CreateFixture(new FixtureDef(new CircleShape(0.35f)).WithDensity(1f).WithUserData("mover"));
            fastMover.LinearVelocity = new Vec2(45f, 0f);

            return world;
        }

        private static void ApplyInputs(int frame, PrismaticJoint slider, RevoluteJoint rotor)
        {
            float direction = (frame / 30) % 2 == 0 ? 1f : -1f;
            slider.SetMotorSpeed(2.5f * direction);
            rotor.SetMotorSpeed(2.0f * direction);
        }

        private static void AssertEventParity(int frame, FrameEventCounter a, FrameEventCounter b)
        {
            Assert.AreEqual(a.ContactBegin, b.ContactBegin, $"Frame {frame}: contact begin mismatch.");
            Assert.AreEqual(a.ContactEnd, b.ContactEnd, $"Frame {frame}: contact end mismatch.");
            Assert.AreEqual(a.ContactHit, b.ContactHit, $"Frame {frame}: contact hit mismatch.");
            Assert.AreEqual(a.SensorBegin, b.SensorBegin, $"Frame {frame}: sensor begin mismatch.");
            Assert.AreEqual(a.SensorEnd, b.SensorEnd, $"Frame {frame}: sensor end mismatch.");
            Assert.AreEqual(a.SensorHit, b.SensorHit, $"Frame {frame}: sensor hit mismatch.");
        }

        private static void AssertWorldParity(int frame, World a, World b)
        {
            Assert.AreEqual(a.Bodies.Count, b.Bodies.Count, $"Frame {frame}: body count mismatch.");
            Assert.AreEqual(a.Contacts.Count, b.Contacts.Count, $"Frame {frame}: contact count mismatch.");

            const float tol = 1e-5f;
            for (int i = 0; i < a.Bodies.Count; ++i)
            {
                Body ba = a.Bodies[i];
                Body bb = b.Bodies[i];
                Assert.AreEqual(ba.Id, bb.Id, $"Frame {frame}: body id mismatch at index {i}.");
                AssertNear(ba.Transform.P.X, bb.Transform.P.X, tol, $"Frame {frame}: body[{i}].x mismatch.");
                AssertNear(ba.Transform.P.Y, bb.Transform.P.Y, tol, $"Frame {frame}: body[{i}].y mismatch.");
                AssertNear(ba.Transform.Q.Angle, bb.Transform.Q.Angle, tol, $"Frame {frame}: body[{i}].angle mismatch.");
                AssertNear(ba.LinearVelocity.X, bb.LinearVelocity.X, tol, $"Frame {frame}: body[{i}].vx mismatch.");
                AssertNear(ba.LinearVelocity.Y, bb.LinearVelocity.Y, tol, $"Frame {frame}: body[{i}].vy mismatch.");
                AssertNear(ba.AngularVelocity, bb.AngularVelocity, tol, $"Frame {frame}: body[{i}].w mismatch.");
            }

            List<string> sigA = BuildContactSignature(a);
            List<string> sigB = BuildContactSignature(b);
            Assert.AreEqual(sigA.Count, sigB.Count, $"Frame {frame}: contact signature length mismatch.");
            for (int i = 0; i < sigA.Count; ++i)
            {
                Assert.AreEqual(sigA[i], sigB[i], $"Frame {frame}: contact signature mismatch at {i}.");
            }
        }

        private static List<string> BuildContactSignature(World world)
        {
            List<string> signature = new List<string>(world.Contacts.Count);
            for (int i = 0; i < world.Contacts.Count; ++i)
            {
                Contact c = world.Contacts[i];
                if (c.FixtureA == null || c.FixtureB == null)
                {
                    continue;
                }

                int a = c.FixtureA.Id;
                int b = c.FixtureB.Id;
                if (a > b)
                {
                    int t = a;
                    a = b;
                    b = t;
                }

                signature.Add($"{a}:{b}:t{(c.IsTouching ? 1 : 0)}:pc{c.Manifold.PointCount}:ci{c.ColorIndex}:ss{(int)c.SolverSetType}");
            }

            signature.Sort(System.StringComparer.Ordinal);
            return signature;
        }

        private static void AssertNear(float a, float b, float tolerance, string message)
        {
            Assert.IsTrue(System.MathF.Abs(a - b) <= tolerance, $"{message} a={a} b={b}");
        }

        private sealed class FrameEventCounter
        {
            public int ContactBegin { get; private set; }
            public int ContactEnd { get; private set; }
            public int ContactHit { get; private set; }
            public int SensorBegin { get; private set; }
            public int SensorEnd { get; private set; }
            public int SensorHit { get; private set; }

            public FrameEventCounter(World world)
            {
                world.Events.ContactEvents += e =>
                {
                    ContactBegin += e.Begin?.Length ?? 0;
                    ContactEnd += e.End?.Length ?? 0;
                    ContactHit += e.Hit?.Length ?? 0;
                };
                world.Events.SensorEvents += e =>
                {
                    SensorBegin += e.Begin?.Length ?? 0;
                    SensorEnd += e.End?.Length ?? 0;
                };
                world.Events.SensorHitEvents += e =>
                {
                    SensorHit += e.Events?.Length ?? 0;
                };
            }

            public void Reset()
            {
                ContactBegin = 0;
                ContactEnd = 0;
                ContactHit = 0;
                SensorBegin = 0;
                SensorEnd = 0;
                SensorHit = 0;
            }
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
