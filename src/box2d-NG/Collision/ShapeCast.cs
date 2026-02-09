using System;

namespace Box2DNG
{
    public static class ShapeCast
    {
        private const int MaxIters = 20;

        public static CastOutput Cast(ShapeCastPairInput input)
        {
            float linearSlop = Constants.LinearSlop;
            float totalRadius = input.ProxyA.Radius + input.ProxyB.Radius;
            float target = MathF.Max(linearSlop, totalRadius - linearSlop);
            float tolerance = 0.25f * linearSlop;

            float fraction = 0f;
            int iterations = 0;

            SimplexCache cache = new SimplexCache(0f, 0, 0, 0, 0, 0, 0, 0);
            DistanceInput distanceInput = new DistanceInput(input.ProxyA, input.ProxyB, input.TransformA, input.TransformB, false);
            Vec2 translationB = input.TranslationB;

            for (int iteration = 0; iteration < MaxIters; ++iteration)
            {
                iterations++;

                DistanceOutput distanceOutput = Distance.Compute(distanceInput, ref cache);
                float distance = distanceOutput.Distance;
                Vec2 normal = distance > 0f ? (distanceOutput.PointB - distanceOutput.PointA).Normalize() : Vec2.Zero;

                if (distance < target + tolerance)
                {
                    if (iteration == 0)
                    {
                        if (input.CanEncroach && distance > 2f * linearSlop)
                        {
                            target = distance - linearSlop;
                        }
                        else
                        {
                            Vec2 c1 = distanceOutput.PointA + input.ProxyA.Radius * normal;
                            Vec2 c2 = distanceOutput.PointB - input.ProxyB.Radius * normal;
                            Vec2 point = 0.5f * (c1 + c2);
                            return new CastOutput(normal, point, 0f, iterations, true);
                        }
                    }
                    else
                    {
                        Vec2 point = distanceOutput.PointA + input.ProxyA.Radius * normal;
                        return new CastOutput(normal, point, fraction, iterations, true);
                    }
                }

                if (distance <= 0f)
                {
                    return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, iterations, false);
                }

                float denominator = Vec2.Dot(translationB, normal);
                if (denominator >= 0f)
                {
                    return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, iterations, false);
                }

                fraction += (target - distance) / denominator;
                if (fraction >= input.MaxFraction)
                {
                    return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, iterations, false);
                }

                Vec2 newPos = input.TransformB.P + translationB * fraction;
                distanceInput = new DistanceInput(input.ProxyA, input.ProxyB, input.TransformA, new Transform(newPos, input.TransformB.Q), false);
            }

            return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, iterations, false);
        }
    }
}
