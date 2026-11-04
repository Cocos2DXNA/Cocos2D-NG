using Box2DNG.Viewer.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class BulletTestSampleModeTests
    {
        [TestMethod]
        public void BulletTest_RandomlyUsesBreakableThreePieceTarget_AndBreaks()
        {
            BulletTestSample sample = new BulletTestSample();
            World world = new World(sample.CreateWorldDef());
            sample.Build(world);

            Body? target = null;
            for (int i = 0; i < world.Bodies.Count; ++i)
            {
                Body body = world.Bodies[i];
                if (body.Type == BodyType.Dynamic && MathF.Abs(body.Transform.P.Y - 4f) < 0.01f)
                {
                    target = body;
                    break;
                }
            }

            Assert.IsNotNull(target, "Expected target plank body.");

            bool sawThreePieceMode = false;
            bool sawBreakToSingle = false;

            for (int frame = 0; frame < 1200; ++frame)
            {
                sample.Step(world, 1f / 60f);
                world.Step(1f / 60f);

                int fixtureCount = target!.Fixtures.Count;
                if (fixtureCount == 3)
                {
                    sawThreePieceMode = true;
                }
                else if (sawThreePieceMode && fixtureCount == 1)
                {
                    sawBreakToSingle = true;
                    break;
                }
            }

            Assert.IsTrue(sawThreePieceMode, "Expected random launch to enable 3-piece breakable target.");
            Assert.IsTrue(sawBreakToSingle, "Expected breakable target to break down to a single center piece on impact.");
        }
    }
}
