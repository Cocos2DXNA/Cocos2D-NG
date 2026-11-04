namespace Box2DNG
{
    public struct PulleyJointData
    {
        public int Id;
        public int BodyA;
        public int BodyB;
        public bool CollideConnected;

        public Vec2 GroundAnchorA;
        public Vec2 GroundAnchorB;
        public Vec2 LocalAnchorA;
        public Vec2 LocalAnchorB;
        public float LengthA;
        public float LengthB;
        public float Ratio;

        public Vec2 UA;
        public Vec2 UB;
        public float Impulse;
        public float Mass;
    }
}
