using System;

namespace Box2DNG
{
    public readonly struct Vec3 : IEquatable<Vec3>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public static readonly Vec3 Zero = new Vec3(0f, 0f, 0f);

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Vec3 other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object? obj) => obj is Vec3 other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + X.GetHashCode();
                hash = (hash * 31) + Y.GetHashCode();
                hash = (hash * 31) + Z.GetHashCode();
                return hash;
            }
        }

        public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);

        public float LengthSquared => X * X + Y * Y + Z * Z;

        public Vec3 Normalize()
        {
            float length = Length;
            if (length > 0f)
            {
                float inv = 1f / length;
                return new Vec3(X * inv, Y * inv, Z * inv);
            }
            return this;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3 operator -(Vec3 a) => new Vec3(-a.X, -a.Y, -a.Z);
        public static Vec3 operator *(Vec3 v, float s) => new Vec3(v.X * s, v.Y * s, v.Z * s);
        public static Vec3 operator *(float s, Vec3 v) => new Vec3(v.X * s, v.Y * s, v.Z * s);
        public static Vec3 operator /(Vec3 v, float s) => new Vec3(v.X / s, v.Y / s, v.Z / s);

        public static float Dot(Vec3 a, Vec3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            return new Vec3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
