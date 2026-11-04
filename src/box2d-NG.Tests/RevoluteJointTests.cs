using Box2DNG;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Box2DNG.Tests.Integration
{
    [TestClass]
    public class RevoluteJointTests
    {
        [TestMethod]
        public void RevoluteJoint_MaintainsLimits()
        {
             World world = new World(new WorldDef());
             Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
             Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(10f, 0f));
             
             RevoluteJointDef def = new RevoluteJointDef(bodyA, bodyB, new Vec2(0f, 0f))
                .WithLimit(-0.25f * MathF.PI, 0.25f * MathF.PI)
                .WithMotor(true, 1.0f, 1000f);
             
             RevoluteJoint joint = world.CreateJoint(def);
             
             Assert.IsNotNull(joint);
             Assert.AreEqual(0, joint.Index);
             Assert.IsTrue(joint.EnableLimit);
             Assert.IsTrue(joint.EnableMotor);
             
             // Step
             for(int i=0; i<60; ++i) world.Step(1/60f);
             
             // Check angle
             Body bA = bodyA;
             Body bB = bodyB;
             float angle = bB.Rotation.Angle - bA.Rotation.Angle - joint.ReferenceAngle;
             // joint.GetJointAngle() was defined in my handle logic?
             // Yes.
             float jointAngle = joint.GetJointAngle();
             
             // Assertions
             // Basic check: Angle should stay within limits or be pushed by motor?
             // Motor wants speed 1.0. Lower limit is -45 deg, Upper is +45.
             // Starts at 0. Motor pushes positive? 
             // 1.0 rad/s * 1s = 1 rad ~ 57 deg. Should hit upper limit (45 deg = 0.785 rad).
             
             Assert.IsTrue(jointAngle <= joint.UpperAngle + 0.1f, $"Angle {jointAngle} exceeded upper limit {joint.UpperAngle}");
             Assert.IsTrue(jointAngle >= joint.LowerAngle - 0.1f, $"Angle {jointAngle} exceeded lower limit {joint.LowerAngle}");
             
             world.DestroyJoint(joint);
             Assert.AreEqual(-1, joint.Index);
        }
    }
}
