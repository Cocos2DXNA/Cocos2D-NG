using Box2DNG.Viewer.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class GearsPulleysSampleTests
    {
        [TestMethod]
        public void GearsSample_BuildsExpectedJointTopologyAndAssemblyLayout()
        {
            GearsSample sample = new GearsSample();
            World world = new World(sample.CreateWorldDef());
            sample.Build(world);

            Assert.AreEqual(3, world.GearJoints.Count, "Expected three gear joints.");
            Assert.AreEqual(4, world.RevoluteJoints.Count, "Expected four revolute joints.");
            Assert.AreEqual(1, world.PrismaticJoints.Count, "Expected one prismatic joint.");

            Body? largeGear = null;
            Body? verticalBar = null;
            for (int i = 0; i < world.Bodies.Count; ++i)
            {
                Body body = world.Bodies[i];
                if (body.Type != BodyType.Dynamic || body.Fixtures.Count != 1)
                {
                    continue;
                }

                Shape shape = body.Fixtures[0].Shape;
                if (shape is CircleShape circle &&
                    MathF.Abs(circle.Radius - 2f) < 1e-4f &&
                    MathF.Abs(body.Transform.P.X - 10f) < 1e-3f &&
                    MathF.Abs(body.Transform.P.Y - 8f) < 1e-3f)
                {
                    largeGear = body;
                }
                else if (shape is PolygonShape polygon &&
                         polygon.Vertices.Count == 4 &&
                         MathF.Abs(body.Transform.P.X - 10f) < 1e-3f &&
                         MathF.Abs(body.Transform.P.Y - 6f) < 1e-3f)
                {
                    verticalBar = body;
                }
            }

            Assert.IsNotNull(largeGear, "Expected dynamic large gear body at (10, 8).");
            Assert.IsNotNull(verticalBar, "Expected dynamic vertical bar body at (10, 6).");

            for (int i = 0; i < 180; ++i)
            {
                sample.Step(world, 1f / 60f);
                world.Step(1f / 60f);
            }

            Assert.IsFalse(float.IsNaN(largeGear!.Transform.P.X) || float.IsNaN(largeGear.Transform.P.Y), "Expected finite large gear transform.");
            Assert.IsFalse(float.IsNaN(verticalBar!.Transform.P.X) || float.IsNaN(verticalBar.Transform.P.Y), "Expected finite bar transform.");
            Assert.IsTrue(MathF.Abs(world.PrismaticJoints[0].GetTranslation()) <= 5.25f, "Expected prismatic translation to stay near configured limits.");
        }

        [TestMethod]
        public void PulleysSample_PreservesRopeLengthConstraint()
        {
            PulleysSample sample = new PulleysSample();
            World world = new World(sample.CreateWorldDef().WithGravity(new Vec2(0f, -10f)));
            sample.Build(world);

            Assert.AreEqual(1, world.PulleyJoints.Count, "Expected one pulley joint.");
            PulleyJoint pulley = world.PulleyJoints[0];

            float ratio = pulley.Ratio;
            float target = ComputePulleyCoordinate(pulley);

            // Disturb one side so the pulley has to react.
            pulley.BodyA.SetTransform(pulley.BodyA.Transform.P + new Vec2(0f, 1.5f), pulley.BodyA.Transform.Q.Angle);
            pulley.BodyA.SetAwake(true);
            pulley.BodyB.SetAwake(true);

            float maxError = 0f;
            float minA = float.MaxValue;
            float maxA = float.MinValue;
            float minB = float.MaxValue;
            float maxB = float.MinValue;
            for (int i = 0; i < 240; ++i)
            {
                sample.Step(world, 1f / 60f);
                world.Step(1f / 60f);

                float coordinate = ComputePulleyCoordinate(pulley);
                float error = MathF.Abs(coordinate - target);
                if (error > maxError)
                {
                    maxError = error;
                }

                float yA = pulley.BodyA.Transform.P.Y;
                float yB = pulley.BodyB.Transform.P.Y;
                if (yA < minA) minA = yA;
                if (yA > maxA) maxA = yA;
                if (yB < minB) minB = yB;
                if (yB > maxB) maxB = yB;
            }

            Assert.IsTrue(maxError < 0.12f, $"Expected pulley coordinate to remain near constant; ratio={ratio}, maxError={maxError}.");

            float travelA = maxA - minA;
            float travelB = maxB - minB;
            Assert.IsTrue(travelA > 0.05f && travelB > 0.05f, "Expected both pulley bodies to move.");

            // For ratio=1.5, side B travels less than side A.
            Assert.IsTrue(travelB < travelA, $"Expected ratio effect on travel (A>{travelA}, B={travelB}).");
        }

        private static float ComputePulleyCoordinate(PulleyJoint pulley)
        {
            Vec2 pA = pulley.BodyA.GetWorldPoint(pulley.LocalAnchorA);
            Vec2 pB = pulley.BodyB.GetWorldPoint(pulley.LocalAnchorB);
            float lengthA = (pA - pulley.GroundAnchorA).Length;
            float lengthB = (pB - pulley.GroundAnchorB).Length;
            return lengthA + pulley.Ratio * lengthB;
        }
    }
}
