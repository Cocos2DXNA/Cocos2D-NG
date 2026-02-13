using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class CarSampleTests
    {
        [TestMethod]
        public void Car_RestsWithoutExcessiveBounce()
        {
            (World world, Body car, WheelJoint motorJoint) = BuildCarWorld();
            motorJoint.SetMotorSpeed(0f);

            for (int i = 0; i < 600; ++i)
            {
                world.Step(1f / 60f);
            }

            System.Console.WriteLine($"Car rest: pos={car.Transform.P} v={car.LinearVelocity} w={car.AngularVelocity} contacts={world.Contacts.Count}");
            Assert.IsTrue(MathF.Abs(car.LinearVelocity.Y) < 0.5f, "Car vertical velocity should dampen.");
            Assert.IsTrue(MathF.Abs(car.AngularVelocity) < 1.0f, "Car angular velocity should dampen.");
            Assert.IsTrue(car.Transform.P.Y > 0.2f && car.Transform.P.Y < 2.0f, "Car should settle near the ground.");
        }

        [TestMethod]
        public void Car_DrivesForward()
        {
            (World world, Body car, WheelJoint motorJoint) = BuildCarWorld();
            float startX = car.Transform.P.X;

            motorJoint.SetMotorSpeed(-100f);
            for (int i = 0; i < 600; ++i)
            {
                world.Step(1f / 60f);
            }

            System.Console.WriteLine($"Car forward: pos={car.Transform.P} v={car.LinearVelocity} contacts={world.Contacts.Count}");
            Assert.IsTrue(car.Transform.P.X > startX + 0.05f, "Car should move forward under motor drive.");
        }

        [TestMethod]
        public void Car_DrivesBackward()
        {
            (World world, Body car, WheelJoint motorJoint) = BuildCarWorld();
            float startX = car.Transform.P.X;

            motorJoint.SetMotorSpeed(100f);
            for (int i = 0; i < 600; ++i)
            {
                world.Step(1f / 60f);
            }

            System.Console.WriteLine($"Car backward: pos={car.Transform.P} v={car.LinearVelocity} contacts={world.Contacts.Count}");
            Assert.IsTrue(car.Transform.P.X < startX - 0.05f, "Car should move backward under reverse motor drive.");
        }

        [TestMethod]
        public void Car_SlowsWhenMotorOff()
        {
            (World world, Body car, WheelJoint motorJoint) = BuildCarWorld();

            motorJoint.SetMotorSpeed(-100f);
            for (int i = 0; i < 600; ++i)
            {
                world.Step(1f / 60f);
            }

            float speedBefore = MathF.Abs(car.LinearVelocity.X);

            motorJoint.SetMotorSpeed(0f);
            for (int i = 0; i < 600; ++i)
            {
                world.Step(1f / 60f);
            }

            float speedAfter = MathF.Abs(car.LinearVelocity.X);
            System.Console.WriteLine($"Car slowdown: before={speedBefore} after={speedAfter} v={car.LinearVelocity} contacts={world.Contacts.Count}");
            float target = MathF.Max(speedBefore * 0.9f, 0.05f);
            Assert.IsTrue(speedAfter < target, "Car should slow down when motor is off.");
        }

        private static (World world, Body car, WheelJoint motorJoint) BuildCarWorld()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-200f, 0f), new Vec2(200f, 0f)))
                .WithDensity(0f)
                .WithFriction(0.6f));

            Filter carFilter = new Filter(1, ulong.MaxValue, -1);
            PolygonShape chassis = new PolygonShape(new[]
            {
                new Vec2(-1.5f, -0.5f),
                new Vec2(1.5f, -0.5f),
                new Vec2(1.5f, 0f),
                new Vec2(0f, 0.9f),
                new Vec2(-1.15f, 0.9f),
                new Vec2(-1.5f, 0.2f)
            });
            CircleShape wheel = new CircleShape(0.4f);

            Body car = world.CreateBody(new BodyDef()
                .AsDynamic()
                .At(0f, 1f)
                .WithLinearDamping(0.6f)
                .WithAngularDamping(0.2f));
            car.CreateFixture(new FixtureDef(chassis).WithDensity(1f).WithFilter(carFilter));

            FixtureDef wheelDef = new FixtureDef(wheel).WithDensity(1f).WithFriction(0.9f).WithFilter(carFilter);
            Body wheel1 = world.CreateBody(new BodyDef()
                .AsDynamic()
                .At(-1f, 0.35f)
                .WithLinearDamping(0.6f)
                .WithAngularDamping(0.2f));
            wheel1.CreateFixture(wheelDef);
            Body wheel2 = world.CreateBody(new BodyDef()
                .AsDynamic()
                .At(1f, 0.4f)
                .WithLinearDamping(0.6f)
                .WithAngularDamping(0.2f));
            wheel2.CreateFixture(wheelDef);

            Vec2 axis = new Vec2(0f, 1f);
            WheelJointDef jd = new WheelJointDef(car, wheel1, wheel1.Transform.P, axis)
                .WithMotor(true, 0f, 400f)
                .WithSpring(12f, 1.0f)
                .WithLimits(-0.3f, 0.3f);
            WheelJoint motorJoint = world.CreateJoint(jd);

            jd = new WheelJointDef(car, wheel2, wheel2.Transform.P, axis)
                .WithMotor(false, 0f, 200f)
                .WithSpring(12f, 1.0f)
                .WithLimits(-0.3f, 0.3f);
            world.CreateJoint(jd);

            return (world, car, motorJoint);
        }
    }
}
