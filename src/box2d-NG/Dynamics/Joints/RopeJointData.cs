namespace Box2DNG
{
    public struct RopeJointData
    {
        public int Id;
        public int BodyA;
        public int BodyB;
        public bool CollideConnected;

        public Vec2 LocalAnchorA;
        public Vec2 LocalAnchorB;
        public float MaxLength;

        public Vec2 U;
        public Vec2 RA;
        public Vec2 RB;
        public float Mass;
        public float Impulse;
        public float Length;
    }
}
