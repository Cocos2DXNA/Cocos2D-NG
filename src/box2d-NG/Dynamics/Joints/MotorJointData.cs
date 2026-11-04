namespace Box2DNG
{
    public struct MotorJointData
    {
        public int Id;
        public int BodyA;
        public int BodyB;
        public bool CollideConnected;

        public Vec2 LinearOffset;
        public float AngularOffset;
        public float MaxForce;
        public float MaxTorque;
        public float CorrectionFactor;

        public Mat22 LinearMass;
        public float AngularMass;
        public Vec2 LinearImpulse;
        public float AngularImpulse;
        public Vec2 RA;
        public Vec2 RB;
        public Vec2 LinearBias;
        public float AngularBias;
    }
}
