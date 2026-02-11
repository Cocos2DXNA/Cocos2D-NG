using System;

namespace Box2DNG
{
    public sealed class PrismaticJoint
    {
        public int Id { get; internal set; } = -1;
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public Vec2 LocalAxisA { get; }
        public float ReferenceAngle { get; }
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
        public bool CollideConnected { get; }
        public float ConstraintHertz { get; private set; }
        public float ConstraintDampingRatio { get; private set; }

        private Mat22 _k;
        private Vec2 _perp;
        private Vec2 _axis;
        private float _s1;
        private float _s2;
        private float _a1;
        private float _a2;
        private Vec2 _impulse;
        private float _motorImpulse;
        private float _springImpulse;
        private float _lowerImpulse;
        private float _upperImpulse;
        private float _axialMass;
        private Softness _springSoftness;
        private Softness _constraintSoftness;

        public PrismaticJoint(PrismaticJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            LocalAxisA = def.LocalAxisA.Normalize();
            ReferenceAngle = def.ReferenceAngle;
            EnableSpring = def.EnableSpring;
            FrequencyHz = def.FrequencyHz;
            DampingRatio = def.DampingRatio;
            TargetTranslation = def.TargetTranslation;
            EnableLimit = def.EnableLimit;
            LowerTranslation = def.LowerTranslation;
            UpperTranslation = def.UpperTranslation;
            EnableMotor = def.EnableMotor;
            MotorSpeed = def.MotorSpeed;
            MaxMotorForce = def.MaxMotorForce;
            CollideConnected = def.CollideConnected;
            ConstraintHertz = def.ConstraintHertz;
            ConstraintDampingRatio = def.ConstraintDampingRatio;
            _springSoftness = default;
            _constraintSoftness = default;
        }

        public void SetSpringEnabled(bool enable)
        {
            if (enable != EnableSpring)
            {
                EnableSpring = enable;
                _springImpulse = 0f;
            }
        }

        public void SetSpringFrequencyHz(float hz) => FrequencyHz = MathF.Max(0f, hz);

        public void SetSpringDampingRatio(float ratio) => DampingRatio = MathF.Max(0f, ratio);

        public void SetTargetTranslation(float translation) => TargetTranslation = translation;

        public void SetMotorEnabled(bool enable)
        {
            if (enable != EnableMotor)
            {
                EnableMotor = enable;
                _motorImpulse = 0f;
            }
        }

        public void SetMotorSpeed(float speed) => MotorSpeed = speed;

        public void SetMaxMotorForce(float force) => MaxMotorForce = MathF.Max(0f, force);

        public void SetLimitEnabled(bool enable)
        {
            if (enable != EnableLimit)
            {
                EnableLimit = enable;
                _lowerImpulse = 0f;
                _upperImpulse = 0f;
            }
        }

        public void SetConstraintTuning(float hertz, float dampingRatio)
        {
            ConstraintHertz = MathF.Max(0f, hertz);
            ConstraintDampingRatio = MathF.Max(0f, dampingRatio);
        }

        public void SetLimits(float lower, float upper)
        {
            EnableLimit = true;
            LowerTranslation = MathF.Min(lower, upper);
            UpperTranslation = MathF.Max(lower, upper);
            _lowerImpulse = 0f;
            _upperImpulse = 0f;
        }

        public float GetTranslation()
        {
            Vec2 axis = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
            Vec2 pA = Transform.Mul(BodyA.Transform, LocalAnchorA);
            Vec2 pB = Transform.Mul(BodyB.Transform, LocalAnchorB);
            Vec2 d = pB - pA;
            return Vec2.Dot(d, axis);
        }

        public float GetSpeed()
        {
            Vec2 axis = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
            Vec2 d = (BodyB.GetWorldCenter() - BodyA.GetWorldCenter()) + (rB - rA);

            Vec2 vA = BodyA.LinearVelocity + Vec2.Cross(BodyA.AngularVelocity, rA);
            Vec2 vB = BodyB.LinearVelocity + Vec2.Cross(BodyB.AngularVelocity, rB);
            Vec2 vRel = vB - vA;

            return Vec2.Dot(d, Vec2.Cross(BodyA.AngularVelocity, axis)) + Vec2.Dot(axis, vRel);
        }

        public float GetMotorForce(float invDt)
        {
            return invDt * _motorImpulse;
        }

        internal void InitVelocityConstraints(float dt)
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            Vec2 d = (BodyB.GetWorldCenter() + rB) - (BodyA.GetWorldCenter() + rA);
            _axis = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
            _perp = new Vec2(-_axis.Y, _axis.X);

            _s1 = Vec2.Cross(d + rA, _perp);
            _s2 = Vec2.Cross(rB, _perp);
            _a1 = Vec2.Cross(d + rA, _axis);
            _a2 = Vec2.Cross(rB, _axis);

            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            float k11 = mA + mB + iA * _s1 * _s1 + iB * _s2 * _s2;
            float k12 = iA * _s1 + iB * _s2;
            float k22 = iA + iB;
            if (k22 == 0f)
            {
                // For bodies with fixed rotation, keep the matrix invertible.
                k22 = 1f;
            }
            _k = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));

            float motorMass = mA + mB + iA * _a1 * _a1 + iB * _a2 * _a2;
            _axialMass = motorMass > 0f ? 1f / motorMass : 0f;

            if (!EnableMotor)
            {
                _motorImpulse = 0f;
            }

            if (!EnableLimit)
            {
                _lowerImpulse = 0f;
                _upperImpulse = 0f;
            }

            _springSoftness = Softness.Make(FrequencyHz, DampingRatio, dt);
            if (!EnableSpring)
            {
                _springImpulse = 0f;
            }

            _constraintSoftness = Softness.Make(ConstraintHertz, ConstraintDampingRatio, dt);
            if (ConstraintHertz <= 0f)
            {
                _constraintSoftness = new Softness(0f, 1f, 0f);
            }

            float axialImpulse = _springImpulse + _motorImpulse + _lowerImpulse - _upperImpulse;
            if (_impulse.X != 0f || _impulse.Y != 0f || axialImpulse != 0f)
            {
                Vec2 P = axialImpulse * _axis + _impulse.X * _perp;
                float LA = axialImpulse * _a1 + _impulse.X * _s1 + _impulse.Y;
                float LB = axialImpulse * _a2 + _impulse.X * _s2 + _impulse.Y;

                BodyA.LinearVelocity -= mA * P;
                BodyA.AngularVelocity -= iA * LA;
                BodyB.LinearVelocity += mB * P;
                BodyB.AngularVelocity += iB * LB;
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
            _axis = Rot.Mul(BodyA.Transform.Q, LocalAxisA);
            _perp = new Vec2(-_axis.Y, _axis.X);

            _s1 = Vec2.Cross(d + rA, _perp);
            _s2 = Vec2.Cross(rB, _perp);
            _a1 = Vec2.Cross(d + rA, _axis);
            _a2 = Vec2.Cross(rB, _axis);

            float axialMass = mA + mB + iA * _a1 * _a1 + iB * _a2 * _a2;
            _axialMass = axialMass > 0f ? 1f / axialMass : 0f;

            if (EnableSpring)
            {
                float translation = Vec2.Dot(_axis, d);
                float C = translation - TargetTranslation;
                float Cdot = Vec2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                float impulse = -_springSoftness.MassScale * _axialMass * (Cdot + _springSoftness.BiasRate * C)
                                - _springSoftness.ImpulseScale * _springImpulse;
                _springImpulse += impulse;

                Vec2 P = impulse * _axis;
                float LA = impulse * _a1;
                float LB = impulse * _a2;
                vA -= mA * P;
                wA -= iA * LA;
                vB += mB * P;
                wB += iB * LB;
            }

            if (EnableMotor)
            {
                float Cdot = Vec2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                float impulse = _axialMass * (MotorSpeed - Cdot);
                float oldImpulse = _motorImpulse;
                float maxImpulse = MaxMotorForce * dt;
                _motorImpulse = MathFng.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                impulse = _motorImpulse - oldImpulse;

                Vec2 P = impulse * _axis;
                float LA = impulse * _a1;
                float LB = impulse * _a2;
                vA -= mA * P;
                wA -= iA * LA;
                vB += mB * P;
                wB += iB * LB;
            }

            if (EnableLimit)
            {
                float translation = Vec2.Dot(_axis, d);
                float speculativeDistance = 0.25f * (UpperTranslation - LowerTranslation);
                if (speculativeDistance < 0f)
                {
                    speculativeDistance = 0f;
                }
                float invDt = dt > 0f ? 1f / dt : 0f;

                {
                    float C = translation - LowerTranslation;
                    if (C < speculativeDistance)
                    {
                        float bias1 = 0f;
                        float massScale = 1f;
                        float impulseScale = 0f;
                        if (C > 0f)
                        {
                            float safe = 1f;
                            bias1 = MathF.Min(C, safe) * invDt;
                        }
                        else
                        {
                            bias1 = _constraintSoftness.BiasRate * C;
                            massScale = _constraintSoftness.MassScale;
                            impulseScale = _constraintSoftness.ImpulseScale;
                        }

                        float Cdot = Vec2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                        float oldImpulse = _lowerImpulse;
                        float deltaImpulse = -_axialMass * massScale * (Cdot + bias1) - impulseScale * oldImpulse;
                        _lowerImpulse = MathF.Max(oldImpulse + deltaImpulse, 0f);
                        deltaImpulse = _lowerImpulse - oldImpulse;

                        Vec2 P = deltaImpulse * _axis;
                        float LA = deltaImpulse * _a1;
                        float LB = deltaImpulse * _a2;
                        vA -= mA * P;
                        wA -= iA * LA;
                        vB += mB * P;
                        wB += iB * LB;
                    }
                    else
                    {
                        _lowerImpulse = 0f;
                    }
                }

                {
                    float C = UpperTranslation - translation;
                    if (C < speculativeDistance)
                    {
                        float bias2 = 0f;
                        float massScale = 1f;
                        float impulseScale = 0f;
                        if (C > 0f)
                        {
                            float safe = 1f;
                            bias2 = MathF.Min(C, safe) * invDt;
                        }
                        else
                        {
                            bias2 = _constraintSoftness.BiasRate * C;
                            massScale = _constraintSoftness.MassScale;
                            impulseScale = _constraintSoftness.ImpulseScale;
                        }

                        float Cdot = Vec2.Dot(_axis, vA - vB) + _a1 * wA - _a2 * wB;
                        float oldImpulse = _upperImpulse;
                        float deltaImpulse = -_axialMass * massScale * (Cdot + bias2) - impulseScale * oldImpulse;
                        _upperImpulse = MathF.Max(oldImpulse + deltaImpulse, 0f);
                        deltaImpulse = _upperImpulse - oldImpulse;

                        Vec2 P = deltaImpulse * _axis;
                        float LA = deltaImpulse * _a1;
                        float LB = deltaImpulse * _a2;
                        vA += mA * P;
                        wA += iA * LA;
                        vB -= mB * P;
                        wB -= iB * LB;
                    }
                    else
                    {
                        _upperImpulse = 0f;
                    }
                }
            }

            Vec2 Cdot1 = new Vec2(
                Vec2.Dot(_perp, vB - vA) + _s2 * wB - _s1 * wA,
                wB - wA);

            Vec2 C1 = new Vec2(
                Vec2.Dot(_perp, d),
                (BodyB.Transform.Q.Angle - BodyA.Transform.Q.Angle) - ReferenceAngle);

            Vec2 bias = new Vec2(_constraintSoftness.BiasRate * C1.X, _constraintSoftness.BiasRate * C1.Y);
            Vec2 rhs = Cdot1 + bias;
            Vec2 impulseDelta = Solve22(_k, rhs);
            Vec2 df = new Vec2(
                -_constraintSoftness.MassScale * impulseDelta.X - _constraintSoftness.ImpulseScale * _impulse.X,
                -_constraintSoftness.MassScale * impulseDelta.Y - _constraintSoftness.ImpulseScale * _impulse.Y);
            _impulse += df;

            Vec2 Pperp = df.X * _perp;
            float LAperp = df.X * _s1 + df.Y;
            float LBperp = df.X * _s2 + df.Y;
            vA -= mA * Pperp;
            wA -= iA * LAperp;
            vB += mB * Pperp;
            wB += iB * LBperp;

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
            float a1 = Vec2.Cross(d + rA, axis);
            float a2 = Vec2.Cross(rB, axis);

            Vec2 C1 = new Vec2(
                Vec2.Dot(perp, d),
                (BodyB.Transform.Q.Angle - BodyA.Transform.Q.Angle) - ReferenceAngle);

            float linearError = MathF.Abs(C1.X);
            float angularError = MathF.Abs(C1.Y);

            bool active = false;
            float C2 = 0f;
            if (EnableLimit)
            {
                float translation = Vec2.Dot(axis, d);
                if (MathF.Abs(UpperTranslation - LowerTranslation) < 2f * Constants.LinearSlop)
                {
                    C2 = MathFng.Clamp(translation, -Constants.MaxLinearCorrection, Constants.MaxLinearCorrection);
                    linearError = MathF.Max(linearError, MathF.Abs(translation));
                    active = true;
                }
                else if (translation <= LowerTranslation)
                {
                    C2 = MathFng.Clamp(translation - LowerTranslation + Constants.LinearSlop, -Constants.MaxLinearCorrection, 0f);
                    linearError = MathF.Max(linearError, LowerTranslation - translation);
                    active = true;
                }
                else if (translation >= UpperTranslation)
                {
                    C2 = MathFng.Clamp(translation - UpperTranslation - Constants.LinearSlop, 0f, Constants.MaxLinearCorrection);
                    linearError = MathF.Max(linearError, translation - UpperTranslation);
                    active = true;
                }
            }

            Vec3 impulse;
            if (active)
            {
                float k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
                float k12 = iA * s1 + iB * s2;
                float k13 = iA * s1 * a1 + iB * s2 * a2;
                float k22 = iA + iB;
                if (k22 == 0f)
                {
                    k22 = 1f;
                }
                float k23 = iA * a1 + iB * a2;
                float k33 = mA + mB + iA * a1 * a1 + iB * a2 * a2;

                Mat33 K = new Mat33(
                    new Vec3(k11, k12, k13),
                    new Vec3(k12, k22, k23),
                    new Vec3(k13, k23, k33));

                Vec3 C = new Vec3(C1.X, C1.Y, C2);
                impulse = Solve33(K, -C);
            }
            else
            {
                float k11 = mA + mB + iA * s1 * s1 + iB * s2 * s2;
                float k12 = iA * s1 + iB * s2;
                float k22 = iA + iB;
                if (k22 == 0f)
                {
                    k22 = 1f;
                }

                Mat22 K = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));
                Vec2 impulse1 = Solve22(K, -C1);
                impulse = new Vec3(impulse1.X, impulse1.Y, 0f);
            }

            Vec2 P = impulse.X * perp + impulse.Z * axis;
            float LA = impulse.X * s1 + impulse.Y + impulse.Z * a1;
            float LB = impulse.X * s2 + impulse.Y + impulse.Z * a2;

            Vec2 centerA = BodyA.GetWorldCenter() - mA * P;
            Vec2 centerB = BodyB.GetWorldCenter() + mB * P;
            float angleA = BodyA.Transform.Q.Angle - iA * LA;
            float angleB = BodyB.Transform.Q.Angle + iB * LB;
            BodyA.SetTransformFromCenter(centerA, angleA);
            BodyB.SetTransformFromCenter(centerB, angleB);

            if (linearError <= Constants.LinearSlop && angularError <= Constants.AngularSlop)
            {
                return;
            }
        }

        private static Vec2 Solve22(Mat22 m, Vec2 b)
        {
            float a11 = m.Ex.X;
            float a12 = m.Ey.X;
            float a21 = m.Ex.Y;
            float a22 = m.Ey.Y;
            float det = a11 * a22 - a12 * a21;
            if (det != 0f)
            {
                det = 1f / det;
            }
            return new Vec2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
        }

        private static Vec2 Solve22(Mat33 m, Vec2 b)
        {
            float a11 = m.Ex.X;
            float a12 = m.Ey.X;
            float a21 = m.Ex.Y;
            float a22 = m.Ey.Y;
            float det = a11 * a22 - a12 * a21;
            if (det != 0f)
            {
                det = 1f / det;
            }
            return new Vec2(det * (a22 * b.X - a12 * b.Y), det * (a11 * b.Y - a21 * b.X));
        }

        private static Vec3 Solve33(Mat33 m, Vec3 b)
        {
            Vec3 ex = m.Ex;
            Vec3 ey = m.Ey;
            Vec3 ez = m.Ez;
            Vec3 cx = Vec3.Cross(ey, ez);
            float det = Vec3.Dot(ex, cx);
            if (det != 0f)
            {
                det = 1f / det;
            }
            float x = det * Vec3.Dot(b, cx);
            cx = Vec3.Cross(b, ez);
            float y = det * Vec3.Dot(ex, cx);
            cx = Vec3.Cross(ey, b);
            float z = det * Vec3.Dot(ex, cx);
            return new Vec3(x, y, z);
        }
    }
}
