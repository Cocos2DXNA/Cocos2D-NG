using System;

namespace Box2DNG
{
    public sealed class PrismaticJoint
    {
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public Vec2 LocalAxisA { get; }
        public float ReferenceAngle { get; }
        public bool EnableLimit { get; }
        public float LowerTranslation { get; }
        public float UpperTranslation { get; }
        public bool EnableMotor { get; }
        public float MotorSpeed { get; }
        public float MaxMotorForce { get; }

        private Mat33 _k;
        private Vec2 _perp;
        private Vec2 _axis;
        private float _s1;
        private float _s2;
        private float _a1;
        private float _a2;
        private Vec3 _impulse;
        private float _motorImpulse;
        private float _motorMass;
        private LimitState _limitState;

        private enum LimitState
        {
            Inactive,
            AtLower,
            AtUpper,
            Equal
        }

        public PrismaticJoint(PrismaticJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            LocalAxisA = def.LocalAxisA.Normalize();
            ReferenceAngle = def.ReferenceAngle;
            EnableLimit = def.EnableLimit;
            LowerTranslation = def.LowerTranslation;
            UpperTranslation = def.UpperTranslation;
            EnableMotor = def.EnableMotor;
            MotorSpeed = def.MotorSpeed;
            MaxMotorForce = def.MaxMotorForce;
            _limitState = LimitState.Inactive;
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
            float k13 = iA * _s1 * _a1 + iB * _s2 * _a2;
            float k22 = iA + iB;
            if (k22 == 0f)
            {
                // For bodies with fixed rotation, keep the matrix invertible.
                k22 = 1f;
            }
            float k23 = iA * _a1 + iB * _a2;
            float k33 = mA + mB + iA * _a1 * _a1 + iB * _a2 * _a2;
            _k = new Mat33(
                new Vec3(k11, k12, k13),
                new Vec3(k12, k22, k23),
                new Vec3(k13, k23, k33));

            float motorMass = mA + mB + iA * _a1 * _a1 + iB * _a2 * _a2;
            _motorMass = motorMass > 0f ? 1f / motorMass : 0f;

            if (EnableLimit)
            {
                float translation = Vec2.Dot(_axis, d);
                if (MathF.Abs(UpperTranslation - LowerTranslation) < 2f * Constants.LinearSlop)
                {
                    _limitState = LimitState.Equal;
                }
                else if (translation <= LowerTranslation)
                {
                    if (_limitState != LimitState.AtLower)
                    {
                        _impulse = new Vec3(_impulse.X, _impulse.Y, 0f);
                    }
                    _limitState = LimitState.AtLower;
                }
                else if (translation >= UpperTranslation)
                {
                    if (_limitState != LimitState.AtUpper)
                    {
                        _impulse = new Vec3(_impulse.X, _impulse.Y, 0f);
                    }
                    _limitState = LimitState.AtUpper;
                }
                else
                {
                    _limitState = LimitState.Inactive;
                    _impulse = new Vec3(_impulse.X, _impulse.Y, 0f);
                }
            }
            else
            {
                _limitState = LimitState.Inactive;
                _impulse = new Vec3(_impulse.X, _impulse.Y, 0f);
            }

            if (!EnableMotor)
            {
                _motorImpulse = 0f;
            }

            if (_impulse.X != 0f || _impulse.Y != 0f || _impulse.Z != 0f || _motorImpulse != 0f)
            {
                Vec2 P = _impulse.X * _perp + (_motorImpulse + _impulse.Z) * _axis;
                float LA = _impulse.X * _s1 + _impulse.Y + (_motorImpulse + _impulse.Z) * _a1;
                float LB = _impulse.X * _s2 + _impulse.Y + (_motorImpulse + _impulse.Z) * _a2;

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

            // Solve linear motor constraint.
            if (EnableMotor && _limitState != LimitState.Equal)
            {
                float Cdot = Vec2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                float impulse = _motorMass * (MotorSpeed - Cdot);
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

            Vec2 Cdot1 = new Vec2(
                Vec2.Dot(_perp, vB - vA) + _s2 * wB - _s1 * wA,
                wB - wA);

            if (EnableLimit && _limitState != LimitState.Inactive)
            {
                float Cdot2 = Vec2.Dot(_axis, vB - vA) + _a2 * wB - _a1 * wA;
                Vec3 Cdot = new Vec3(Cdot1.X, Cdot1.Y, Cdot2);

                Vec3 f1 = _impulse;
                Vec3 df = Solve33(_k, -Cdot);
                _impulse = new Vec3(_impulse.X + df.X, _impulse.Y + df.Y, _impulse.Z + df.Z);

                if (_limitState == LimitState.AtLower)
                {
                    _impulse = new Vec3(_impulse.X, _impulse.Y, MathF.Max(_impulse.Z, 0f));
                }
                else if (_limitState == LimitState.AtUpper)
                {
                    _impulse = new Vec3(_impulse.X, _impulse.Y, MathF.Min(_impulse.Z, 0f));
                }

                Vec2 b = -Cdot1 - (_impulse.Z - f1.Z) * new Vec2(_k.Ez.X, _k.Ez.Y);
                Vec2 f2r = Solve22(_k, b) + new Vec2(f1.X, f1.Y);
                _impulse = new Vec3(f2r.X, f2r.Y, _impulse.Z);

                df = _impulse - f1;

                Vec2 P = df.X * _perp + df.Z * _axis;
                float LA = df.X * _s1 + df.Y + df.Z * _a1;
                float LB = df.X * _s2 + df.Y + df.Z * _a2;

                vA -= mA * P;
                wA -= iA * LA;
                vB += mB * P;
                wB += iB * LB;
            }
            else
            {
                Vec2 df = Solve22(_k, -Cdot1);
                _impulse = new Vec3(_impulse.X + df.X, _impulse.Y + df.Y, _impulse.Z);

                Vec2 P = df.X * _perp;
                float LA = df.X * _s1 + df.Y;
                float LB = df.X * _s2 + df.Y;

                vA -= mA * P;
                wA -= iA * LA;
                vB += mB * P;
                wB += iB * LB;
            }

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
