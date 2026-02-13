using System;

namespace Box2DNG
{
    public sealed class MotorJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 LinearOffset { get; private set; }
        public float AngularOffset { get; private set; }
        public float MaxForce { get; private set; } = 1f;
        public float MaxTorque { get; private set; } = 1f;
        public float CorrectionFactor { get; private set; } = 0.3f;
        public bool CollideConnected { get; private set; }

        public MotorJointDef(Body bodyA, Body bodyB)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            Vec2 worldOffset = bodyB.GetWorldCenter() - bodyA.GetWorldCenter();
            LinearOffset = Rot.MulT(bodyA.Transform.Q, worldOffset);
            AngularOffset = bodyB.Transform.Q.Angle - bodyA.Transform.Q.Angle;
        }

        public MotorJointDef WithLinearOffset(Vec2 offset) { LinearOffset = offset; return this; }
        public MotorJointDef WithAngularOffset(float offset) { AngularOffset = offset; return this; }
        public MotorJointDef WithMaxForce(float value) { MaxForce = MathF.Max(0f, value); return this; }
        public MotorJointDef WithMaxTorque(float value) { MaxTorque = MathF.Max(0f, value); return this; }
        public MotorJointDef WithCorrectionFactor(float value) { CorrectionFactor = MathFng.Clamp(value, 0f, 1f); return this; }
        public MotorJointDef WithCollideConnected(bool collideConnected = true) { CollideConnected = collideConnected; return this; }
    }
}
