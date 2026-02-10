using System;

namespace Box2DNG
{
    public sealed class DistanceJoint
    {
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public float Length { get; private set; }
        public float FrequencyHz { get; }
        public float DampingRatio { get; }
        public bool CollideConnected { get; }

        private float _impulse;
        private float _mass;
        private float _gamma;
        private float _bias;
        private Vec2 _u;

        public DistanceJoint(DistanceJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            Length = def.Length;
            FrequencyHz = def.FrequencyHz;
            DampingRatio = def.DampingRatio;
            CollideConnected = def.CollideConnected;
        }

        internal void InitVelocityConstraints(float dt)
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            Vec2 pA = BodyA.GetWorldCenter() + rA;
            Vec2 pB = BodyB.GetWorldCenter() + rB;
            Vec2 d = pB - pA;
            float length = d.Length;
            _u = length > Constants.Epsilon ? d / length : new Vec2(1f, 0f);

            float crA = Vec2.Cross(rA, _u);
            float crB = Vec2.Cross(rB, _u);
            float invMass = BodyA.InverseMass + BodyB.InverseMass + BodyA.InverseInertia * crA * crA + BodyB.InverseInertia * crB * crB;
            _mass = invMass > 0f ? 1f / invMass : 0f;

            if (FrequencyHz > 0f)
            {
                float C = length - Length;
                float omega = 2f * MathF.PI * FrequencyHz;
                float dmp = 2f * _mass * DampingRatio * omega;
                float k = _mass * omega * omega;
                _gamma = dt * (dmp + dt * k);
                _gamma = _gamma != 0f ? 1f / _gamma : 0f;
                _bias = C * dt * k * _gamma;
                _mass = 1f / (invMass + _gamma);
            }
            else
            {
                _gamma = 0f;
                _bias = 0f;
            }

            if (_impulse != 0f)
            {
                Vec2 P = _impulse * _u;
                BodyA.LinearVelocity -= BodyA.InverseMass * P;
                BodyA.AngularVelocity -= BodyA.InverseInertia * Vec2.Cross(rA, P);
                BodyB.LinearVelocity += BodyB.InverseMass * P;
                BodyB.AngularVelocity += BodyB.InverseInertia * Vec2.Cross(rB, P);
            }
        }

        internal void SolveVelocityConstraints()
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            Vec2 vA = BodyA.LinearVelocity + Vec2.Cross(BodyA.AngularVelocity, rA);
            Vec2 vB = BodyB.LinearVelocity + Vec2.Cross(BodyB.AngularVelocity, rB);

            float Cdot = Vec2.Dot(_u, vB - vA);
            float impulse = -_mass * (Cdot + _bias + _gamma * _impulse);
            _impulse += impulse;
            Vec2 P = impulse * _u;

            BodyA.LinearVelocity -= BodyA.InverseMass * P;
            BodyA.AngularVelocity -= BodyA.InverseInertia * Vec2.Cross(rA, P);
            BodyB.LinearVelocity += BodyB.InverseMass * P;
            BodyB.AngularVelocity += BodyB.InverseInertia * Vec2.Cross(rB, P);
        }

        internal void SolvePositionConstraints()
        {
            if (FrequencyHz > 0f)
            {
                return;
            }

            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
            Vec2 pA = BodyA.GetWorldCenter() + rA;
            Vec2 pB = BodyB.GetWorldCenter() + rB;
            Vec2 d = pB - pA;
            float length = d.Length;
            Vec2 u = length > Constants.Epsilon ? d / length : new Vec2(1f, 0f);
            float C = length - Length;
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
