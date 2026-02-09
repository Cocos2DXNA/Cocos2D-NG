using System;

namespace Box2DNG
{
    public sealed class RevoluteJoint
    {
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public float ReferenceAngle { get; }
        public bool EnableMotor { get; private set; }
        public float MotorSpeed { get; private set; }
        public float MaxMotorTorque { get; private set; }
        public bool EnableLimit { get; private set; }
        public float LowerAngle { get; private set; }
        public float UpperAngle { get; private set; }

        private Vec2 _impulse;
        private float _motorImpulse;
        private float _limitImpulse;
        private Mat22 _mass;
        private float _motorMass;
        private Vec2 _rA;
        private Vec2 _rB;

        public RevoluteJoint(RevoluteJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            ReferenceAngle = def.ReferenceAngle;
            EnableMotor = def.EnableMotor;
            MotorSpeed = def.MotorSpeed;
            MaxMotorTorque = def.MaxMotorTorque;
            EnableLimit = def.EnableLimit;
            LowerAngle = def.LowerAngle;
            UpperAngle = def.UpperAngle;
        }

        public void SetMotorEnabled(bool enable) => EnableMotor = enable;

        public void SetMotorSpeed(float speed) => MotorSpeed = speed;

        public void SetMaxMotorTorque(float torque) => MaxMotorTorque = MathF.Max(0f, torque);

        public void SetLimitEnabled(bool enable) => EnableLimit = enable;

        public void SetLimits(float lowerAngle, float upperAngle)
        {
            EnableLimit = true;
            LowerAngle = MathF.Min(lowerAngle, upperAngle);
            UpperAngle = MathF.Max(lowerAngle, upperAngle);
        }

        internal void InitVelocityConstraints(float dt)
        {
            _rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            _rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            float k11 = mA + mB + iA * _rA.Y * _rA.Y + iB * _rB.Y * _rB.Y;
            float k12 = -iA * _rA.X * _rA.Y - iB * _rB.X * _rB.Y;
            float k22 = mA + mB + iA * _rA.X * _rA.X + iB * _rB.X * _rB.X;

            _mass = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));
            _motorMass = iA + iB;
            if (_motorMass > 0f)
            {
                _motorMass = 1f / _motorMass;
            }

            if (!EnableLimit)
            {
                _limitImpulse = 0f;
            }
            if (!EnableMotor)
            {
                _motorImpulse = 0f;
            }

            // Warm start using last step's impulses.
            if (_impulse.X != 0f || _impulse.Y != 0f || _motorImpulse != 0f || _limitImpulse != 0f)
            {
                Vec2 P = _impulse;
                BodyA.LinearVelocity -= mA * P;
                BodyA.AngularVelocity -= iA * (Vec2.Cross(_rA, P) + _motorImpulse + _limitImpulse);
                BodyB.LinearVelocity += mB * P;
                BodyB.AngularVelocity += iB * (Vec2.Cross(_rB, P) + _motorImpulse + _limitImpulse);
            }
        }

        internal void SolveVelocityConstraints(float dt)
        {
            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            if (EnableMotor)
            {
                float Cdot = BodyB.AngularVelocity - BodyA.AngularVelocity - MotorSpeed;
                float impulse = -_motorMass * Cdot;
                float maxImpulse = MaxMotorTorque * dt;
                float newImpulse = MathFng.Clamp(_motorImpulse + impulse, -maxImpulse, maxImpulse);
                float applied = newImpulse - _motorImpulse;
                _motorImpulse = newImpulse;

                BodyA.AngularVelocity -= iA * applied;
                BodyB.AngularVelocity += iB * applied;
            }

            if (EnableLimit)
            {
                float angle = (BodyB.Transform.Q.Angle - BodyA.Transform.Q.Angle) - ReferenceAngle;
                if (angle < LowerAngle)
                {
                    float limitC = angle - LowerAngle;
                    float Cdot = BodyB.AngularVelocity - BodyA.AngularVelocity;
                    float limitImpulse = -_motorMass * (Cdot + MathF.Min(0f, limitC) * 0.2f);
                    float prevImpulse = _limitImpulse;
                    _limitImpulse = MathF.Min(prevImpulse + limitImpulse, 0f);
                    limitImpulse = _limitImpulse - prevImpulse;

                    BodyA.AngularVelocity -= iA * limitImpulse;
                    BodyB.AngularVelocity += iB * limitImpulse;
                }
                else if (angle > UpperAngle)
                {
                    float limitC = angle - UpperAngle;
                    float Cdot = BodyB.AngularVelocity - BodyA.AngularVelocity;
                    float limitImpulse = -_motorMass * (Cdot + MathF.Max(0f, limitC) * 0.2f);
                    float prevImpulse = _limitImpulse;
                    _limitImpulse = MathF.Max(prevImpulse + limitImpulse, 0f);
                    limitImpulse = _limitImpulse - prevImpulse;

                    BodyA.AngularVelocity -= iA * limitImpulse;
                    BodyB.AngularVelocity += iB * limitImpulse;
                }
                else
                {
                    _limitImpulse = 0f;
                }
            }

            Vec2 vA = BodyA.LinearVelocity + Vec2.Cross(BodyA.AngularVelocity, _rA);
            Vec2 vB = BodyB.LinearVelocity + Vec2.Cross(BodyB.AngularVelocity, _rB);
            Vec2 Cdot2 = vB - vA;

            Vec2 impulse2 = Solve22(_mass, -Cdot2);
            _impulse += impulse2;

            BodyA.LinearVelocity -= mA * impulse2;
            BodyB.LinearVelocity += mB * impulse2;
            BodyA.AngularVelocity -= iA * Vec2.Cross(_rA, impulse2);
            BodyB.AngularVelocity += iB * Vec2.Cross(_rB, impulse2);
        }

        internal void SolvePositionConstraints()
        {
            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            if (EnableLimit)
            {
                float angle = (BodyB.Transform.Q.Angle - BodyA.Transform.Q.Angle) - ReferenceAngle;
                float limitC = 0f;
                if (angle < LowerAngle)
                {
                    limitC = angle - LowerAngle;
                }
                else if (angle > UpperAngle)
                {
                    limitC = angle - UpperAngle;
                }

                if (limitC != 0f)
                {
                    float limitImpulse = -_motorMass * limitC;
                    float newAngleA = BodyA.Transform.Q.Angle - iA * limitImpulse;
                    float newAngleB = BodyB.Transform.Q.Angle + iB * limitImpulse;
                    BodyA.SetTransformFromCenter(BodyA.GetWorldCenter(), newAngleA);
                    BodyB.SetTransformFromCenter(BodyB.GetWorldCenter(), newAngleB);
                }
            }

            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
            Vec2 pA = BodyA.GetWorldCenter() + rA;
            Vec2 pB = BodyB.GetWorldCenter() + rB;
            Vec2 positionC = pB - pA;

            float k11 = mA + mB + iA * rA.Y * rA.Y + iB * rB.Y * rB.Y;
            float k12 = -iA * rA.X * rA.Y - iB * rB.X * rB.Y;
            float k22 = mA + mB + iA * rA.X * rA.X + iB * rB.X * rB.X;
            Mat22 k = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));
            Vec2 positionImpulse = Solve22(k, -positionC);

            Vec2 centerA = BodyA.GetWorldCenter() - mA * positionImpulse;
            Vec2 centerB = BodyB.GetWorldCenter() + mB * positionImpulse;
            float angleA = BodyA.Transform.Q.Angle - iA * Vec2.Cross(rA, positionImpulse);
            float angleB = BodyB.Transform.Q.Angle + iB * Vec2.Cross(rB, positionImpulse);
            BodyA.SetTransformFromCenter(centerA, angleA);
            BodyB.SetTransformFromCenter(centerB, angleB);
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
    }
}
