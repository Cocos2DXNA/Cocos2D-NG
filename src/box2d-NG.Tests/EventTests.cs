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
    }
}
