using System;
using System.Runtime.CompilerServices;

namespace Box2DNG
{
    public readonly struct Vec2 : IEquatable<Vec2>
    {
        public readonly float X;
        public readonly float Y;

        public static readonly Vec2 Zero = new Vec2(0f, 0f);

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Vec2 other) => X == other.X && Y == other.Y;

        public override bool Equals(object? obj) => obj is Vec2 other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + X.GetHashCode();
                hash = (hash * 31) + Y.GetHashCode();
                return hash;
            }
        }

        public float Length => MathF.Sqrt(X * X + Y * Y);

        public float LengthSquared => X * X + Y * Y;

        public Vec2 Normalize()
        {
            float length = Length;
            if (length > 0f)
            {
                float inv = 1f / length;
                return new Vec2(X * inv, Y * inv);
            }
            return this;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator -(Vec2 a) => new Vec2(-a.X, -a.Y);
        public static Vec2 operator *(Vec2 v, float s) => new Vec2(v.X * s, v.Y * s);
        public static Vec2 operator *(float s, Vec2 v) => new Vec2(v.X * s, v.Y * s);
        public static Vec2 operator /(Vec2 v, float s) => new Vec2(v.X / s, v.Y / s);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross(Vec2 a, Vec2 b) => a.X * b.Y - a.Y * b.X;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Cross(Vec2 v, float s) => new Vec2(s * v.Y, -s * v.X);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2 Cross(float s, Vec2 v) => new Vec2(-s * v.Y, s * v.X);

        public override string ToString() => $"({X}, {Y})";
    }
}
