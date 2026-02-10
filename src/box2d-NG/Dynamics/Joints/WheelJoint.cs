using System;

namespace Box2DNG
{
    public sealed class WheelJoint
    {
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public Vec2 LocalAxisA { get; }
        public bool EnableMotor { get; private set; }
        public float MotorSpeed { get; private set; }
        public float MaxMotorTorque { get; private set; }
        public float FrequencyHz { get; private set; }
        public float DampingRatio { get; private set; }
        public bool EnableLimit { get; private set; }
        public float LowerTranslation { get; private set; }
        public float UpperTranslation { get; private set; }
        public bool CollideConnected { get; }

        private float _perpMass;
        private float _springMass;
        private float _motorMass;
        private float _impulse;
        private float _springImpulse;
        private float _motorImpulse;
        private float _lowerImpulse;
        private float _upperImpulse;
        private Softness _springSoftness;

        public WheelJoint(WheelJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            LocalAxisA = def.LocalAxisA.Normalize();
            EnableMotor = def.EnableMotor;
            MotorSpeed = def.MotorSpeed;
            MaxMotorTorque = def.MaxMotorTorque;
            FrequencyHz = def.FrequencyHz;
            DampingRatio = def.DampingRatio;
            EnableLimit = def.EnableLimit;
            LowerTranslation = def.LowerTranslation;
            UpperTranslation = def.UpperTranslation;
            CollideConnected = def.CollideConnected;
        }

        public void SetMotorEnabled(bool enable) => EnableMotor = enable;

        public void SetMotorSpeed(float speed) => MotorSpeed = speed;

        public void SetMaxMotorTorque(float torque) => MaxMotorTorque = MathF.Max(0f, torque);

        public void SetSpringFrequencyHz(float hz) => FrequencyHz = MathF.Max(0f, hz);

        public void SetSpringDampingRatio(float ratio) => DampingRatio = MathF.Max(0f, ratio);

        public void SetLimits(float lower, float upper)
        {
            EnableLimit = true;
            LowerTranslation = MathF.Min(lower, upper);
            UpperTranslation = MathF.Max(lower, upper);
        }

        public void SetLimitsEnabled(bool enable) => EnableLimit = enable;

        internal void InitVelocityConstraints(float dt)
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
            Vec2 d = (BodyB.GetWorldCenter() + rB) - (BodyA.GetWorldCenter() + rA);

            Vec2 axis = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
            Vec2 perp = new Vec2(-axis.Y, axis.X);

            float s1 = Vec2.Cross(d + rA, perp);
            float s2 = Vec2.Cross(rB, perp);
            float a1 = Vec2.Cross(d + rA, axis);
            float a2 = Vec2.Cross(rB, axis);

            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            float kPerp = mA + mB + iA * s1 * s1 + iB * s2 * s2;
            _perpMass = kPerp > 0f ? 1f / kPerp : 0f;

            float kAxis = mA + mB + iA * a1 * a1 + iB * a2 * a2;
            _springMass = kAxis > 0f ? 1f / kAxis : 0f;
            _springSoftness = Softness.Make(FrequencyHz, DampingRatio, dt);
            if (FrequencyHz <= 0f)
            {
                _springImpulse = 0f;
            }

            if (EnableMotor)
            {
                float motorMass = iA + iB;
                _motorMass = motorMass > 0f ? 1f / motorMass : 0f;
            }
            else
            {
                _motorMass = 0f;
                _motorImpulse = 0f;
            }

            if (!EnableLimit)
            {
                _lowerImpulse = 0f;
                _upperImpulse = 0f;
            }

        }

        internal void SolveVelocityConstraints(float dt)
        {
            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            Vec2 vA = BodyA.LinearVelocity;
            Vec2 vB = BodyB.LinearVelocity;
            float wA = BodyA.AngularVelocity;
            float wB = BodyB.AngularVelocity;

            Vec2 d = (BodyB.GetWorldCenter() + rB) - (BodyA.GetWorldCenter() + rA);
            Vec2 axis = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
            Vec2 perp = new Vec2(-axis.Y, axis.X);
            float s1 = Vec2.Cross(d + rA, perp);
            float s2 = Vec2.Cross(rB, perp);
            float a1 = Vec2.Cross(d + rA, axis);
            float a2 = Vec2.Cross(rB, axis);

            if (EnableMotor)
            {
                float CdotMotor = wB - wA - MotorSpeed;
                float impulseMotor = -_motorMass * CdotMotor;
                float maxImpulse = MaxMotorTorque * dt;
                float newImpulse = MathFng.Clamp(_motorImpulse + impulseMotor, -maxImpulse, maxImpulse);
                float applied = newImpulse - _motorImpulse;
                _motorImpulse = newImpulse;

                wA -= iA * applied;
                wB += iB * applied;
            }

            if (FrequencyHz > 0f)
            {
                float translation = Vec2.Dot(axis, d);
                float CdotAxis = Vec2.Dot(axis, vB + Vec2.Cross(wB, rB) - vA - Vec2.Cross(wA, rA));
                float impulseAxis = -_springSoftness.MassScale * _springMass * (CdotAxis + _springSoftness.BiasRate * translation)
                                    - _springSoftness.ImpulseScale * _springImpulse;
                _springImpulse += impulseAxis;

                Vec2 Paxis = impulseAxis * axis;
                float LAaxis = impulseAxis * a1;
                float LBaxis = impulseAxis * a2;
                vA -= mA * Paxis;
                wA -= iA * LAaxis;
                vB += mB * Paxis;
                wB += iB * LBaxis;
            }

            if (EnableLimit)
            {
                float translation = Vec2.Dot(axis, d);
                if (translation < LowerTranslation)
                {
                    float C = translation - LowerTranslation;
                    float Cdot = Vec2.Dot(axis, vB + Vec2.Cross(wB, rB) - vA - Vec2.Cross(wA, rA));
                    float impulse = -_springMass * (Cdot + MathF.Min(0f, C) * 0.2f);
                    float prevImpulse = _lowerImpulse;
                    _lowerImpulse = MathF.Min(prevImpulse + impulse, 0f);
                    impulse = _lowerImpulse - prevImpulse;

                    Vec2 limitP = impulse * axis;
                    float limitLA = impulse * a1;
                    float limitLB = impulse * a2;
                    vA -= mA * limitP;
                    wA -= iA * limitLA;
                    vB += mB * limitP;
                    wB += iB * limitLB;
                }
                else
                {
                    _lowerImpulse = 0f;
                }

                if (translation > UpperTranslation)
                {
                    float C = UpperTranslation - translation;
                    float Cdot = Vec2.Dot(axis, vA + Vec2.Cross(wA, rA) - vB - Vec2.Cross(wB, rB));
                    float impulse = -_springMass * (Cdot + MathF.Min(0f, C) * 0.2f);
                    float prevImpulse = _upperImpulse;
                    _upperImpulse = MathF.Max(prevImpulse + impulse, 0f);
                    impulse = _upperImpulse - prevImpulse;

                    Vec2 limitP = impulse * axis;
                    float limitLA = impulse * a1;
                    float limitLB = impulse * a2;
                    vA += mA * limitP;
                    wA += iA * limitLA;
                    vB -= mB * limitP;
                    wB -= iB * limitLB;
                }
                else
                {
                    _upperImpulse = 0f;
                }
            }

            float CdotPerp = Vec2.Dot(perp, vB + Vec2.Cross(wB, rB) - vA - Vec2.Cross(wA, rA));
            float impulsePerp = -_perpMass * CdotPerp;
            _impulse += impulsePerp;

            Vec2 P = impulsePerp * perp;
            float LA = impulsePerp * s1;
            float LB = impulsePerp * s2;
            vA -= mA * P;
            wA -= iA * LA;
            vB += mB * P;
            wB += iB * LB;

            BodyA.LinearVelocity = vA;
            BodyA.AngularVelocity = wA;
            BodyB.LinearVelocity = vB;
            BodyB.AngularVelocity = wB;
        }

        internal void SolvePositionConstraints()
        {
            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
            Vec2 d = (BodyB.GetWorldCenter() + rB) - (BodyA.GetWorldCenter() + rA);

            Vec2 axis = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
            Vec2 perp = new Vec2(-axis.Y, axis.X);

            float s1 = Vec2.Cross(d + rA, perp);
            float s2 = Vec2.Cross(rB, perp);
            float C = Vec2.Dot(perp, d);

            float k = mA + mB + iA * s1 * s1 + iB * s2 * s2;
            float impulse = k > 0f ? -C / k : 0f;
            Vec2 P = impulse * perp;
            float LA = impulse * s1;
            float LB = impulse * s2;

            Vec2 centerA = BodyA.GetWorldCenter() - mA * P;
            Vec2 centerB = BodyB.GetWorldCenter() + mB * P;
            float angleA = BodyA.Transform.Q.Angle - iA * LA;
            float angleB = BodyB.Transform.Q.Angle + iB * LB;
            BodyA.SetTransformFromCenter(centerA, angleA);
            BodyB.SetTransformFromCenter(centerB, angleB);

            if (EnableLimit)
            {
                Vec2 rA2 = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
                Vec2 rB2 = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
                Vec2 d2 = (BodyB.GetWorldCenter() + rB2) - (BodyA.GetWorldCenter() + rA2);
                Vec2 axis2 = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
                float a1 = Vec2.Cross(d2 + rA2, axis2);
                float a2 = Vec2.Cross(rB2, axis2);
                float kAxis = mA + mB + iA * a1 * a1 + iB * a2 * a2;

                if (kAxis > 0f)
                {
                    float translation = Vec2.Dot(axis2, d2);
                    float limitC = 0f;
                    if (translation < LowerTranslation)
                    {
                        limitC = translation - LowerTranslation;
                    }
                    else if (translation > UpperTranslation)
                    {
                        limitC = translation - UpperTranslation;
                    }
                    else
                    {
                        return;
                    }

                    float limitImpulse = -limitC / kAxis;
                    Vec2 limitP = limitImpulse * axis2;
                    float limitLA = limitImpulse * a1;
                    float limitLB = limitImpulse * a2;

                    Vec2 centerA3 = BodyA.GetWorldCenter() - mA * limitP;
                    Vec2 centerB3 = BodyB.GetWorldCenter() + mB * limitP;
                    float angleA3 = BodyA.Transform.Q.Angle - iA * limitLA;
                    float angleB3 = BodyB.Transform.Q.Angle + iB * limitLB;
                    BodyA.SetTransformFromCenter(centerA3, angleA3);
                    BodyB.SetTransformFromCenter(centerB3, angleB3);
                }
            }
        }
    }
}
