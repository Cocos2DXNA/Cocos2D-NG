using System;

namespace Box2DNG
{
    public sealed class RopeJoint
    {
        public int Id { get; internal set; } = -1;
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public float MaxLength { get; private set; }
        public bool CollideConnected { get; }

        private Vec2 _u;
        private Vec2 _rA;
        private Vec2 _rB;
        private float _mass;
        private float _impulse;
        private float _length;

        public RopeJoint(RopeJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            MaxLength = def.MaxLength;
            CollideConnected = def.CollideConnected;
        }

        public void SetMaxLength(float maxLength)
        {
            MaxLength = MathF.Max(Constants.LinearSlop, maxLength);
        }

        internal void InitVelocityConstraints(float dt)
        {
            _rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            _rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            Vec2 pA = BodyA.GetWorldCenter() + _rA;
            Vec2 pB = BodyB.GetWorldCenter() + _rB;
            _u = pB - pA;
            _length = _u.Length;

            if (_length > Constants.LinearSlop)
            {
                _u = _u / _length;
            }
            else
            {
                _u = Vec2.Zero;
                _mass = 0f;
                _impulse = 0f;
                return;
            }

            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            float crA = Vec2.Cross(_rA, _u);
            float crB = Vec2.Cross(_rB, _u);
            float invMass = mA + iA * crA * crA + mB + iB * crB * crB;
            _mass = invMass > 0f ? 1f / invMass : 0f;
        }

        internal void SolveVelocityConstraints(float dt)
        {
            Vec2 vA = BodyA.LinearVelocity;
            Vec2 vB = BodyB.LinearVelocity;
            float wA = BodyA.AngularVelocity;
            float wB = BodyB.AngularVelocity;

            Vec2 vpA = vA + Vec2.Cross(wA, _rA);
            Vec2 vpB = vB + Vec2.Cross(wB, _rB);

            float C = _length - MaxLength;
            float Cdot = Vec2.Dot(_u, vpB - vpA);

            if (C < 0f && dt > 0f)
            {
                Cdot += C / dt;
            }

            float impulse = -_mass * Cdot;
            float oldImpulse = _impulse;
            _impulse = MathF.Min(0f, _impulse + impulse);
            impulse = _impulse - oldImpulse;

            Vec2 P = impulse * _u;
            vA -= BodyA.InverseMass * P;
            wA -= BodyA.InverseInertia * Vec2.Cross(_rA, P);
            vB += BodyB.InverseMass * P;
            wB += BodyB.InverseInertia * Vec2.Cross(_rB, P);

            BodyA.LinearVelocity = vA;
            BodyA.AngularVelocity = wA;
            BodyB.LinearVelocity = vB;
            BodyB.AngularVelocity = wB;
        }

        internal void SolvePositionConstraints()
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
            Vec2 pA = BodyA.GetWorldCenter() + rA;
            Vec2 pB = BodyB.GetWorldCenter() + rB;
            Vec2 u = pB - pA;
            float length = u.Length;

            if (length > Constants.LinearSlop)
            {
                u = u / length;
            }
            else
            {
                return;
            }

            float C = length - MaxLength;
            C = MathFng.Clamp(C, 0f, Constants.MaxLinearCorrection);
            float impulse = -_mass * C;
            Vec2 P = impulse * u;

            Vec2 centerA = BodyA.GetWorldCenter() - BodyA.InverseMass * P;
            Vec2 centerB = BodyB.GetWorldCenter() + BodyB.InverseMass * P;
            float angleA = BodyA.Transform.Q.Angle - BodyA.InverseInertia * Vec2.Cross(rA, P);
            float angleB = BodyB.Transform.Q.Angle + BodyB.InverseInertia * Vec2.Cross(rB, P);
            BodyA.SetTransformFromCenter(centerA, angleA);
            BodyB.SetTransformFromCenter(centerB, angleB);
        }
    }
}
