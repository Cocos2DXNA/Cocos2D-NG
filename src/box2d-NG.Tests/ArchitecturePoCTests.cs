using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Box2DNG.Tests
{
    [TestClass]
    public class ArchitecturePoCTests
    {
        [TestMethod]
        public void BodyCreation_AssignsContiguousIds()
        {
            World world = new World(new WorldDef());
            
            Body b0 = world.CreateBody(new BodyDef());
            Body b1 = world.CreateBody(new BodyDef());
            Body b2 = world.CreateBody(new BodyDef());
            
            Assert.AreEqual(0, b0.Id, "b0 should have ID 0");
            Assert.AreEqual(1, b1.Id, "b1 should have ID 1");
            Assert.AreEqual(2, b2.Id, "b2 should have ID 2");
        }

        [TestMethod]
        public void DestroyBody_MaintainsContiguousIds_ViaSwapRemove()
        {
            World world = new World(new WorldDef());
            
            Body b0 = world.CreateBody(new BodyDef());
            Body b1 = world.CreateBody(new BodyDef());
            Body b2 = world.CreateBody(new BodyDef()); 
            
            // Destroy middle one
            world.DestroyBody(b1);
            
            // Expected: b2 (last) is swapped into b1's slot (1)
            Assert.AreEqual(1, b2.Id, "b2 should satisfy swap-remove invariant");
            Assert.AreEqual(2, world.Bodies.Count);
            
            // b1 should be invalid
            Assert.AreEqual(-1, b1.Id);
            
            // b0 unchanged
            Assert.AreEqual(0, b0.Id);
        }
        
        [TestMethod]
        public void DestroyBody_MovesDataCorrectly()
        {
             World world = new World(new WorldDef());
             Body b0 = world.CreateBody(new BodyDef().At(0,0));
             Body b1 = world.CreateBody(new BodyDef().At(10,10));
             Body b2 = world.CreateBody(new BodyDef().At(20,20));
             
             // Destroy b1
             world.DestroyBody(b1);
             
             // b2 moves to index 1.
             // We ensure that the data was copied from index 2 to index 1
             // and that the b2 wrapper now points to index 1.
             
             Assert.AreEqual(20f, b2.Transform.P.X);
             Assert.AreEqual(20f, b2.Transform.P.Y);
             
             // Verify the world array effectively has the data at the new index
             // Indirect verification via another body created at the old index?
             // No, old index is gone (count reduced).
             
             // Verify calling property works
             b2.LinearVelocity = new Vec2(5, 5);
             Assert.AreEqual(5f, b2.LinearVelocity.X);
        }

        [TestMethod]
        public void Test_CreateBody_Simple()
        {
             World world = new World(new WorldDef());
             Body b = world.CreateBody(new BodyDef());
             Assert.AreEqual(0, b.Id);
        }

        [TestMethod]
        public void Test_CreateBody_WithDamping()
        {
             World world = new World(new WorldDef());
             Body b = world.CreateBody(new BodyDef().WithLinearDamping(1.5f));
             Assert.AreEqual(1.5f, b.LinearDamping);
        }

        [TestMethod]
        public void Test_CreateBody_WithSolverSetType()
        {
             World world = new World(new WorldDef());
             Body b = world.CreateBody(new BodyDef().AsDynamic().IsAwake(true));
             // Should be Awake
             Assert.AreEqual(SolverSetType.Awake, b.SolverSetType);
        }

        [TestMethod]
        public void Test_DestroyBody_Swap()
        {
            World world = new World(new WorldDef());
            
            Body b0 = world.CreateBody(new BodyDef()); // Id 0
            Body b1 = world.CreateBody(new BodyDef()); // Id 1
            
            BodyDef def2 = new BodyDef()
                .AsDynamic()
                .WithLinearDamping(1.5f)
                .WithAngularDamping(2.5f)
                .WithGravityScale(3.5f)
                .IsAwake(true)
                .AllowSleeping(false);
            
            Body b2 = world.CreateBody(def2); // Id 2

            // Destroy b1 (Id 1)
            world.DestroyBody(b1);

            // b2 should move to Id 1
            Assert.AreEqual(1, b2.Id);
            
            // Verify properties
            Assert.AreEqual(1.5f, b2.LinearDamping);
            Assert.AreEqual(2.5f, b2.AngularDamping);
            Assert.AreEqual(3.5f, b2.GravityScale);
            Assert.AreEqual(true, b2.Awake);
            Assert.AreEqual(false, b2.AllowSleep);
        }

        
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void AccessingDestroyedBody_ThrowsException()
        {
            World world = new World(new WorldDef());
            Body b0 = world.CreateBody(new BodyDef());
            world.DestroyBody(b0);
            
            // Should throw because Id is -1
            var p = b0.Transform.P;
        }
    }
}
