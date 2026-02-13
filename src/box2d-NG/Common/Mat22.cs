using System;

namespace Box2DNG
{
    public readonly struct Mat22 : IEquatable<Mat22>
    {
        public readonly Vec2 Ex;
        public readonly Vec2 Ey;

        public Mat22(Vec2 ex, Vec2 ey)
        {
            Ex = ex;
            Ey = ey;
        }

        public static Mat22 Identity => new Mat22(new Vec2(1f, 0f), new Vec2(0f, 1f));

        public bool Equals(Mat22 other) => Ex.Equals(other.Ex) && Ey.Equals(other.Ey);

        public override bool Equals(object? obj) => obj is Mat22 other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + Ex.GetHashCode();
                hash = (hash * 31) + Ey.GetHashCode();
                return hash;
            }
        }

        public static Vec2 Mul(Mat22 a, Vec2 v)
        {
            return new Vec2(a.Ex.X * v.X + a.Ey.X * v.Y, a.Ex.Y * v.X + a.Ey.Y * v.Y);
        }

        public static Vec2 MulT(Mat22 a, Vec2 v)
        {
            return new Vec2(Vec2.Dot(v, a.Ex), Vec2.Dot(v, a.Ey));
        }

        public override string ToString() => $"[{Ex} {Ey}]";
    }
}
