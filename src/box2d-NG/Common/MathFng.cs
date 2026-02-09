using System;

namespace Box2DNG
{
    public static class MathFng
    {
        public const float Pi = MathF.PI;

        public static bool IsValidFloat(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static bool IsValidVec2(Vec2 v) => IsValidFloat(v.X) && IsValidFloat(v.Y);

        public static bool IsValidRotation(Rot q)
        {
            if (!IsValidFloat(q.C) || !IsValidFloat(q.S))
            {
                return false;
            }

            float mag = q.C * q.C + q.S * q.S;
            return MathF.Abs(mag - 1f) <= 0.01f;
        }

        public static bool IsValidTransform(Transform t) => IsValidVec2(t.P) && IsValidRotation(t.Q);

        public static bool IsValidAabb(Aabb aabb)
        {
            if (!IsValidVec2(aabb.LowerBound) || !IsValidVec2(aabb.UpperBound))
            {
                return false;
            }
            Vec2 d = aabb.UpperBound - aabb.LowerBound;
            return d.X >= 0f && d.Y >= 0f;
        }

        public static bool IsValidPlane(Plane plane)
        {
            if (!IsValidVec2(plane.Normal) || !IsValidFloat(plane.Offset))
            {
                return false;
            }
            float length = plane.Normal.Length;
            return MathF.Abs(length - 1f) <= 0.01f;
        }

        public static float Atan2(float y, float x) => MathF.Atan2(y, x);

        public static CosSin ComputeCosSin(float radians)
        {
            return new CosSin(MathF.Cos(radians), MathF.Sin(radians));
        }

        public static float Min(float a, float b) => a < b ? a : b;
        public static float Max(float a, float b) => a > b ? a : b;
        public static float Abs(float a) => a < 0 ? -a : a;

        public static int Min(int a, int b) => a < b ? a : b;
        public static int Max(int a, int b) => a > b ? a : b;
        public static int Abs(int a) => a < 0 ? -a : a;

        public static float Clamp(float value, float min, float max) => value < min ? min : (value > max ? max : value);
        public static int Clamp(int value, int min, int max) => value < min ? min : (value > max ? max : value);

        public static Vec2 Clamp(Vec2 value, Vec2 min, Vec2 max)
        {
            float x = Clamp(value.X, min.X, max.X);
            float y = Clamp(value.Y, min.Y, max.Y);
            return new Vec2(x, y);
        }

        public static Vec2 LeftPerp(Vec2 v) => new Vec2(-v.Y, v.X);
        public static Vec2 RightPerp(Vec2 v) => new Vec2(v.Y, -v.X);

        public static float MixFriction(float a, float b) => MathF.Sqrt(a * b);
        public static float MixRestitution(float a, float b) => a > b ? a : b;
    }
}
