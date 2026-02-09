using System;

namespace Box2DNG
{
    public sealed class GearJoint
    {
        private enum JointKind
        {
            Revolute,
            Prismatic
        }

        public Body BodyA { get; }
        public Body BodyB { get; }
        public Body BodyC { get; }
        public Body BodyD { get; }
        public float Ratio { get; }

        private readonly JointKind _typeA;
        private readonly JointKind _typeB;
        private readonly RevoluteJoint? _revoluteA;
        private readonly RevoluteJoint? _revoluteB;
        private readonly PrismaticJoint? _prismaticA;
        private readonly PrismaticJoint? _prismaticB;

        private float _constant;
        private float _mass;

        private Vec2 _jvA;
        private float _jwA;
        private Vec2 _jvB;
        private float _jwB;
        private Vec2 _jvC;
        private float _jwC;
        private Vec2 _jvD;
        private float _jwD;

        public GearJoint(GearJointDef def)
        {
            if (def.JointA is RevoluteJoint revA)
            {
                _typeA = JointKind.Revolute;
                _revoluteA = revA;
                BodyA = revA.BodyA;
                BodyB = revA.BodyB;
            }
            else if (def.JointA is PrismaticJoint priA)
            {
                _typeA = JointKind.Prismatic;
                _prismaticA = priA;
                BodyA = priA.BodyA;
                BodyB = priA.BodyB;
            }
            else
            {
                throw new ArgumentException("JointA must be RevoluteJoint or PrismaticJoint.", nameof(def));
            }

            if (def.JointB is RevoluteJoint revB)
            {
                _typeB = JointKind.Revolute;
                _revoluteB = revB;
                BodyC = revB.BodyA;
                BodyD = revB.BodyB;
            }
            else if (def.JointB is PrismaticJoint priB)
            {
                _typeB = JointKind.Prismatic;
                _prismaticB = priB;
                BodyC = priB.BodyA;
                BodyD = priB.BodyB;
            }
            else
            {
                throw new ArgumentException("JointB must be RevoluteJoint or PrismaticJoint.", nameof(def));
            }

            Ratio = def.Ratio;

            float coordA = GetCoordinateA();
            float coordB = GetCoordinateB();
            _constant = coordA + Ratio * coordB;
        }

        internal void InitVelocityConstraints(float dt)
        {
            _jvA = Vec2.Zero;
            _jvB = Vec2.Zero;
            _jvC = Vec2.Zero;
            _jvD = Vec2.Zero;
            _jwA = 0f;
            _jwB = 0f;
            _jwC = 0f;
            _jwD = 0f;

            float mass = 0f;

            if (_typeA == JointKind.Revolute && _revoluteA != null)
            {
                _jwA = -1f;
                _jwB = 1f;
                mass += BodyA.InverseInertia + BodyB.InverseInertia;
            }
            else if (_typeA == JointKind.Prismatic && _prismaticA != null)
            {
                Vec2 axis = Rot.Mul(BodyA.Transform.Q, _prismaticA.LocalAxisA);
                Vec2 rA = Rot.Mul(BodyA.Transform.Q, _prismaticA.LocalAnchorA - BodyA.LocalCenter);
                Vec2 rB = Rot.Mul(BodyB.Transform.Q, _prismaticA.LocalAnchorB - BodyB.LocalCenter);
                Vec2 d = (BodyB.GetWorldCenter() + rB) - (BodyA.GetWorldCenter() + rA);
                float a1 = Vec2.Cross(d + rA, axis);
                float a2 = Vec2.Cross(rB, axis);

                _jvA = -axis;
                _jwA = -a1;
                _jvB = axis;
                _jwB = a2;
                mass += BodyA.InverseMass + BodyB.InverseMass + BodyA.InverseInertia * a1 * a1 + BodyB.InverseInertia * a2 * a2;
            }

            if (_typeB == JointKind.Revolute && _revoluteB != null)
            {
                _jwC = -Ratio;
                _jwD = Ratio;
                mass += Ratio * Ratio * (BodyC.InverseInertia + BodyD.InverseInertia);
            }
            else if (_typeB == JointKind.Prismatic && _prismaticB != null)
            {
                Vec2 axis = Rot.Mul(BodyC.Transform.Q, _prismaticB.LocalAxisA);
                Vec2 rC = Rot.Mul(BodyC.Transform.Q, _prismaticB.LocalAnchorA - BodyC.LocalCenter);
                Vec2 rD = Rot.Mul(BodyD.Transform.Q, _prismaticB.LocalAnchorB - BodyD.LocalCenter);
                Vec2 d = (BodyD.GetWorldCenter() + rD) - (BodyC.GetWorldCenter() + rC);
                float a1 = Vec2.Cross(d + rC, axis);
                float a2 = Vec2.Cross(rD, axis);

                _jvC = -Ratio * axis;
                _jwC = -Ratio * a1;
                _jvD = Ratio * axis;
                _jwD = Ratio * a2;
                mass += Ratio * Ratio * (BodyC.InverseMass + BodyD.InverseMass + BodyC.InverseInertia * a1 * a1 + BodyD.InverseInertia * a2 * a2);
            }

            _mass = mass > 0f ? 1f / mass : 0f;
        }

        internal void SolveVelocityConstraints(float dt)
        {
            float Cdot = Vec2.Dot(_jvA, BodyA.LinearVelocity) + _jwA * BodyA.AngularVelocity +
                         Vec2.Dot(_jvB, BodyB.LinearVelocity) + _jwB * BodyB.AngularVelocity +
                         Vec2.Dot(_jvC, BodyC.LinearVelocity) + _jwC * BodyC.AngularVelocity +
                         Vec2.Dot(_jvD, BodyD.LinearVelocity) + _jwD * BodyD.AngularVelocity;

            float impulse = -_mass * Cdot;

            BodyA.LinearVelocity += BodyA.InverseMass * impulse * _jvA;
            BodyA.AngularVelocity += BodyA.InverseInertia * impulse * _jwA;
            BodyB.LinearVelocity += BodyB.InverseMass * impulse * _jvB;
            BodyB.AngularVelocity += BodyB.InverseInertia * impulse * _jwB;
            BodyC.LinearVelocity += BodyC.InverseMass * impulse * _jvC;
            BodyC.AngularVelocity += BodyC.InverseInertia * impulse * _jwC;
            BodyD.LinearVelocity += BodyD.InverseMass * impulse * _jvD;
            BodyD.AngularVelocity += BodyD.InverseInertia * impulse * _jwD;
        }

        internal void SolvePositionConstraints()
        {
            float C = (GetCoordinateA() + Ratio * GetCoordinateB()) - _constant;
            float impulse = -_mass * C;

            Vec2 centerA = BodyA.GetWorldCenter() + BodyA.InverseMass * impulse * _jvA;
            Vec2 centerB = BodyB.GetWorldCenter() + BodyB.InverseMass * impulse * _jvB;
            Vec2 centerC = BodyC.GetWorldCenter() + BodyC.InverseMass * impulse * _jvC;
            Vec2 centerD = BodyD.GetWorldCenter() + BodyD.InverseMass * impulse * _jvD;
            float angleA = BodyA.Transform.Q.Angle + BodyA.InverseInertia * impulse * _jwA;
            float angleB = BodyB.Transform.Q.Angle + BodyB.InverseInertia * impulse * _jwB;
            float angleC = BodyC.Transform.Q.Angle + BodyC.InverseInertia * impulse * _jwC;
            float angleD = BodyD.Transform.Q.Angle + BodyD.InverseInertia * impulse * _jwD;
            BodyA.SetTransformFromCenter(centerA, angleA);
            BodyB.SetTransformFromCenter(centerB, angleB);
            BodyC.SetTransformFromCenter(centerC, angleC);
            BodyD.SetTransformFromCenter(centerD, angleD);
        }

        private float GetCoordinateA()
        {
            if (_typeA == JointKind.Revolute && _revoluteA != null)
            {
                return (BodyB.Transform.Q.Angle - BodyA.Transform.Q.Angle) - _revoluteA.ReferenceAngle;
            }

            if (_typeA == JointKind.Prismatic && _prismaticA != null)
            {
                Vec2 axis = Rot.Mul(BodyA.Transform.Q, _prismaticA.LocalAxisA);
                Vec2 rA = Rot.Mul(BodyA.Transform.Q, _prismaticA.LocalAnchorA - BodyA.LocalCenter);
                Vec2 rB = Rot.Mul(BodyB.Transform.Q, _prismaticA.LocalAnchorB - BodyB.LocalCenter);
                Vec2 d = (BodyB.GetWorldCenter() + rB) - (BodyA.GetWorldCenter() + rA);
                return Vec2.Dot(axis, d);
            }

            return 0f;
        }

        private float GetCoordinateB()
        {
            if (_typeB == JointKind.Revolute && _revoluteB != null)
            {
                return (BodyD.Transform.Q.Angle - BodyC.Transform.Q.Angle) - _revoluteB.ReferenceAngle;
            }

            if (_typeB == JointKind.Prismatic && _prismaticB != null)
            {
                Vec2 axis = Rot.Mul(BodyC.Transform.Q, _prismaticB.LocalAxisA);
                Vec2 rC = Rot.Mul(BodyC.Transform.Q, _prismaticB.LocalAnchorA - BodyC.LocalCenter);
                Vec2 rD = Rot.Mul(BodyD.Transform.Q, _prismaticB.LocalAnchorB - BodyD.LocalCenter);
                Vec2 d = (BodyD.GetWorldCenter() + rD) - (BodyC.GetWorldCenter() + rC);
                return Vec2.Dot(axis, d);
            }

            return 0f;
        }
    }
}
