using System;

namespace Box2DNG
{
    public sealed class PulleyJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 GroundAnchorA { get; private set; }
        public Vec2 GroundAnchorB { get; private set; }
        public Vec2 LocalAnchorA { get; private set; }
        public Vec2 LocalAnchorB { get; private set; }
        public float LengthA { get; private set; }
        public float LengthB { get; private set; }
        public float Ratio { get; private set; } = 1f;
        public bool CollideConnected { get; private set; }

        public PulleyJointDef(Body bodyA, Body bodyB, Vec2 groundAnchorA, Vec2 groundAnchorB, Vec2 anchorA, Vec2 anchorB, float ratio = 1f)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            GroundAnchorA = groundAnchorA;
            GroundAnchorB = groundAnchorB;
            LocalAnchorA = Transform.MulT(bodyA.Transform, anchorA);
            LocalAnchorB = Transform.MulT(bodyB.Transform, anchorB);
            LengthA = (anchorA - groundAnchorA).Length;
            LengthB = (anchorB - groundAnchorB).Length;
            Ratio = MathF.Max(Constants.Epsilon, ratio);
        }

        public PulleyJointDef WithRatio(float ratio)
        {
            Ratio = MathF.Max(Constants.Epsilon, ratio);
            return this;
        }

        public PulleyJointDef WithCollideConnected(bool collideConnected = true)
        {
            CollideConnected = collideConnected;
            return this;
        }
    }
}
