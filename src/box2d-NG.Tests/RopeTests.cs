using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class RopeTests
    {
        [TestMethod]
        public void Rope_MaintainsSegmentLengths()
        {
            Vec2[] vertices =
            {
                new Vec2(0f, 0f),
                new Vec2(1f, 0f),
                new Vec2(2f, 0f),
                new Vec2(3f, 0f)
            };
            float[] masses = { 0f, 1f, 1f, 0f };

            RopeDef def = RopeDef.CreateDefault(vertices, masses);
            def.Gravity = new Vec2(0f, -10f);
            def.Damping = 0.1f;
            def.K2 = 1f;
            def.K3 = 0.1f;

            Rope rope = new Rope();
            rope.Initialize(def);

            for (int i = 0; i < 60; ++i)
            {
                rope.Step(1f / 60f, 10);
            }

            Vec2[] positions = rope.Positions;
            for (int i = 0; i < positions.Length - 1; ++i)
            {
                float len = (positions[i + 1] - positions[i]).Length;
                Assert.IsTrue(MathF.Abs(len - 1f) < 0.2f, $"Rope segment length drifted: {len}");
            }
        }
    }
}
