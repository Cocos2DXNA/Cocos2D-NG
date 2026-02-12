using Box2DNG.Viewer.Samples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class SampleCatalogParityTests
    {
        [TestMethod]
        public void AllViewerSamples_BuildAndStep_WithFiniteState()
        {
            ISample[] samples = (ISample[])SampleCatalog.All;
            for (int s = 0; s < samples.Length; ++s)
            {
                ISample sample = samples[s];
                World world = new World(sample.CreateWorldDef());
                sample.Build(world);

                int subSteps = System.Math.Max(1, sample.SubSteps);
                float dt = (1f / 60f) / subSteps;
                for (int frame = 0; frame < 120; ++frame)
                {
                    for (int i = 0; i < subSteps; ++i)
                    {
                        sample.Step(world, dt);
                        world.Step(dt);
                    }
                }

                AssertSampleStateFinite(sample.Name, world);
            }
        }

        [TestMethod]
        public void AllViewerSamples_AreDeterministicForFixedReplay()
        {
            ISample[] samples = (ISample[])SampleCatalog.All;
            for (int s = 0; s < samples.Length; ++s)
            {
                System.Type sampleType = samples[s].GetType();
                ISample sampleA = CreateSampleInstance(sampleType);
                ISample sampleB = CreateSampleInstance(sampleType);

                World worldA = new World(sampleA.CreateWorldDef());
                World worldB = new World(sampleB.CreateWorldDef());
                sampleA.Build(worldA);
                sampleB.Build(worldB);

                int subStepsA = System.Math.Max(1, sampleA.SubSteps);
                int subStepsB = System.Math.Max(1, sampleB.SubSteps);
                Assert.AreEqual(subStepsA, subStepsB, $"Sample {sampleA.Name}: substep mismatch.");
                float dt = (1f / 60f) / subStepsA;

                for (int frame = 0; frame < 90; ++frame)
                {
                    for (int i = 0; i < subStepsA; ++i)
                    {
                        sampleA.Step(worldA, dt);
                        worldA.Step(dt);
                    }
                }

                for (int frame = 0; frame < 90; ++frame)
                {
                    for (int i = 0; i < subStepsA; ++i)
                    {
                        sampleB.Step(worldB, dt);
                        worldB.Step(dt);
                    }
                }

                AssertWorldParity(sampleA.Name, worldA, worldB);
            }
        }

        private static ISample CreateSampleInstance(System.Type type)
        {
            return (ISample)System.Activator.CreateInstance(type)!;
        }

        private static void AssertSampleStateFinite(string sampleName, World world)
        {
            for (int i = 0; i < world.Bodies.Count; ++i)
            {
                Body body = world.Bodies[i];
                Assert.IsFalse(float.IsNaN(body.Transform.P.X) || float.IsNaN(body.Transform.P.Y),
                    $"Sample {sampleName}: body[{i}] transform is NaN.");
                Assert.IsFalse(float.IsNaN(body.LinearVelocity.X) || float.IsNaN(body.LinearVelocity.Y),
                    $"Sample {sampleName}: body[{i}] velocity is NaN.");
                Assert.IsFalse(float.IsNaN(body.AngularVelocity),
                    $"Sample {sampleName}: body[{i}] angular velocity is NaN.");
                Assert.IsTrue(System.MathF.Abs(body.Transform.P.X) < 1e5f && System.MathF.Abs(body.Transform.P.Y) < 1e5f,
                    $"Sample {sampleName}: body[{i}] moved to unreasonable coordinates ({body.Transform.P.X}, {body.Transform.P.Y}).");
            }
        }

        private static void AssertWorldParity(string sampleName, World a, World b)
        {
            const float tol = 1e-5f;
            Assert.AreEqual(a.Bodies.Count, b.Bodies.Count, $"Sample {sampleName}: body count mismatch.");
            Assert.AreEqual(a.Contacts.Count, b.Contacts.Count, $"Sample {sampleName}: contact count mismatch.");

            for (int i = 0; i < a.Bodies.Count; ++i)
            {
                Body ba = a.Bodies[i];
                Body bb = b.Bodies[i];
                Assert.AreEqual(ba.Id, bb.Id, $"Sample {sampleName}: body id mismatch at index {i}.");
                AssertNear(ba.Transform.P.X, bb.Transform.P.X, tol, $"Sample {sampleName}: body[{i}].x mismatch.");
                AssertNear(ba.Transform.P.Y, bb.Transform.P.Y, tol, $"Sample {sampleName}: body[{i}].y mismatch.");
                AssertNear(ba.Transform.Q.Angle, bb.Transform.Q.Angle, tol, $"Sample {sampleName}: body[{i}].angle mismatch.");
                AssertNear(ba.LinearVelocity.X, bb.LinearVelocity.X, tol, $"Sample {sampleName}: body[{i}].vx mismatch.");
                AssertNear(ba.LinearVelocity.Y, bb.LinearVelocity.Y, tol, $"Sample {sampleName}: body[{i}].vy mismatch.");
                AssertNear(ba.AngularVelocity, bb.AngularVelocity, tol, $"Sample {sampleName}: body[{i}].w mismatch.");
            }
        }

        private static void AssertNear(float a, float b, float tolerance, string message)
        {
            Assert.IsTrue(System.MathF.Abs(a - b) <= tolerance, $"{message} a={a} b={b}");
        }
    }
}
