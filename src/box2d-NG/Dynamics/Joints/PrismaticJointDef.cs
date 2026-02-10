using System;

namespace Box2DNG
{
    public sealed class PrismaticJointDef
    {
        public Body BodyA { get; private set; }
        public Body BodyB { get; private set; }
        public Vec2 LocalAnchorA { get; private set; }
        public Vec2 LocalAnchorB { get; private set; }
        public Vec2 LocalAxisA { get; private set; }
        public float ReferenceAngle { get; private set; }
        public bool EnableSpring { get; private set; }
        public float FrequencyHz { get; private set; }
        public float DampingRatio { get; private set; }
        public float TargetTranslation { get; private set; }
        public bool EnableLimit { get; private set; }
        public float LowerTranslation { get; private set; }
        public float UpperTranslation { get; private set; }
        public bool EnableMotor { get; private set; }
        public float MotorSpeed { get; private set; }
        public float MaxMotorForce { get; private set; }
        public bool CollideConnected { get; private set; }
        public float ConstraintHertz { get; private set; }
        public float ConstraintDampingRatio { get; private set; }

        public PrismaticJointDef(Body bodyA, Body bodyB, Vec2 worldAnchor, Vec2 worldAxis)
        {
            BodyA = bodyA ?? throw new ArgumentNullException(nameof(bodyA));
            BodyB = bodyB ?? throw new ArgumentNullException(nameof(bodyB));
            LocalAnchorA = Transform.MulT(bodyA.Transform, worldAnchor);
            LocalAnchorB = Transform.MulT(bodyB.Transform, worldAnchor);
            Vec2 axis = worldAxis.Normalize();
            LocalAxisA = Rot.MulT(bodyA.Transform.Q, axis);
            ReferenceAngle = bodyB.Transform.Q.Angle - bodyA.Transform.Q.Angle;
        }

        public PrismaticJointDef WithLimit(float lower, float upper)
        {
            EnableLimit = true;
            LowerTranslation = MathF.Min(lower, upper);
            UpperTranslation = MathF.Max(lower, upper);
            return this;
        }

        public PrismaticJointDef WithSpring(float hertz, float dampingRatio, float targetTranslation = 0f)
        {
            EnableSpring = true;
            FrequencyHz = MathF.Max(0f, hertz);
            DampingRatio = MathF.Max(0f, dampingRatio);
            TargetTranslation = targetTranslation;
            return this;
        }

        public PrismaticJointDef WithTargetTranslation(float translation)
        {
            TargetTranslation = translation;
            return this;
        }

        public PrismaticJointDef WithMotor(float speed, float maxForce)
        {
            EnableMotor = true;
            MotorSpeed = speed;
            MaxMotorForce = MathF.Max(0f, maxForce);
            return this;
        }

        public PrismaticJointDef WithConstraintTuning(float hertz, float dampingRatio)
        {
            ConstraintHertz = MathF.Max(0f, hertz);
            ConstraintDampingRatio = MathF.Max(0f, dampingRatio);
            return this;
        }

        public PrismaticJointDef WithCollideConnected(bool collideConnected = true)
        {
            CollideConnected = collideConnected;
            return this;
        }
    }
}
