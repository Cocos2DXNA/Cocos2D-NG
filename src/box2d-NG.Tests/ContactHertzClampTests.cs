using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class ContactHertzClampTests
    {
        [TestMethod]
        public void ContactSoftness_ClampingReducesSoftness()
        {
            float timeStep = 1f / 10f;
            float unclamped = MeasureContactSoftness(clampHertz: false, contactHertz: 120f, timeStep: timeStep);
            float clamped = MeasureContactSoftness(clampHertz: true, contactHertz: 120f, timeStep: timeStep);
            float expectedClampedHertz = MathF.Min(120f, 0.125f / timeStep);
            float lowHertz = MeasureContactSoftness(clampHertz: false, contactHertz: expectedClampedHertz, timeStep: timeStep);

            Assert.IsTrue(unclamped > 0f, $"Expected softness > 0 without clamping, got {unclamped}.");
            Assert.IsTrue(clamped >= 0f, $"Expected softness >= 0 with clamping, got {clamped}.");
            Assert.IsTrue(MathF.Abs(clamped - lowHertz) < 1e-2f,
                $"Expected clamped softness to match low-hertz softness (clamped={clamped}, lowHertz={lowHertz}).");
            Assert.IsTrue(MathF.Abs(unclamped - clamped) > 1e-3f,
                $"Expected clamped softness to differ from unclamped softness (clamped={clamped}, unclamped={unclamped}).");
        }

        private static float MeasureContactSoftness(bool clampHertz, float contactHertz, float timeStep)
        {
            WorldDef def = new WorldDef()
                .WithGravity(new Vec2(0f, -10f))
                .WithContactHertz(contactHertz)
                .EnableSoftening(true)
                .UseSoftConstraintsSolver(true)
                .EnableContactHertzClamping(clampHertz);

            World world = new World(def);

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            PolygonShape groundShape = new PolygonShape(new[]
            {
                new Vec2(-10f, -0.5f),
                new Vec2(10f, -0.5f),
                new Vec2(10f, 0f),
                new Vec2(-10f, 0f)
            });
            ground.CreateFixture(new FixtureDef(groundShape));

            Body box = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f));
            PolygonShape boxShape = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.5f),
                new Vec2(0.5f, -0.5f),
                new Vec2(0.5f, 0.5f),
                new Vec2(-0.5f, 0.5f)
            });
            box.CreateFixture(new FixtureDef(boxShape).WithDensity(1f));

            for (int i = 0; i < 20; ++i)
            {
                world.Step(timeStep);
            }

            float maxSoftness = 0f;
            for (int i = 0; i < world.Contacts.Count; ++i)
            {
                Contact contact = world.Contacts[i];
                if (contact.Manifold.PointCount == 0)
                {
                    continue;
                }

                for (int p = 0; p < contact.Manifold.PointCount; ++p)
                {
                    float softness = contact.SolverPoints[p].Softness;
                    if (softness > maxSoftness)
                    {
                        maxSoftness = softness;
                    }
                }
            }

            return maxSoftness;
        }
    }
}
