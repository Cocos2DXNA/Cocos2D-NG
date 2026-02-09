using System;

namespace Box2DNG
{
    public sealed class RevoluteJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 LocalAnchorA { get; private set; }
        public Vec2 LocalAnchorB { get; private set; }
        public float ReferenceAngle { get; private set; }
        public bool EnableMotor { get; private set; }
        public float MotorSpeed { get; private set; }
        public float MaxMotorTorque { get; private set; }
        public bool EnableLimit { get; private set; }
        public float LowerAngle { get; private set; }
        public float UpperAngle { get; private set; }

        public RevoluteJointDef(Body bodyA, Body bodyB, Vec2 worldAnchor)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            LocalAnchorA = Transform.MulT(bodyA.Transform, worldAnchor);
            LocalAnchorB = Transform.MulT(bodyB.Transform, worldAnchor);
            ReferenceAngle = bodyB.Transform.Q.Angle - bodyA.Transform.Q.Angle;
        }

        public RevoluteJointDef WithMotor(bool enable, float speed, float maxTorque)
        {
            EnableMotor = enable;
            MotorSpeed = speed;
            MaxMotorTorque = maxTorque;
            return this;
        }

        public RevoluteJointDef WithLimit(float lowerAngle, float upperAngle, bool enable = true)
        {
            EnableLimit = enable;
            LowerAngle = MathF.Min(lowerAngle, upperAngle);
            UpperAngle = MathF.Max(lowerAngle, upperAngle);
            return this;
        }
    }
}
