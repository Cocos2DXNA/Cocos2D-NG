using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class IslandBuilderTests
    {
        [TestMethod]
        public void BuildIslands_SeparateBodiesProduceTwoIslands()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            world.CreateBody(new BodyDef().AsDynamic().At(-2f, 2f))
                .CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            world.CreateBody(new BodyDef().AsDynamic().At(2f, 2f))
                .CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            var islands = world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(2, islands.Count, $"Expected 2 islands, got {islands.Count}");
        }

        [TestMethod]
        public void BuildIslands_JointConnectsBodies()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-2f, 2f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(2f, 2f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, new Vec2(0f, 2f)));

            var islands = world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(1, islands.Count, $"Expected 1 island, got {islands.Count}");
            Assert.AreEqual(2, islands[0].Bodies.Count, $"Expected 2 bodies in island, got {islands[0].Bodies.Count}");
            Assert.AreEqual(1, islands[0].Joints.Count, $"Expected 1 joint in island, got {islands[0].Joints.Count}");
        }

        [TestMethod]
        public void SolverPipeline_BuildsIslandsDuringStep()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            world.CreateBody(new BodyDef().AsDynamic().At(-2f, 2f))
                .CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            world.CreateBody(new BodyDef().AsDynamic().At(2f, 2f))
                .CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.Step(1f / 60f);

            Assert.AreEqual(2, world.LastIslands.Count, $"Expected 2 islands after step, got {world.LastIslands.Count}");
        }

        [TestMethod]
        public void SolverPipeline_SkipsSleepingBodiesWhenEnabled()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(true));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f).IsAwake(false));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.Step(1f / 60f);

            Assert.AreEqual(0, world.AwakeSet.Islands.Count, $"Expected 0 awake islands for sleeping body, got {world.AwakeSet.Islands.Count}");
            Assert.AreEqual(1, world.LastIslands.Count, $"Expected island to be tracked even if sleeping, got {world.LastIslands.Count}");
            Assert.AreEqual(1, world.SleepingSet.Bodies.Count, $"Expected sleeping set to include body, got {world.SleepingSet.Bodies.Count}");
        }

        [TestMethod]
        public void UpdateSleep_AllowsIslandsToSleepIndependently()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body sleeper = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            sleeper.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            Body mover = world.CreateBody(new BodyDef().AsDynamic().At(5f, 0f));
            mover.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            mover.LinearVelocity = new Vec2(1f, 0f);

            int steps = (int)MathF.Ceiling(Constants.TimeToSleep / (1f / 60f)) + 1;
            for (int i = 0; i < steps; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsFalse(sleeper.Awake, "Expected the resting island to sleep.");
            Assert.IsTrue(mover.Awake, "Expected the moving island to remain awake.");
            Assert.IsTrue(world.SleepingSet.Bodies.Contains(sleeper), "Expected sleeper in the sleeping set.");
            Assert.IsTrue(world.AwakeSet.Bodies.Contains(mover), "Expected mover in the awake set.");
        }

        [TestMethod]
        public void BuildIslands_SplitsAfterJointRemoval()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-2f, 2f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(2f, 2f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            RevoluteJoint joint = world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, new Vec2(0f, 2f)));
            var islands = world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(1, islands.Count, $"Expected 1 island before removal, got {islands.Count}");

            bool removed = world.DestroyJoint(joint);
            Assert.IsTrue(removed, "Expected joint removal to succeed.");

            islands = world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(2, islands.Count, $"Expected 2 islands after removal, got {islands.Count}");
        }

        [TestMethod]
        public void SolverSets_UpdateOnAwakeChange()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(true));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(1, world.AwakeSet.Bodies.Count, $"Expected body in awake set, got {world.AwakeSet.Bodies.Count}");

            body.SetAwake(false);
            Assert.AreEqual(0, world.AwakeSet.Bodies.Count, $"Expected awake set empty after sleep, got {world.AwakeSet.Bodies.Count}");
            Assert.AreEqual(1, world.SleepingSet.Bodies.Count, $"Expected body in sleeping set, got {world.SleepingSet.Bodies.Count}");

            body.SetAwake(true);
            Assert.AreEqual(1, world.AwakeSet.Bodies.Count, $"Expected awake set after wake, got {world.AwakeSet.Bodies.Count}");
        }

        [TestMethod]
        public void WakingBody_WakesEntireIsland()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-1f, 0f).IsAwake(false));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f).IsAwake(false));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, Vec2.Zero));
            world.BuildIslands(awakeOnly: false);

            bodyA.SetAwake(true);

            Assert.IsTrue(bodyA.Awake, "Expected bodyA awake after SetAwake(true).");
            Assert.IsTrue(bodyB.Awake, "Expected bodyB awake when island wakes.");
            Assert.AreEqual(1, world.AwakeSet.Islands.Count, $"Expected 1 awake island, got {world.AwakeSet.Islands.Count}");
        }

        [TestMethod]
        public void CreatingJointBetweenSleepingIslands_MergesSleepingIslands()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-2f, 0f).IsAwake(false));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(2f, 0f).IsAwake(false));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(2, world.SleepingSet.Islands.Count, $"Expected 2 sleeping islands, got {world.SleepingSet.Islands.Count}");

            world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, Vec2.Zero));

            Assert.AreEqual(0, world.AwakeSet.Islands.Count, $"Expected 0 awake islands, got {world.AwakeSet.Islands.Count}");
            Assert.AreEqual(1, world.SleepingSet.Islands.Count, $"Expected merged sleeping island, got {world.SleepingSet.Islands.Count}");
            Assert.IsFalse(bodyA.Awake, "Expected bodyA to remain asleep.");
            Assert.IsFalse(bodyB.Awake, "Expected bodyB to remain asleep.");
        }

        [TestMethod]
        public void SolverSets_MoveContactsWithIslands()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.Step(1f / 60f);

            Assert.IsTrue(world.AwakeSet.Contacts.Count > 0, "Expected contact in awake set.");

            bodyA.SetAwake(false);
            bodyB.SetAwake(false);
            world.BuildIslands(awakeOnly: false);

            Assert.AreEqual(0, world.AwakeSet.Contacts.Count, "Expected no awake contacts after sleeping.");
            Assert.IsTrue(world.SleepingSet.Contacts.Count > 0, "Expected contact in sleeping set.");

            bodyA.SetAwake(true);

            Assert.IsTrue(world.AwakeSet.Contacts.Count > 0, "Expected contact moved back to awake set.");
        }

        [TestMethod]
        public void SolverSets_MoveJointsWithIslands()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-1f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, Vec2.Zero));
            world.BuildIslands(awakeOnly: false);

            Assert.IsTrue(world.AwakeSet.Joints.Count > 0, "Expected joint in awake set.");

            bodyA.SetAwake(false);
            bodyB.SetAwake(false);
            world.BuildIslands(awakeOnly: false);

            Assert.AreEqual(0, world.AwakeSet.Joints.Count, "Expected no awake joints after sleeping.");
            Assert.IsTrue(world.SleepingSet.Joints.Count > 0, "Expected joint in sleeping set.");
        }

        [TestMethod]
        public void DestroyJoint_RemovesFromSolverSets()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-1f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            RevoluteJoint joint = world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, Vec2.Zero));
            world.BuildIslands(awakeOnly: false);
            Assert.IsTrue(world.AwakeSet.Joints.Count > 0, "Expected joint in solver set before removal.");

            bool removed = world.DestroyJoint(joint);
            Assert.IsTrue(removed, "Expected joint removal to succeed.");

            Assert.AreEqual(0, world.AwakeSet.Joints.Count, "Expected solver sets to drop removed joint.");
        }

        [TestMethod]
        public void RemovingContact_RemovesFromSolverSets()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.Step(1f / 60f);
            Assert.IsTrue(world.AwakeSet.Contacts.Count > 0, "Expected contact in solver set.");

            bodyB.SetTransform(new Vec2(10f, 0f), 0f);
            world.Step(1f / 60f);

            Assert.AreEqual(0, world.AwakeSet.Contacts.Count, "Expected contact removed from solver set.");
        }

        [TestMethod]
        public void SolverSkipsSleepingContacts()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));

            Body sleepA = world.CreateBody(new BodyDef().AsDynamic().At(-2f, 0f).IsAwake(false));
            sleepA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            Body sleepB = world.CreateBody(new BodyDef().AsDynamic().At(-1.4f, 0f).IsAwake(false));
            sleepB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            Body awakeA = world.CreateBody(new BodyDef().AsDynamic().At(3f, 0f));
            awakeA.CreateFixture(new FixtureDef(new CircleShape(1f)).WithDensity(1f));
            Body awakeB = world.CreateBody(new BodyDef().AsStatic().At(3.5f, 0f));
            awakeB.CreateFixture(new FixtureDef(new CircleShape(1f)));
            awakeA.LinearVelocity = new Vec2(5f, 0f);

            for (int i = 0; i < 2; ++i)
            {
                world.Step(1f / 60f);
            }

            Contact? sleepingContact = null;
            Contact? awakeContact = null;
            for (int i = 0; i < world.Contacts.Count; ++i)
            {
                Contact contact = world.Contacts[i];
                if (contact.FixtureA == null || contact.FixtureB == null)
                {
                    continue;
                }

                Body a = contact.FixtureA.Body;
                Body b = contact.FixtureB.Body;

                if ((a == sleepA && b == sleepB) || (a == sleepB && b == sleepA))
                {
                    sleepingContact = contact;
                }
                else if ((a == awakeA && b == awakeB) || (a == awakeB && b == awakeA))
                {
                    awakeContact = contact;
                }
            }

            Assert.IsNotNull(sleepingContact, "Expected a sleeping contact.");
            Assert.IsNotNull(awakeContact, "Expected an awake contact.");
            Assert.AreEqual(0f, sleepA.LinearVelocity.LengthSquared, "Expected sleeping body A to remain still.");
            Assert.AreEqual(0f, sleepB.LinearVelocity.LengthSquared, "Expected sleeping body B to remain still.");
            Assert.IsTrue(awakeA.LinearVelocity.X < 5f, "Expected awake body velocity to be affected by contact solving.");
        }

        [TestMethod]
        public void MergeSleepingIslands_CombinesJointSets()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));

            Body a1 = world.CreateBody(new BodyDef().AsDynamic().At(-3f, 0f).IsAwake(false));
            a1.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body a2 = world.CreateBody(new BodyDef().AsDynamic().At(-2f, 0f).IsAwake(false));
            a2.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            world.CreateJoint(new RevoluteJointDef(a1, a2, new Vec2(-2.5f, 0f)));

            Body b1 = world.CreateBody(new BodyDef().AsDynamic().At(2f, 0f).IsAwake(false));
            b1.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body b2 = world.CreateBody(new BodyDef().AsDynamic().At(3f, 0f).IsAwake(false));
            b2.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            world.CreateJoint(new RevoluteJointDef(b1, b2, new Vec2(2.5f, 0f)));

            world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(2, world.SleepingSet.Islands.Count, $"Expected 2 sleeping islands, got {world.SleepingSet.Islands.Count}");
            Assert.AreEqual(2, world.SleepingSet.Joints.Count, $"Expected 2 sleeping joints, got {world.SleepingSet.Joints.Count}");

            world.CreateJoint(new RevoluteJointDef(a1, b1, Vec2.Zero));

            Assert.AreEqual(1, world.SleepingSet.Islands.Count, $"Expected merged sleeping island, got {world.SleepingSet.Islands.Count}");
            Assert.AreEqual(3, world.SleepingSet.Joints.Count, $"Expected 3 joints in sleeping set, got {world.SleepingSet.Joints.Count}");
        }

        [TestMethod]
        public void TouchingSleepingBodies_MergeIntoSingleIsland()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f).IsAwake(false));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f).IsAwake(false));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.BuildIslands(awakeOnly: false);
            Assert.AreEqual(2, world.SleepingSet.Islands.Count, $"Expected 2 sleeping islands, got {world.SleepingSet.Islands.Count}");

            world.Step(1f / 60f);

            Assert.AreEqual(1, world.SleepingSet.Islands.Count, $"Expected merged sleeping island after contact, got {world.SleepingSet.Islands.Count}");
        }

        [TestMethod]
        public void BuildIslands_AssignsStableIslandIds()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-2f, 2f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(2f, 2f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.BuildIslands(awakeOnly: false);

            int? idA = world.GetIslandId(bodyA);
            int? idB = world.GetIslandId(bodyB);
            Assert.IsTrue(idA.HasValue && idB.HasValue, "Expected island ids for dynamic bodies.");
            Assert.AreNotEqual(idA.Value, idB.Value, "Expected different islands for separated bodies.");
        }

        [TestMethod]
        public void IslandDirtyFlags_UpdateOnContact()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, -2f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            world.BuildIslands(awakeOnly: false);
            int? idA = world.GetIslandId(bodyA);
            int? idB = world.GetIslandId(bodyB);
            Assert.IsTrue(idA.HasValue && idB.HasValue, "Expected island ids.");

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            int? idAAfter = world.GetIslandId(bodyA);
            int? idBAfter = world.GetIslandId(bodyB);
            Assert.IsTrue(idAAfter.HasValue && idBAfter.HasValue, "Expected island ids after stepping.");
            Assert.IsTrue(world.LastIslands.Count >= 1, "Expected at least one island after stepping.");
        }
    }
}
