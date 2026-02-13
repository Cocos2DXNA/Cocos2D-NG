using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class SolverPipelineTests
    {
        [TestMethod]
        public void SolverPipeline_IsDeterministicForSimpleFall()
        {
            World worldA = CreateWorld();
            World worldB = CreateWorld();

            for (int i = 0; i < 120; ++i)
            {
                worldA.Step(1f / 60f);
                worldB.Step(1f / 60f);
            }

            Body bodyA = worldA.Bodies[1];
            Body bodyB = worldB.Bodies[1];

            float dx = MathF.Abs(bodyA.Transform.P.X - bodyB.Transform.P.X);
            float dy = MathF.Abs(bodyA.Transform.P.Y - bodyB.Transform.P.Y);
            float da = MathF.Abs(bodyA.Transform.Q.Angle - bodyB.Transform.Q.Angle);

            Assert.IsTrue(dx < 1e-5f && dy < 1e-5f && da < 1e-5f,
                $"Expected deterministic results, got dx={dx} dy={dy} da={da}");
        }

        private static World CreateWorld()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));
            world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 10f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            return world;
        }
    }
}
