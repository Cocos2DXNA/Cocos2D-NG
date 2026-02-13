using System;

namespace Box2DNG
{
    public readonly struct SegmentDistanceResult
    {
        public readonly float Fraction1;
        public readonly float Fraction2;
        public readonly Vec2 Closest1;
        public readonly Vec2 Closest2;
        public readonly float DistanceSquared;

        public SegmentDistanceResult(float fraction1, float fraction2, Vec2 closest1, Vec2 closest2, float distanceSquared)
        {
            Fraction1 = fraction1;
            Fraction2 = fraction2;
            Closest1 = closest1;
            Closest2 = closest2;
            DistanceSquared = distanceSquared;
        }
    }

    public static class DistanceAlgorithms
    {
        public static SegmentDistanceResult SegmentDistance(Vec2 p1, Vec2 q1, Vec2 p2, Vec2 q2)
        {
            Vec2 d1 = q1 - p1;
            Vec2 d2 = q2 - p2;
            Vec2 r = p1 - p2;
            float dd1 = Vec2.Dot(d1, d1);
            float dd2 = Vec2.Dot(d2, d2);
            float rd1 = Vec2.Dot(r, d1);
            float rd2 = Vec2.Dot(r, d2);

            const float epsSqr = 1.1920929e-7f * 1.1920929e-7f;
            float f1;
            float f2;

            if (dd1 < epsSqr || dd2 < epsSqr)
            {
                if (dd1 >= epsSqr)
                {
                    f1 = MathFng.Clamp(-rd1 / dd1, 0f, 1f);
                    f2 = 0f;
                }
                else if (dd2 >= epsSqr)
                {
                    f1 = 0f;
                    f2 = MathFng.Clamp(rd2 / dd2, 0f, 1f);
                }
                else
                {
                    f1 = 0f;
                    f2 = 0f;
                }
            }
            else
            {
                float d12 = Vec2.Dot(d1, d2);
                float denominator = dd1 * dd2 - d12 * d12;

                f1 = 0f;
                if (denominator != 0f)
                {
                    f1 = MathFng.Clamp((d12 * rd2 - rd1 * dd2) / denominator, 0f, 1f);
                }

                f2 = (d12 * f1 + rd2) / dd2;

                if (f2 < 0f)
                {
                    f2 = 0f;
                    f1 = MathFng.Clamp(-rd1 / dd1, 0f, 1f);
                }
                else if (f2 > 1f)
                {
                    f2 = 1f;
                    f1 = MathFng.Clamp((d12 - rd1) / dd1, 0f, 1f);
                }
            }

            Vec2 closest1 = p1 + f1 * d1;
            Vec2 closest2 = p2 + f2 * d2;
            float distanceSquared = (closest1 - closest2).LengthSquared;

            return new SegmentDistanceResult(f1, f2, closest1, closest2, distanceSquared);
        }
    }
}
