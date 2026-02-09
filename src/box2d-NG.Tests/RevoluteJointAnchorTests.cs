using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Box2DNG.Tests
{
    [TestClass]
    public class RevoluteJointAnchorTests
    {
        [TestMethod]
        public void RevoluteJoint_AnchorsDynamicBodyToStatic()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body anchor = world.CreateBody(new BodyDef().AsStatic().At(0f, 5f));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(1f, 5f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            Vec2 jointAnchor = new Vec2(0f, 5f);
            world.CreateJoint(new RevoluteJointDef(anchor, body, jointAnchor));

            Vec2 anchorPosStart = anchor.Transform.P;

            for (int i = 0; i < 300; ++i)
            {
                world.Step(1f / 60f);
            }

            Vec2 anchorPosEnd = anchor.Transform.P;
            Assert.IsTrue((anchorPosEnd - anchorPosStart).Length < 1e-3f, "Anchor body should remain fixed.");

            Vec2 rA = Rot.Mul(anchor.Transform.Q, Transform.MulT(anchor.Transform, jointAnchor) - anchor.LocalCenter);
            Vec2 rB = Rot.Mul(body.Transform.Q, Transform.MulT(body.Transform, jointAnchor) - body.LocalCenter);
            Vec2 pA = anchor.GetWorldCenter() + rA;
            Vec2 pB = body.GetWorldCenter() + rB;
            float separation = (pB - pA).Length;
            Assert.IsTrue(separation < 0.1f, $"Joint anchor should remain coincident. separation={separation}");
        }
    }
}
