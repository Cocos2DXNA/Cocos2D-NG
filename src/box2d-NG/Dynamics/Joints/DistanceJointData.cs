using System.Runtime.InteropServices;

namespace Box2DNG
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DistanceJointData
    {
        public int BodyA;
        public int BodyB;
        public Vec2 LocalAnchorA;
        public Vec2 LocalAnchorB;
        public float Length;
        public float FrequencyHz;
        public float DampingRatio;
        public bool CollideConnected;

        // Solver temp variables
        public float Impulse;
        public float Mass;
        public float Gamma;
        public float Bias;
        public Vec2 U;
    }
}
