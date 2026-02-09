using System;

namespace Box2DNG
{
    public sealed class WheelJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 LocalAnchorA { get; private set; }
        public Vec2 LocalAnchorB { get; private set; }
        public Vec2 LocalAxisA { get; private set; }
        public bool EnableMotor { get; private set; }
        public float MotorSpeed { get; private set; }
        public float MaxMotorTorque { get; private set; }
        public float FrequencyHz { get; private set; }
        public float DampingRatio { get; private set; }
        public bool EnableLimit { get; private set; }
        public float LowerTranslation { get; private set; }
        public float UpperTranslation { get; private set; }

        public WheelJointDef(Body bodyA, Body bodyB, Vec2 worldAnchor, Vec2 worldAxis)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            LocalAnchorA = Transform.MulT(bodyA.Transform, worldAnchor);
            LocalAnchorB = Transform.MulT(bodyB.Transform, worldAnchor);
            Vec2 axis = worldAxis.Normalize();
            LocalAxisA = Rot.MulT(bodyA.Transform.Q, axis);
        }

        public WheelJointDef WithMotor(bool enable, float speed, float maxTorque)
        {
            EnableMotor = enable;
            MotorSpeed = speed;
            MaxMotorTorque = maxTorque;
            return this;
        }

        public WheelJointDef WithSpring(float frequencyHz, float dampingRatio)
        {
            FrequencyHz = frequencyHz;
            DampingRatio = dampingRatio;
            return this;
        }

        public WheelJointDef WithLimits(float lower, float upper, bool enable = true)
        {
            EnableLimit = enable;
            LowerTranslation = MathF.Min(lower, upper);
            UpperTranslation = MathF.Max(lower, upper);
            return this;
        }
    }
}
