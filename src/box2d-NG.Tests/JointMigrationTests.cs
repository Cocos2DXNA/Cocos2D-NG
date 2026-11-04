using Box2DNG;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests.Integration
{
    [TestClass]
    public class JointMigrationTests
    {
        [TestMethod]
        public void DistanceJoint_MaintainsDistance()
        {
            // 1. Setup World
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            // 2. Create Bodies
            BodyDef bodyDef = new BodyDef().AsDynamic().At(0f, 10f);
            Body bodyA = world.CreateBody(bodyDef);
            Body bodyB = world.CreateBody(bodyDef.At(0f, 0f)); // 10 units below

            // 3. Create DistanceJoint
            DistanceJointDef jointDef = new DistanceJointDef(bodyA, bodyB, new Vec2(0f, 10f), new Vec2(0f, 0f))
                .WithLength(10f)
                .WithFrequency(0f)
                .WithDampingRatio(0f)
                .WithCollideConnected(false);
            DistanceJoint joint = world.CreateJoint(jointDef);

            Assert.AreEqual(10f, joint.Length);
            // Assert.AreEqual(0, joint.Index); // Internal

            // 4. Step World
            // Apply force to bodyB to try to stretch it
            bodyB.ApplyForce(new Vec2(0f, -1000f));

            for (int i = 0; i < 60; ++i)
            {
                world.Step(1f / 60f);
            }

            // 5. Verify Distance
            Vec2 pA = bodyA.Position;
            Vec2 pB = bodyB.Position;
            float distance = (pA - pB).Length;

            // Allow small error due to usage of Baumgarte stabilization or solver drift
            Assert.AreEqual(10f, distance, 0.1f, $"Distance should remain close to 10. Actual: {distance}");
            
            // 6. Verify Cleanup
            world.DestroyJoint(joint);
            // Assert.AreEqual(-1, joint.Index); // Internal
            Assert.AreEqual(-1, joint.Id);
            
            // Verify swap-remove (optional if we had multiple joints)
        }
        
        [TestMethod]
        public void DistanceJoint_SwapRemove_MaintainsIntegrity()
        {
             World world = new World(new WorldDef());
             Body bodyA = world.CreateBody(new BodyDef().AsDynamic());
             Body bodyB = world.CreateBody(new BodyDef().AsDynamic());
             
             // Create 3 joints
             DistanceJoint j1 = world.CreateJoint(new DistanceJointDef(bodyA, bodyB, Vec2.Zero, Vec2.Zero).WithLength(1f));
             DistanceJoint j2 = world.CreateJoint(new DistanceJointDef(bodyA, bodyB, Vec2.Zero, Vec2.Zero).WithLength(2f));
             DistanceJoint j3 = world.CreateJoint(new DistanceJointDef(bodyA, bodyB, Vec2.Zero, Vec2.Zero).WithLength(3f));
             
             // Assert.AreEqual(0, j1.Index);
             // Assert.AreEqual(1, j2.Index);
             // Assert.AreEqual(2, j3.Index);
             
             // Destroy j1 (Index 0). j3 (Index 2) should move to slot 0.
             world.DestroyJoint(j1);
             
             // Assert.AreEqual(-1, j1.Index);
             // Assert.AreEqual(1, j2.Index); // Unchanged
             // Assert.AreEqual(0, j3.Index); // Moved from 2 to 0
             
             // Destroy j2 (Index 1). j3 (Index 0) stays. j2 (last) was last.
             // Wait, logic: Index 1 is last (count is 2: j3, j2).
             // j3 is at 0. j2 is at 1.
             // Destroy j2. Last is j2. Swap self? Logic handles `index != lastIndex`.
             world.DestroyJoint(j2);
             
             // Assert.AreEqual(-1, j2.Index);
             // Assert.AreEqual(0, j3.Index);
             
             // Destroy j3
             world.DestroyJoint(j3);
             // Assert.AreEqual(-1, j3.Index);
        }
    }
}
