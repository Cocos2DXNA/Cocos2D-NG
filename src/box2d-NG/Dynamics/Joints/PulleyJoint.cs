using System;

namespace Box2DNG
{
    public sealed class PulleyJoint
    {
        public Body BodyA { get; }
        public Body BodyB { get; }
        public Vec2 GroundAnchorA { get; }
        public Vec2 GroundAnchorB { get; }
        public Vec2 LocalAnchorA { get; }
        public Vec2 LocalAnchorB { get; }
        public float LengthA { get; private set; }
        public float LengthB { get; private set; }
        public float Ratio { get; }
        public bool CollideConnected { get; }

        private Vec2 _uA;
        private Vec2 _uB;
        private float _impulse;
        private float _mass;

        public PulleyJoint(PulleyJointDef def)
        {
            BodyA = def.BodyA;
            BodyB = def.BodyB;
            GroundAnchorA = def.GroundAnchorA;
            GroundAnchorB = def.GroundAnchorB;
            LocalAnchorA = def.LocalAnchorA;
            LocalAnchorB = def.LocalAnchorB;
            LengthA = def.LengthA;
            LengthB = def.LengthB;
            Ratio = def.Ratio;
            CollideConnected = def.CollideConnected;
        }

        internal void InitVelocityConstraints(float dt)
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            Vec2 pA = BodyA.GetWorldCenter() + rA;
            Vec2 pB = BodyB.GetWorldCenter() + rB;

            _uA = pA - GroundAnchorA;
            _uB = pB - GroundAnchorB;

            float lengthA = _uA.Length;
            float lengthB = _uB.Length;

            if (lengthA > Constants.LinearSlop)
            {
                _uA = _uA / lengthA;
            }
            else
            {
                _uA = Vec2.Zero;
            }

            if (lengthB > Constants.LinearSlop)
            {
                _uB = _uB / lengthB;
            }
            else
            {
                _uB = Vec2.Zero;
            }

            float mA = BodyA.InverseMass;
            float mB = BodyB.InverseMass;
            float iA = BodyA.InverseInertia;
            float iB = BodyB.InverseInertia;

            float crA = Vec2.Cross(rA, _uA);
            float crB = Vec2.Cross(rB, _uB);

            float invMass = mA + iA * crA * crA + Ratio * Ratio * (mB + iB * crB * crB);
            _mass = invMass > 0f ? 1f / invMass : 0f;
        }

        internal void SolveVelocityConstraints()
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);

            Vec2 vA = BodyA.LinearVelocity + Vec2.Cross(BodyA.AngularVelocity, rA);
            Vec2 vB = BodyB.LinearVelocity + Vec2.Cross(BodyB.AngularVelocity, rB);

            float Cdot = -Vec2.Dot(_uA, vA) - Ratio * Vec2.Dot(_uB, vB);
            float impulse = -_mass * Cdot;
            _impulse += impulse;

            Vec2 PA = -impulse * _uA;
            Vec2 PB = -Ratio * impulse * _uB;

            BodyA.LinearVelocity += BodyA.InverseMass * PA;
            BodyA.AngularVelocity += BodyA.InverseInertia * Vec2.Cross(rA, PA);
            BodyB.LinearVelocity += BodyB.InverseMass * PB;
            BodyB.AngularVelocity += BodyB.InverseInertia * Vec2.Cross(rB, PB);
        }

        internal void SolvePositionConstraints()
        {
            Vec2 rA = Rot.Mul(BodyA.Transform.Q, LocalAnchorA - BodyA.LocalCenter);
            Vec2 rB = Rot.Mul(BodyB.Transform.Q, LocalAnchorB - BodyB.LocalCenter);
            Vec2 pA = BodyA.GetWorldCenter() + rA;
            Vec2 pB = BodyB.GetWorldCenter() + rB;

            Vec2 uA = pA - GroundAnchorA;
            Vec2 uB = pB - GroundAnchorB;
            float lengthA = uA.Length;
            float lengthB = uB.Length;

            if (lengthA > Constants.LinearSlop)
            {
                uA = uA / lengthA;
            }
            else
            {
                uA = Vec2.Zero;
            }

            if (lengthB > Constants.LinearSlop)
            {
                uB = uB / lengthB;
            }
            else
            {
                uB = Vec2.Zero;
            }

            float target = LengthA + Ratio * LengthB;
            float C = target - (lengthA + Ratio * lengthB);
            float impulse = -_mass * C;

            Vec2 PA = -impulse * uA;
            Vec2 PB = -Ratio * impulse * uB;

            Vec2 centerA = BodyA.GetWorldCenter() + BodyA.InverseMass * PA;
            Vec2 centerB = BodyB.GetWorldCenter() + BodyB.InverseMass * PB;
            float angleA = BodyA.Transform.Q.Angle + BodyA.InverseInertia * Vec2.Cross(rA, PA);
            float angleB = BodyB.Transform.Q.Angle + BodyB.InverseInertia * Vec2.Cross(rB, PB);
            BodyA.SetTransformFromCenter(centerA, angleA);
            BodyB.SetTransformFromCenter(centerB, angleB);
        }
    }
}
