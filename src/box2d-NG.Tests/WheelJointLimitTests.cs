using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class WheelJointLimitTests
    {
        [TestMethod]
        public void WheelJoint_LimitsClampTranslation()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            WheelJoint joint = world.CreateJoint(new WheelJointDef(ground, body, body.Transform.P, new Vec2(0f, 1f))
                .WithSpring(0f, 0f)
                .WithLimits(-0.5f, 0.5f));

            body.SetTransform(new Vec2(0f, 2f), 0f);
            body.LinearVelocity = Vec2.Zero;
            body.AngularVelocity = 0f;
            StepWorld(world, 120);

            float translationUpper = GetTranslation(joint);
            Assert.IsTrue(translationUpper <= 0.5f + 0.05f, $"Upper limit not enforced. translation={translationUpper}");

            body.SetTransform(new Vec2(0f, -2f), 0f);
            body.LinearVelocity = Vec2.Zero;
            body.AngularVelocity = 0f;
            StepWorld(world, 120);

            float translationLower = GetTranslation(joint);
            Assert.IsTrue(translationLower >= -0.5f - 0.05f, $"Lower limit not enforced. translation={translationLower}");
        }

        [TestMethod]
        public void WheelJoint_LimitsPreventExcessiveVelocity()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            WheelJoint joint = world.CreateJoint(new WheelJointDef(ground, body, body.Transform.P, new Vec2(0f, 1f))
                .WithSpring(0f, 0f)
                .WithLimits(-0.5f, 0.5f));

            body.LinearVelocity = new Vec2(0f, 10f);
            StepWorld(world, 120);

            float translationUpper = GetTranslation(joint);
            Assert.IsTrue(translationUpper <= 0.5f + 0.05f, $"Upper limit not enforced under velocity. translation={translationUpper}");

            body.SetTransform(new Vec2(0f, 0f), 0f);
            body.LinearVelocity = new Vec2(0f, -10f);
            StepWorld(world, 120);

            float translationLower = GetTranslation(joint);
            Assert.IsTrue(translationLower >= -0.5f - 0.05f, $"Lower limit not enforced under velocity. translation={translationLower}");
        }

        [TestMethod]
        public void WheelJoint_LimitsSettleWithoutEnergyInjection()
        {
            World world = new World(new WorldDef().WithGravity(Vec2.Zero));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body body = world.CreateBody(new BodyDef()
                .AsDynamic()
                .At(0f, 0f)
                .WithLinearDamping(1.0f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            WheelJoint joint = world.CreateJoint(new WheelJointDef(ground, body, body.Transform.P, new Vec2(0f, 1f))
                .WithSpring(0f, 0f)
                .WithLimits(-0.5f, 0.5f));

            body.LinearVelocity = new Vec2(0f, 20f);
            StepWorld(world, 360);

            float translation = GetTranslation(joint);
            Assert.IsTrue(translation <= 0.5f + 0.05f, $"Upper limit not enforced during settle. translation={translation}");
            Assert.IsTrue(MathF.Abs(body.LinearVelocity.Y) < 0.15f, $"Velocity should settle near zero. v={body.LinearVelocity}");
        }

        [TestMethod]
        public void WheelJoint_LimitsHoldUnderGravity()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body body = world.CreateBody(new BodyDef()
                .AsDynamic()
                .At(0f, 2f)
                .WithLinearDamping(0.2f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            WheelJoint joint = world.CreateJoint(new WheelJointDef(ground, body, body.Transform.P, new Vec2(0f, 1f))
                .WithSpring(0f, 0f)
                .WithLimits(-0.5f, 0.5f));

            StepWorld(world, 240);

            float translation = GetTranslation(joint);
            Assert.IsTrue(translation >= -0.5f - 0.05f, $"Lower limit not enforced under gravity. translation={translation}");
        }

        private static void StepWorld(World world, int steps)
        {
            for (int i = 0; i < steps; ++i)
            {
                world.Step(1f / 60f);
            }
        }

        private static float GetTranslation(WheelJoint joint)
        {
            Vec2 rA = Rot.Mul(joint.BodyA.Transform.Q, joint.LocalAnchorA - joint.BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(joint.BodyB.Transform.Q, joint.LocalAnchorB - joint.BodyB.LocalCenter);
            Vec2 d = (joint.BodyB.GetWorldCenter() + rB) - (joint.BodyA.GetWorldCenter() + rA);
            Vec2 axis = Rot.Mul(joint.BodyA.Transform.Q, joint.LocalAxisA);
            return Vec2.Dot(axis, d);
        }
    }
}
