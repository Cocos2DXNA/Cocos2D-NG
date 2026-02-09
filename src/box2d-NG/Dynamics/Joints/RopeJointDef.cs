using System;

namespace Box2DNG
{
    public sealed class RopeJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 LocalAnchorA { get; private set; }
        public Vec2 LocalAnchorB { get; private set; }
        public float MaxLength { get; private set; }

        public RopeJointDef(Body bodyA, Body bodyB, Vec2 worldAnchorA, Vec2 worldAnchorB)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            LocalAnchorA = Transform.MulT(bodyA.Transform, worldAnchorA);
            LocalAnchorB = Transform.MulT(bodyB.Transform, worldAnchorB);
            MaxLength = (worldAnchorB - worldAnchorA).Length;
        }

        public RopeJointDef WithMaxLength(float maxLength)
        {
            MaxLength = MathF.Max(Constants.LinearSlop, maxLength);
            return this;
        }
    }
}
