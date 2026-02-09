using System;

namespace Box2DNG
{
    public sealed class FrictionJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 LocalAnchorA { get; private set; }
        public Vec2 LocalAnchorB { get; private set; }
        public float MaxForce { get; private set; }
        public float MaxTorque { get; private set; }

        public FrictionJointDef(Body bodyA, Body bodyB, Vec2 worldAnchor)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            LocalAnchorA = Transform.MulT(bodyA.Transform, worldAnchor);
            LocalAnchorB = Transform.MulT(bodyB.Transform, worldAnchor);
        }

        public FrictionJointDef WithMaxForce(float maxForce) { MaxForce = MathF.Max(0f, maxForce); return this; }
        public FrictionJointDef WithMaxTorque(float maxTorque) { MaxTorque = MathF.Max(0f, maxTorque); return this; }
    }
}
