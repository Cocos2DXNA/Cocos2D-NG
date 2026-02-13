using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class SoftConstraintToggleTests
    {
        [TestMethod]
        public void ContactSoftness_RespectsToggle()
        {
            float softnessEnabled = MeasureContactSoftness(useSoftConstraints: true);
            float softnessDisabled = MeasureContactSoftness(useSoftConstraints: false);

            Assert.IsTrue(softnessEnabled > 0f, $"Expected softness > 0 with soft constraints enabled, got {softnessEnabled}.");
            Assert.AreEqual(0f, softnessDisabled, 1e-6f, $"Expected softness ~ 0 with soft constraints disabled, got {softnessDisabled}.");
        }

        private static float MeasureContactSoftness(bool useSoftConstraints)
        {
            WorldDef def = new WorldDef()
                .WithGravity(new Vec2(0f, -10f))
                .EnableSoftening(true)
                .UseSoftConstraintsSolver(useSoftConstraints);

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

            // Step forward until contact is created.
            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
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
