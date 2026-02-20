namespace Box2DNG
{
    public struct WeldJointData
    {
        public int Id;
        public int BodyA;
        public int BodyB;
        public bool CollideConnected;

        public Vec2 LocalAnchorA;
        public Vec2 LocalAnchorB;
        public float ReferenceAngle;

        public Mat22 LinearMass;
        public float AngularMass;
        public Vec2 Impulse;
        public float AngularImpulse;
        public Vec2 RA;
        public Vec2 RB;
    }
}
