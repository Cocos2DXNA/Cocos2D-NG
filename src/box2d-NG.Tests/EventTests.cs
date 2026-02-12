using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class EventTests
    {
        [TestMethod]
        public void ContactBeginAndEndEvents_Fire()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0.9f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithUserData("A"));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithUserData("B"));

            List<ContactBeginEvent> begins = new List<ContactBeginEvent>();
            List<ContactEndEvent> ends = new List<ContactEndEvent>();
            world.Events.ContactEvents += e =>
            {
                if (e.Begin != null) begins.AddRange(e.Begin);
                if (e.End != null) ends.AddRange(e.End);
            };

            world.UpdateContacts();
            Assert.IsTrue(begins.Count > 0, "Expected contact begin event.");

            bodyB.SetTransform(new Vec2(5f, 0f), 0f);
            world.UpdateContacts();
            Assert.IsTrue(ends.Count > 0, "Expected contact end event.");
        }

        [TestMethod]
        public void SensorBeginEndEvents_Fire()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).AsSensor().WithUserData("Sensor"));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithUserData("Body"));

            List<SensorBeginEvent> begins = new List<SensorBeginEvent>();
            List<SensorEndEvent> ends = new List<SensorEndEvent>();
            world.Events.SensorEvents += e =>
            {
                if (e.Begin != null) begins.AddRange(e.Begin);
                if (e.End != null) ends.AddRange(e.End);
            };

            world.UpdateContacts();
            Assert.IsTrue(begins.Count > 0, "Expected sensor begin event.");

            bodyB.SetTransform(new Vec2(5f, 0f), 0f);
            world.UpdateContacts();
            Assert.IsTrue(ends.Count > 0, "Expected sensor end event.");
        }

        [TestMethod]
        public void SensorEventsRespectEnableSensorEventsFlag()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).AsSensor().WithSensorEvents(false).WithUserData("Sensor"));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithUserData("Body"));

            int beginCount = 0;
            int endCount = 0;
            world.Events.SensorEvents += e =>
            {
                if (e.Begin != null) beginCount += e.Begin.Length;
                if (e.End != null) endCount += e.End.Length;
            };

            world.UpdateContacts();
            Assert.AreEqual(0, beginCount, "Sensor events should be suppressed when sensor events are disabled.");

            bodyA.Fixtures[0].SetSensorEventsEnabled(true);
            world.UpdateContacts();
            Assert.IsTrue(beginCount > 0, "Expected sensor begin events after enabling sensor events.");

            bodyA.Fixtures[0].SetSensorEventsEnabled(false);
            world.UpdateContacts();
            Assert.IsTrue(endCount > 0, "Expected sensor end events after disabling sensor events.");
        }

        [TestMethod]
        public void SensorEventsRequireVisitorEnableSensorEvents()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.5f)).AsSensor().WithUserData("Sensor"));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithSensorEvents(false).WithUserData("Body"));

            int beginCount = 0;
            world.Events.SensorEvents += e =>
            {
                if (e.Begin != null) beginCount += e.Begin.Length;
            };

            world.UpdateContacts();
            Assert.AreEqual(0, beginCount, "Expected no sensor begin events when visitor sensor events are disabled.");

            bodyB.Fixtures[0].SetSensorEventsEnabled(true);
            world.UpdateContacts();
            Assert.IsTrue(beginCount > 0, "Expected sensor begin events after enabling visitor sensor events.");
        }

        [TestMethod]
        public void SensorEvents_VisitorFixtureReplacement_EmitsEndThenBegin()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero));
            Body sensorBody = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body visitorBody = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            sensorBody.CreateFixture(new FixtureDef(new CircleShape(0.75f)).AsSensor().WithUserData("Sensor"));
            Fixture visitor = visitorBody.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithUserData("VisitorA"));

            int beginCount = 0;
            int endCount = 0;
            world.Events.SensorEvents += e =>
            {
                if (e.Begin != null) beginCount += e.Begin.Length;
                if (e.End != null) endCount += e.End.Length;
            };

            world.UpdateContacts();
            Assert.AreEqual(1, beginCount, "Expected initial begin event.");
            Assert.AreEqual(0, endCount, "Expected no initial end event.");

            visitorBody.DestroyFixture(visitor);
            visitorBody.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithUserData("VisitorB"));
            world.UpdateContacts();

            Assert.IsTrue(endCount >= 1, "Expected end event for removed visitor overlap.");
            Assert.IsTrue(beginCount >= 2, "Expected begin event for replacement visitor overlap.");
        }

        [TestMethod]
        public void SensorHitEvents_FireForBullets()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body sensorBody = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            sensorBody.CreateFixture(new FixtureDef(new CircleShape(0.5f)).AsSensor().WithUserData("Sensor"));

            Body bullet = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 0f).IsBullet(true));
            bullet.CreateFixture(new FixtureDef(new CircleShape(0.25f)).WithUserData("Bullet"));
            bullet.LinearVelocity = new Vec2(30f, 0f);

            int hitCount = 0;
            world.Events.SensorHitEvents += e =>
            {
                if (e.Events != null)
                {
                    hitCount += e.Events.Length;
                }
            };

            for (int i = 0; i < 30; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(hitCount > 0, "Expected sensor hit events for bullet crossing sensor.");
        }

        [TestMethod]
        public void SensorHitEvents_FireForNonBulletContinuousCasts()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body sensorBody = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            sensorBody.CreateFixture(new FixtureDef(new CircleShape(0.5f)).AsSensor().WithUserData("Sensor"));

            Body mover = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 0f).IsBullet(false));
            mover.CreateFixture(new FixtureDef(new CircleShape(0.25f)).WithUserData("Mover"));
            mover.LinearVelocity = new Vec2(30f, 0f);

            int hitCount = 0;
            world.Events.SensorHitEvents += e =>
            {
                if (e.Events != null)
                {
                    hitCount += e.Events.Length;
                }
            };

            for (int i = 0; i < 30; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(hitCount > 0, "Expected sensor hit events for non-bullet continuous cast.");
        }

        [TestMethod]
        public void SensorHitEvents_FireForContinuousCasts_AllDynamicShapes()
        {
            bool circleHit = RunContinuousSensorHitScenario(new CircleShape(0.25f), "Circle");
            bool capsuleHit = RunContinuousSensorHitScenario(new CapsuleShape(new Vec2(-0.2f, 0f), new Vec2(0.2f, 0f), 0.15f), "Capsule");
            bool polygonHit = RunContinuousSensorHitScenario(new PolygonShape(new[]
            {
                new Vec2(-0.2f, -0.2f),
                new Vec2(0.2f, -0.2f),
                new Vec2(0.2f, 0.2f),
                new Vec2(-0.2f, 0.2f)
            }), "Polygon");

            Assert.IsTrue(circleHit, "Expected circle visitor to generate sensor hit events.");
            Assert.IsTrue(capsuleHit, "Expected capsule visitor to generate sensor hit events.");
            Assert.IsTrue(polygonHit, "Expected polygon visitor to generate sensor hit events.");
        }

        private static bool RunContinuousSensorHitScenario(Shape shape, string visitorLabel)
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero));

            Body sensorBody = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            sensorBody.CreateFixture(new FixtureDef(new CircleShape(0.5f)).AsSensor().WithUserData("Sensor"));

            Body mover = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 0f).IsBullet(false));
            mover.CreateFixture(new FixtureDef(shape).WithDensity(1f).WithUserData(visitorLabel));
            mover.LinearVelocity = new Vec2(30f, 0f);

            bool hit = false;
            world.Events.SensorHitEvents += e =>
            {
                if (e.Events == null)
                {
                    return;
                }

                for (int i = 0; i < e.Events.Length; ++i)
                {
                    if ((string?)e.Events[i].VisitorUserData == visitorLabel)
                    {
                        hit = true;
                        return;
                    }
                }
            };

            for (int i = 0; i < 30 && !hit; ++i)
            {
                world.Step(1f / 60f);
            }

            return hit;
        }
    }
}
