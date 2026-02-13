using System;

namespace Box2DNG
{
    public sealed class WeldJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 LocalAnchorA { get; private set; }
        public Vec2 LocalAnchorB { get; private set; }
        public float ReferenceAngle { get; private set; }
        public bool CollideConnected { get; private set; }

        public WeldJointDef WithCollideConnected(bool collideConnected = true)
        {
            CollideConnected = collideConnected;
            return this;
        }

        public WeldJointDef(Body bodyA, Body bodyB, Vec2 worldAnchor)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            LocalAnchorA = Transform.MulT(bodyA.Transform, worldAnchor);
            LocalAnchorB = Transform.MulT(bodyB.Transform, worldAnchor);
            ReferenceAngle = bodyB.Transform.Q.Angle - bodyA.Transform.Q.Angle;
        }
    }
}
