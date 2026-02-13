using System;

namespace Box2DNG
{
    public struct RopeDef
    {
        public Vec2[] Vertices;
        public int Count;
        public float[] Masses;
        public Vec2 Gravity;
        public float Damping;
        public float K2;
        public float K3;

        public static RopeDef CreateDefault(Vec2[] vertices, float[] masses)
        {
            return new RopeDef
            {
                Vertices = vertices,
                Count = vertices?.Length ?? 0,
                Masses = masses,
                Gravity = new Vec2(0f, -10f),
                Damping = 0.1f,
                K2 = 1f,
                K3 = 0.1f
            };
        }
    }
}
