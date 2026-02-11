using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class UtilitiesTests
    {
        [TestMethod]
        public void BodyId_StableAcrossSteps()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            int id = body.Id;
            Assert.IsTrue(id >= 0, "Expected body id to be assigned.");

            for (int i = 0; i < 5; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.AreEqual(id, body.Id, "Expected body id to remain stable across steps.");
        }

        [TestMethod]
        public void FixtureId_StableAcrossStepsAndReusedAfterDestroy()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f));
            Fixture fixture = body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            int id = fixture.Id;
            Assert.IsTrue(id >= 0, "Expected fixture id to be assigned.");

            for (int i = 0; i < 3; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.AreEqual(id, fixture.Id, "Expected fixture id to remain stable across steps.");

            body.DestroyFixture(fixture);
            Fixture replacement = body.CreateFixture(new FixtureDef(new CircleShape(0.25f)).WithDensity(1f));
            Assert.AreEqual(id, replacement.Id, "Expected fixture id to be reused after destroy.");
        }

        [TestMethod]
        public void JointId_ReusedAfterDestroy()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-1f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            RevoluteJoint joint = world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, Vec2.Zero));
            int id = joint.Id;
            Assert.IsTrue(id >= 0, "Expected joint id to be assigned.");

            bool removed = world.DestroyJoint(joint);
            Assert.IsTrue(removed, "Expected joint removal to succeed.");

            RevoluteJoint replacement = world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, Vec2.Zero));
            Assert.AreEqual(id, replacement.Id, "Expected joint id to be reused after destroy.");
        }

        [TestMethod]
        public void JointHandle_StableAfterOtherJointRemoval()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-2f, 1f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 1f));
            Body bodyC = world.CreateBody(new BodyDef().AsDynamic().At(2f, 1f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            bodyC.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            RevoluteJoint revolute = world.CreateJoint(new RevoluteJointDef(bodyA, bodyB, new Vec2(-1f, 1f)));
            PrismaticJoint prismatic = world.CreateJoint(new PrismaticJointDef(bodyB, bodyC, new Vec2(1f, 1f), new Vec2(1f, 0f)));
            int prismaticId = prismatic.Id;

            world.Step(1f / 60f);

            bool removed = world.DestroyJoint(revolute);
            Assert.IsTrue(removed, "Expected revolute joint removal to succeed.");

            world.Step(1f / 60f);
            Assert.AreEqual(prismaticId, prismatic.Id, "Expected prismatic joint id to remain stable after other joint removal.");
        }

        [TestMethod]
        public void ContactId_ReusedAfterContactDestroyed()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            Fixture fixtureB = bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.Step(1f / 60f);
            Assert.IsTrue(world.Contacts.Count > 0, "Expected contact to exist.");
            int id = world.Contacts[0].Id;
            Assert.IsTrue(id >= 0, "Expected contact id to be assigned.");

            bodyB.DestroyFixture(fixtureB);
            world.Step(1f / 60f);

            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            bodyA.SetAwake(true);
            bodyB.SetAwake(true);
            bodyB.SetTransform(new Vec2(0.5f, 0f), 0f);
            world.Step(1f / 60f);

            Assert.IsTrue(world.Contacts.Count > 0, "Expected contact after fixture recreation.");
            Assert.AreEqual(id, world.Contacts[0].Id, "Expected contact id to be reused.");
        }

        [TestMethod]
        public void ContactEdgeIds_ReusedAfterContactDestroyed()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            Fixture fixtureB = bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.Step(1f / 60f);
            Assert.IsTrue(world.Contacts.Count > 0, "Expected contact to exist.");
            Contact contact = world.Contacts[0];
            int edgeA = contact.EdgeIdA;
            int edgeB = contact.EdgeIdB;
            Assert.IsTrue(edgeA >= 0 && edgeB >= 0, "Expected contact edge ids to be assigned.");
            Assert.IsNotNull(bodyA.ContactList, "Expected bodyA contact list head.");
            Assert.IsNotNull(bodyB.ContactList, "Expected bodyB contact list head.");
            Assert.AreEqual(contact, bodyA.ContactList!.Contact, "Expected bodyA contact edge to reference contact.");
            Assert.AreEqual(contact, bodyB.ContactList!.Contact, "Expected bodyB contact edge to reference contact.");

            bodyB.DestroyFixture(fixtureB);
            world.Step(1f / 60f);

            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            bodyA.SetAwake(true);
            bodyB.SetAwake(true);
            bodyB.SetTransform(new Vec2(0.5f, 0f), 0f);
            world.Step(1f / 60f);

            Assert.IsTrue(world.Contacts.Count > 0, "Expected contact after fixture recreation.");
            Contact next = world.Contacts[0];
            int[] expected = new[] { edgeA, edgeB };
            Array.Sort(expected);
            int[] actual = new[] { next.EdgeIdA, next.EdgeIdB };
            Array.Sort(actual);
            Assert.AreEqual(expected[0], actual[0], "Expected edge id to be reused.");
            Assert.AreEqual(expected[1], actual[1], "Expected edge id to be reused.");
            Assert.IsNotNull(bodyA.ContactList, "Expected bodyA contact list after recreation.");
            Assert.IsNotNull(bodyB.ContactList, "Expected bodyB contact list after recreation.");
        }

        [TestMethod]
        public void ContactEdgeList_OrdersNewestFirst()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(false));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f));
            Body bodyC = world.CreateBody(new BodyDef().AsDynamic().At(5f, 0f));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            bodyC.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.Step(1f / 60f);
            Assert.IsNotNull(bodyB.ContactList, "Expected initial contact for bodyB.");
            Body firstOther = bodyB.ContactList!.Other;

            bodyC.SetTransform(new Vec2(1.0f, 0f), 0f);
            world.Step(1f / 60f);

            Assert.IsNotNull(bodyB.ContactList, "Expected contact list for bodyB after second contact.");
            ContactEdge head = bodyB.ContactList!;
            Assert.AreSame(bodyC, head.Other, "Expected newest contact edge to be at list head.");
            Assert.IsNotNull(head.Next, "Expected second contact edge in list.");
            Assert.AreSame(firstOther, head.Next!.Other, "Expected previous contact edge to follow newest.");
        }

        [TestMethod]
        public void ProxyId_ReusedAfterFixtureDestroyed()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(false));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Fixture fixture = body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));
            int proxyId = fixture.ProxyId;

            body.DestroyFixture(fixture);
            Fixture replacement = body.CreateFixture(new FixtureDef(new CircleShape(0.25f)).WithDensity(1f));

            Assert.AreEqual(proxyId, replacement.ProxyId, "Expected proxy id to be reused after destroy.");
        }

        [TestMethod]
        public void SleepingContact_AssignsSolverSetId()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f).IsAwake(false));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f).IsAwake(false));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.Step(1f / 60f);
            Assert.IsFalse(bodyA.Awake, "Expected bodyA to remain sleeping.");
            Assert.IsFalse(bodyB.Awake, "Expected bodyB to remain sleeping.");
            Assert.IsTrue(world.Contacts.Count > 0, "Expected contact while sleeping.");

            Contact contact = world.Contacts[0];
            Assert.AreEqual(SolverSetType.Sleeping, contact.SolverSetType, "Expected contact in sleeping solver set.");
            Assert.IsTrue(contact.SolverSetId != 0, "Expected non-zero solver set id for sleeping contact.");
        }

        [TestMethod]
        public void SolverSetId_ReusedAfterSleepWakeCycle()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero).EnableSleeping(true));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f).IsAwake(false));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.5f, 0f).IsAwake(false));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.75f)).WithDensity(1f));

            world.Step(1f / 60f);
            Contact contact = world.Contacts[0];
            int sleepingSetId = contact.SolverSetId;
            Assert.IsTrue(sleepingSetId != 0, "Expected non-zero sleeping solver set id.");

            bodyA.SetAwake(true);
            world.Step(1f / 60f);
            Assert.AreEqual(SolverSetType.Awake, contact.SolverSetType, "Expected contact to be awake after waking.");

            bodyA.SetAwake(false);
            bodyB.SetAwake(false);
            world.Step(1f / 60f);

            Assert.AreEqual(SolverSetType.Sleeping, contact.SolverSetType, "Expected contact to return to sleeping.");
            Assert.AreEqual(sleepingSetId, contact.SolverSetId, "Expected solver set id to be reused after sleep/wake cycle.");
        }
        [TestMethod]
        public void HashSet64_AddRemoveContains_TracksCount()
        {
            HashSet64 set = new HashSet64(2);

            Assert.IsFalse(set.Add(1), "Expected first add to report new key.");
            Assert.IsTrue(set.Contains(1));
            Assert.AreEqual(1, set.Count);

            Assert.IsTrue(set.Add(1), "Expected second add to report existing key.");
            Assert.AreEqual(1, set.Count);

            for (ulong i = 2; i <= 64; ++i)
            {
                Assert.IsFalse(set.Add(i), $"Expected key {i} to be new.");
            }

            Assert.IsTrue(set.Contains(64));
            Assert.AreEqual(64, set.Count);

            Assert.IsTrue(set.Remove(1));
            Assert.IsFalse(set.Contains(1));
            Assert.AreEqual(63, set.Count);
        }

        [TestMethod]
        public void IdPool_ReusesFreedIds()
        {
            IdPool pool = new IdPool();

            int first = pool.Alloc();
            int second = pool.Alloc();
            int third = pool.Alloc();

            pool.Free(second);
            int reused = pool.Alloc();
            Assert.AreEqual(second, reused);

            Assert.AreEqual(3, pool.Capacity);
            Assert.AreEqual(3, pool.Count);

            pool.Free(first);
            pool.Free(third);
            pool.Free(reused);

            Assert.AreEqual(0, pool.Count);
        }
    }
}
