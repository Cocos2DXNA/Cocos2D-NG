using System;

namespace Box2DNG
{
    public readonly struct Transform : IEquatable<Transform>
    {
        public readonly Vec2 P;
        public readonly Rot Q;

        public static readonly Transform Identity = new Transform(Vec2.Zero, Rot.Identity);

        public Transform(Vec2 position, Rot rotation)
        {
            P = position;
            Q = rotation;
        }

        public bool Equals(Transform other) => P.Equals(other.P) && Q.Equals(other.Q);

        public override bool Equals(object? obj) => obj is Transform other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + P.GetHashCode();
                hash = (hash * 31) + Q.GetHashCode();
                return hash;
            }
        }

        public static Vec2 Mul(Transform t, Vec2 v)
        {
            Vec2 r = Rot.Mul(t.Q, v);
            return new Vec2(r.X + t.P.X, r.Y + t.P.Y);
        }

        public static Vec2 MulT(Transform t, Vec2 v)
        {
            float px = v.X - t.P.X;
            float py = v.Y - t.P.Y;
            return Rot.MulT(t.Q, new Vec2(px, py));
        }

        public override string ToString() => $"pos:{P} rot:{Q}";
    }
}
