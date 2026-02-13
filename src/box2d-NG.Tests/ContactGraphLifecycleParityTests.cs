using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class ContactGraphLifecycleParityTests
    {
        [TestMethod]
        public void ContactColor_Lifecycle_TouchSeparateRetouch()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(false));

            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(3.5f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(1f)).WithDensity(1f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(1f)).WithDensity(1f));

            world.Step(1f / 60f);
            Assert.IsNull(FindContact(world, bodyA, bodyB), "Expected no contact before overlap.");

            bodyA.SetAwake(true);
            bodyB.SetAwake(true);
            bodyB.SetTransform(new Vec2(1.5f, 0f), 0f);
            world.Step(1f / 60f);

            Contact contact = RequireContact(world, bodyA, bodyB);
            Assert.IsTrue(contact.IsTouching, "Expected touching contact after overlap.");
            Assert.AreEqual(SolverSetType.Awake, contact.SolverSetType, "Expected awake solver set while active.");
            Assert.IsTrue(contact.ColorIndex >= 0 && contact.ColorIndex < Constants.GraphColorCount, "Expected valid graph color.");

            bodyA.SetAwake(true);
            bodyB.SetAwake(true);
            bodyB.SetTransform(new Vec2(6f, 0f), 0f);
            world.Step(1f / 60f);

            Contact? separated = FindContact(world, bodyA, bodyB);
            if (separated != null)
            {
                Assert.IsFalse(separated.IsTouching, "Expected non-touching if contact persists after separation.");
                Assert.AreEqual(-1, separated.ColorIndex, "Expected non-touching contact to be removed from graph colors.");
            }

            bodyA.SetAwake(true);
            bodyB.SetAwake(true);
            bodyB.SetTransform(new Vec2(1.5f, 0f), 0f);
            world.Step(1f / 60f);

            Contact retouch = RequireContact(world, bodyA, bodyB);
            Assert.IsTrue(retouch.IsTouching, "Expected touching contact after re-overlap.");
            Assert.AreEqual(SolverSetType.Awake, retouch.SolverSetType, "Expected awake solver set after re-overlap.");
            Assert.IsTrue(retouch.ColorIndex >= 0 && retouch.ColorIndex < Constants.GraphColorCount, "Expected valid graph color after re-overlap.");

            AssertAwakeTouchingContactsHaveColors(world);
        }

        [TestMethod]
        public void ContactColor_Lifecycle_SleepWakeSleep()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));

            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f).IsAwake(false));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1.5f, 0f).IsAwake(false));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(1f)).WithDensity(1f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(1f)).WithDensity(1f));

            world.Step(1f / 60f);
            Contact sleeping = RequireContact(world, bodyA, bodyB);
            Assert.IsTrue(sleeping.IsTouching, "Expected touching sleeping contact.");
            Assert.AreEqual(SolverSetType.Sleeping, sleeping.SolverSetType, "Expected sleeping solver set.");
            Assert.AreEqual(-1, sleeping.ColorIndex, "Expected no graph color while sleeping.");
            Assert.IsTrue(sleeping.SolverSetId != 0, "Expected non-zero sleeping solver set id.");

            bodyA.SetAwake(true);
            world.Step(1f / 60f);

            Contact awake = RequireContact(world, bodyA, bodyB);
            Assert.IsTrue(awake.IsTouching, "Expected touching contact after wake.");
            Assert.AreEqual(SolverSetType.Awake, awake.SolverSetType, "Expected awake solver set after wake.");
            Assert.IsTrue(awake.ColorIndex >= 0 && awake.ColorIndex < Constants.GraphColorCount, "Expected graph color after wake.");

            bodyA.SetAwake(false);
            bodyB.SetAwake(false);
            world.Step(1f / 60f);

            Contact sleepingAgain = RequireContact(world, bodyA, bodyB);
            Assert.IsTrue(sleepingAgain.IsTouching, "Expected touching contact after re-sleep.");
            Assert.AreEqual(SolverSetType.Sleeping, sleepingAgain.SolverSetType, "Expected sleeping solver set after re-sleep.");
            Assert.AreEqual(-1, sleepingAgain.ColorIndex, "Expected no graph color after re-sleep.");
        }

        [TestMethod]
        public void ContactColor_Lifecycle_BodyTypeFlip()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(20f, 0.5f, new Vec2(0f, -0.5f), 0f))));

            Body mover = world.CreateBody(new BodyDef().AsDynamic().At(0f, 3f));
            mover.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(0.5f, 0.5f, Vec2.Zero, 0f))).WithDensity(1f));

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            Contact dynamicContact = RequireContact(world, ground, mover);
            Assert.IsTrue(dynamicContact.IsTouching, "Expected touching dynamic-ground contact.");
            Assert.AreEqual(SolverSetType.Awake, dynamicContact.SolverSetType, "Expected awake solver set while dynamic.");
            Assert.IsTrue(dynamicContact.ColorIndex >= 0 && dynamicContact.ColorIndex < Constants.GraphColorCount, "Expected graph color while dynamic.");

            mover.SetType(BodyType.Kinematic);
            mover.LinearVelocity = new Vec2(2f, 0f);
            for (int i = 0; i < 30; ++i)
            {
                world.Step(1f / 60f);
            }
            AssertAwakeTouchingContactsHaveColors(world);

            mover.SetType(BodyType.Static);
            for (int i = 0; i < 2; ++i)
            {
                world.Step(1f / 60f);
            }
            Contact? staticPair = FindContact(world, ground, mover);
            if (staticPair != null)
            {
                Assert.AreEqual(-1, staticPair.ColorIndex, "Expected no graph color for static-static pair.");
            }

            mover.SetType(BodyType.Dynamic);
            mover.SetAwake(true);
            mover.SetTransform(new Vec2(0f, 2f), 0f);
            mover.LinearVelocity = Vec2.Zero;

            for (int i = 0; i < 30; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(mover.Fixtures.Count > 0, "Expected mover fixtures to remain after body type flips.");
            AssertAwakeTouchingContactsHaveColors(world);
        }

        private static Contact RequireContact(World world, Body a, Body b)
        {
            Contact? c = FindContact(world, a, b);
            Assert.IsNotNull(c, "Expected contact between requested bodies.");
            return c!;
        }

        private static Contact? FindContact(World world, Body a, Body b)
        {
            for (int i = 0; i < world.Contacts.Count; ++i)
            {
                Contact c = world.Contacts[i];
                if (c.FixtureA == null || c.FixtureB == null)
                {
                    continue;
                }

                Body ca = c.FixtureA.Body;
                Body cb = c.FixtureB.Body;
                if ((ca == a && cb == b) || (ca == b && cb == a))
                {
                    return c;
                }
            }

            return null;
        }

        private static void AssertAwakeTouchingContactsHaveColors(World world)
        {
            for (int i = 0; i < world.Contacts.Count; ++i)
            {
                Contact c = world.Contacts[i];
                if (!c.IsTouching || c.SolverSetType != SolverSetType.Awake)
                {
                    continue;
                }

                if (c.FixtureA?.IsSensor == true || c.FixtureB?.IsSensor == true)
                {
                    continue;
                }

                Assert.IsTrue(c.ColorIndex >= 0 && c.ColorIndex < Constants.GraphColorCount,
                    "Expected awake touching non-sensor contact to have valid graph color.");
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
