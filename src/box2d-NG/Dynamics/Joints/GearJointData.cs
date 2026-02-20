namespace Box2DNG
{
    public enum GearJointKind : byte
    {
        Revolute = 0,
        Prismatic = 1
    }

    public struct GearJointData
    {
        public int Id;
        public bool CollideConnected;

        public int BodyA;
        public int BodyB;
        public int BodyC;
        public int BodyD;

        public GearJointKind TypeA;
        public GearJointKind TypeB;
        public int JointAId;
        public int JointBId;

        public float Ratio;
        public float Constant;
        public float Mass;

        public Vec2 JV_A;
        public float JW_A;
        public Vec2 JV_B;
        public float JW_B;
        public Vec2 JV_C;
        public float JW_C;
        public Vec2 JV_D;
        public float JW_D;
    }
}
