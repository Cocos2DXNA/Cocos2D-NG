using System;

namespace Box2DNG
{
    public readonly struct Rot : IEquatable<Rot>
    {
        public readonly float S;
        public readonly float C;

        public static readonly Rot Identity = new Rot(0f, 1f);

        public Rot(float angle)
        {
            S = MathF.Sin(angle);
            C = MathF.Cos(angle);
        }

        public Rot(float s, float c)
        {
            S = s;
            C = c;
        }

        public Vec2 GetXAxis() => new Vec2(C, S);
        public Vec2 GetYAxis() => new Vec2(-S, C);
        public float Angle => MathF.Atan2(S, C);

        public bool Equals(Rot other) => S == other.S && C == other.C;

        public override bool Equals(object? obj) => obj is Rot other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + S.GetHashCode();
                hash = (hash * 31) + C.GetHashCode();
                return hash;
            }
        }

        public static Rot Mul(Rot a, Rot b)
        {
            return new Rot(a.S * b.C + a.C * b.S, a.C * b.C - a.S * b.S);
        }

        public static Rot MulT(Rot a, Rot b)
        {
            return new Rot(a.C * b.S - a.S * b.C, a.C * b.C + a.S * b.S);
        }

        public static Vec2 Mul(Rot q, Vec2 v)
        {
            return new Vec2(q.C * v.X - q.S * v.Y, q.S * v.X + q.C * v.Y);
        }

        public static Vec2 MulT(Rot q, Vec2 v)
        {
            return new Vec2(q.C * v.X + q.S * v.Y, -q.S * v.X + q.C * v.Y);
        }

        public override string ToString() => $"(s:{S}, c:{C})";
    }
}
