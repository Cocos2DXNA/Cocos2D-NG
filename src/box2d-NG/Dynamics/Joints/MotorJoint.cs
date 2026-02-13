using System;

namespace Box2DNG
{
    public sealed class MotorJoint
    {
        public int Id { get; internal set; } = -1;
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LinearOffset { get; }
        public float AngularOffset { get; }
        public float MaxForce { get; }
        public float MaxTorque { get; }
        public float CorrectionFactor { get; }
        public bool CollideConnected { get; }

        private Mat22 _linearMass;
        private float _angularMass;
        private Vec2 _linearImpulse;
        private float _angularImpulse;
        private Vec2 _rA;
        private Vec2 _rB;
        private Vec2 _linearBias;
        private float _angularBias;

        public MotorJoint(MotorJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LinearOffset = def.LinearOffset;
            AngularOffset = def.AngularOffset;
            MaxForce = def.MaxForce;
            MaxTorque = def.MaxTorque;
            CorrectionFactor = def.CorrectionFactor;
            CollideConnected = def.CollideConnected;
        }

        internal void InitVelocityConstraints(float dt)
        {
            _rA = Vec2.Zero;
            _rB = Vec2.Zero;

            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            float k11 = mA + mB;
            float k12 = 0f;
            float k22 = mA + mB;
            _linearMass = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));

            float angularMass = iA + iB;
            _angularMass = angularMass > 0f ? 1f / angularMass : 0f;

            Vec2 target = BodyA.GetWorldCenter() + Rot.Mul(BodyA.Transform.Q, LinearOffset);
            Vec2 C = (BodyB.GetWorldCenter() - target);
            _linearBias = (CorrectionFactor / dt) * C;

            float angleError = (BodyB.Transform.Q.Angle - BodyA.Transform.Q.Angle) - AngularOffset;
            _angularBias = (CorrectionFactor / dt) * angleError;
        }

        internal void SolveVelocityConstraints(float dt)
        {
            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            Vec2 vA = BodyA.LinearVelocity;
            Vec2 vB = BodyB.LinearVelocity;
            float wA = BodyA.AngularVelocity;
            float wB = BodyB.AngularVelocity;

            Vec2 Cdot = vB - vA;
            Vec2 impulse = Solve22(_linearMass, -Cdot - _linearBias);

            Vec2 oldImpulse = _linearImpulse;
            _linearImpulse += impulse;
            float maxImpulse = MaxForce * dt;
            if (_linearImpulse.Length > maxImpulse && maxImpulse > 0f)
            {
                _linearImpulse = _linearImpulse.Normalize() * maxImpulse;
            }
            impulse = _linearImpulse - oldImpulse;

            vA -= mA * impulse;
            vB += mB * impulse;

            float CdotAngle = wB - wA;
            float angularImpulse = -_angularMass * (CdotAngle + _angularBias);
            float oldAngular = _angularImpulse;
            _angularImpulse += angularImpulse;
            float maxAngularImpulse = MaxTorque * dt;
            _angularImpulse = MathFng.Clamp(_angularImpulse, -maxAngularImpulse, maxAngularImpulse);
            angularImpulse = _angularImpulse - oldAngular;

            wA -= iA * angularImpulse;
            wB += iB * angularImpulse;

            BodyA.LinearVelocity = vA;
            BodyA.AngularVelocity = wA;
            BodyB.LinearVelocity = vB;
            BodyB.AngularVelocity = wB;
        }

        internal void SolvePositionConstraints()
        {
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
