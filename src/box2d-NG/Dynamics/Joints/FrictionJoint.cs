using System;

namespace Box2DNG
{
    public sealed class FrictionJoint
    {
        public int Id { get; internal set; } = -1;
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public float MaxForce { get; private set; }
        public float MaxTorque { get; private set; }
        public bool CollideConnected { get; }

        private Vec2 _rA;
        private Vec2 _rB;
        private Mat22 _linearMass;
        private float _angularMass;
        private Vec2 _linearImpulse;
        private float _angularImpulse;

        public FrictionJoint(FrictionJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            MaxForce = def.MaxForce;
            MaxTorque = def.MaxTorque;
            CollideConnected = def.CollideConnected;
        }

        public void SetMaxForce(float maxForce) => MaxForce = MathF.Max(0f, maxForce);

        public void SetMaxTorque(float maxTorque) => MaxTorque = MathF.Max(0f, maxTorque);

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
            _linearMass = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));

            float angularMass = iA + iB;
            _angularMass = angularMass > 0f ? 1f / angularMass : 0f;

            _linearImpulse = Vec2.Zero;
            _angularImpulse = 0f;
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

            float CdotAngle = wB - wA;
            float angularImpulse = -_angularMass * CdotAngle;
            float oldAngular = _angularImpulse;
            float maxAngular = MaxTorque * dt;
            _angularImpulse = MathFng.Clamp(_angularImpulse + angularImpulse, -maxAngular, maxAngular);
            angularImpulse = _angularImpulse - oldAngular;
            wA -= iA * angularImpulse;
            wB += iB * angularImpulse;

            Vec2 Cdot = vB + Vec2.Cross(wB, _rB) - vA - Vec2.Cross(wA, _rA);
            Vec2 impulse = Solve22(_linearMass, -Cdot);
            Vec2 oldImpulse = _linearImpulse;
            _linearImpulse += impulse;
            float maxImpulse = MaxForce * dt;
            if (_linearImpulse.Length > maxImpulse && maxImpulse > 0f)
            {
                _linearImpulse = _linearImpulse.Normalize() * maxImpulse;
            }
            impulse = _linearImpulse - oldImpulse;

            vA -= mA * impulse;
            wA -= iA * Vec2.Cross(_rA, impulse);
            vB += mB * impulse;
            wB += iB * Vec2.Cross(_rB, impulse);

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
