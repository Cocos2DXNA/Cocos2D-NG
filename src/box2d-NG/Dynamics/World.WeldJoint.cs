namespace Box2DNG
{
    public sealed partial class World
    {
        internal void InitWeldJointVelocityConstraints(int index, float dt)
        {
            ref WeldJointData joint = ref _weldJointsData[index];
            int indexA = joint.BodyA;
            int indexB = joint.BodyB;

            joint.RA = Rot.Mul(_bodyRotations[indexA], joint.LocalAnchorA - _bodyLocalCenters[indexA]);
            joint.RB = Rot.Mul(_bodyRotations[indexB], joint.LocalAnchorB - _bodyLocalCenters[indexB]);

            float mA = _bodyInverseMasses[indexA];
            float mB = _bodyInverseMasses[indexB];
            float iA = _bodyInverseInertias[indexA];
            float iB = _bodyInverseInertias[indexB];

            float k11 = mA + mB + iA * joint.RA.Y * joint.RA.Y + iB * joint.RB.Y * joint.RB.Y;
            float k12 = -iA * joint.RA.X * joint.RA.Y - iB * joint.RB.X * joint.RB.Y;
            float k22 = mA + mB + iA * joint.RA.X * joint.RA.X + iB * joint.RB.X * joint.RB.X;
            joint.LinearMass = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));

            float angularMass = iA + iB;
            joint.AngularMass = angularMass > 0f ? 1f / angularMass : 0f;
        }

        internal void SolveWeldJointVelocityConstraints(int index, float dt)
        {
            ref WeldJointData joint = ref _weldJointsData[index];
            int indexA = joint.BodyA;
            int indexB = joint.BodyB;

            float mA = _bodyInverseMasses[indexA];
            float mB = _bodyInverseMasses[indexB];
            float iA = _bodyInverseInertias[indexA];
            float iB = _bodyInverseInertias[indexB];

            ref Vec2 vA = ref _bodyLinearVelocities[indexA];
            ref Vec2 vB = ref _bodyLinearVelocities[indexB];
            ref float wA = ref _bodyAngularVelocities[indexA];
            ref float wB = ref _bodyAngularVelocities[indexB];

            Vec2 Cdot = (vB + Vec2.Cross(wB, joint.RB)) - (vA + Vec2.Cross(wA, joint.RA));
            Vec2 impulse = Solve22(joint.LinearMass, -Cdot);
            joint.Impulse += impulse;

            vA -= mA * impulse;
            wA -= iA * Vec2.Cross(joint.RA, impulse);
            vB += mB * impulse;
            wB += iB * Vec2.Cross(joint.RB, impulse);

            float CdotAngle = wB - wA;
            float angularImpulse = -joint.AngularMass * CdotAngle;
            joint.AngularImpulse += angularImpulse;

            wA -= iA * angularImpulse;
            wB += iB * angularImpulse;
        }

        internal void SolveWeldJointPositionConstraints(int index)
        {
            ref WeldJointData joint = ref _weldJointsData[index];
            int indexA = joint.BodyA;
            int indexB = joint.BodyB;

            ref Vec2 cA = ref _bodyPositions[indexA];
            ref Vec2 cB = ref _bodyPositions[indexB];
            float aA = _bodyRotations[indexA].Angle;
            float aB = _bodyRotations[indexB].Angle;

            float mA = _bodyInverseMasses[indexA];
            float mB = _bodyInverseMasses[indexB];
            float iA = _bodyInverseInertias[indexA];
            float iB = _bodyInverseInertias[indexB];

            Vec2 rA = Rot.Mul(new Rot(aA), joint.LocalAnchorA - _bodyLocalCenters[indexA]);
            Vec2 rB = Rot.Mul(new Rot(aB), joint.LocalAnchorB - _bodyLocalCenters[indexB]);
            Vec2 C = (cB + rB) - (cA + rA);

            float k11 = mA + mB + iA * rA.Y * rA.Y + iB * rB.Y * rB.Y;
            float k12 = -iA * rA.X * rA.Y - iB * rB.X * rB.Y;
            float k22 = mA + mB + iA * rA.X * rA.X + iB * rB.X * rB.X;
            Mat22 k = new Mat22(new Vec2(k11, k12), new Vec2(k12, k22));
            Vec2 impulse = Solve22(k, -C);

            cA -= mA * impulse;
            cB += mB * impulse;
            aA -= iA * Vec2.Cross(rA, impulse);
            aB += iB * Vec2.Cross(rB, impulse);

            float angleError = (aB - aA) - joint.ReferenceAngle;
            float angularImpulse = -joint.AngularMass * angleError;
            aA -= iA * angularImpulse;
            aB += iB * angularImpulse;

            _bodyRotations[indexA] = new Rot(aA);
            _bodyRotations[indexB] = new Rot(aB);
        }
    }
}
