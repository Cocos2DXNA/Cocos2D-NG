namespace Box2DNG
{
    public struct FrictionJointData
    {
        public int Id;
        public int BodyA;
        public int BodyB;
        public bool CollideConnected;

        public Vec2 LocalAnchorA;
        public Vec2 LocalAnchorB;
        public float MaxForce;
        public float MaxTorque;

        public Vec2 RA;
        public Vec2 RB;
        public Mat22 LinearMass;
        public float AngularMass;
        public Vec2 LinearImpulse;
        public float AngularImpulse;
    }
}
