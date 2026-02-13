using Box2D;
using Box2D.Dynamics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DTests
{
    [TestClass]
    public class Box2DScenarioTests
    {
        [TestMethod]
        public void StackOfBoxesGetsPushed()
        {
            b2World world = Box2DScenarios.CreateStackOfBoxes(out b2Body topBox, out b2Body pusher);
            float startX = topBox.Position.x;
            float startAngle = topBox.Angle;

            StepWorld(world, 240);

            float endX = topBox.Position.x;
            float endAngle = topBox.Angle;
            bool moved = System.Math.Abs(endX - startX) > 0.2f || System.Math.Abs(endAngle - startAngle) > 0.1f;
            Assert.IsTrue(moved, "Top box should move or rotate when the stack is pushed.");
            Assert.IsTrue(pusher.Position.x > -4.0f, "Pusher should advance into the stack.");
        }

        [TestMethod]
        public void RollerCoasterMovesForward()
        {
            b2World world = Box2DScenarios.CreateRollerCoaster(out b2Body cart);
            float startX = cart.Position.x;

            StepWorld(world, 300);

            Assert.IsTrue(!float.IsNaN(cart.Position.x), "Cart X should stay valid.");
            Assert.IsTrue(cart.Position.x > startX + 4.0f, "Cart should move forward along the track.");
        }

        [TestMethod]
        public void ProjectileRisesThenFalls()
        {
            b2World world = Box2DScenarios.CreateProjectile(out b2Body projectile);
            float startY = projectile.Position.y;

            StepWorld(world, 30);
            float peakY = projectile.Position.y;

            StepWorld(world, 180);
            float endY = projectile.Position.y;

            Assert.IsTrue(peakY > startY + 0.5f, "Projectile should rise after launch.");
            Assert.IsTrue(endY < peakY - 0.5f, "Projectile should fall after reaching a peak.");
        }

        private static void StepWorld(b2World world, int steps)
        {
            for (int i = 0; i < steps; ++i)
            {
                world.Step(1.0f / 60.0f, 8, 3);
            }
        }
    }
}
