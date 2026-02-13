using System;

namespace Box2DNG
{
    public static class Collision
    {
        public static bool IsValidRay(RayCastInput input)
        {
            if (!MathFng.IsValidVec2(input.Origin) || !MathFng.IsValidVec2(input.Translation))
            {
                return false;
            }
            if (!MathFng.IsValidFloat(input.MaxFraction) || input.MaxFraction < 0f)
            {
                return false;
            }
            return true;
        }

        public static bool TestOverlap(Aabb a, Aabb b)
        {
            if (a.UpperBound.X < b.LowerBound.X || a.LowerBound.X > b.UpperBound.X)
            {
                return false;
            }
            if (a.UpperBound.Y < b.LowerBound.Y || a.LowerBound.Y > b.UpperBound.Y)
            {
                return false;
            }
            return true;
        }

        public static CastOutput RayCastCircle(Circle circle, RayCastInput input)
        {
            Vec2 p = input.Origin;
            Vec2 s = circle.Center;
            Vec2 d = input.Translation;

            Vec2 m = p - s;
            float b = Vec2.Dot(m, d);
            float c = Vec2.Dot(m, m) - circle.Radius * circle.Radius;

            if (c > 0f && b > 0f)
            {
                return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
            }

            float rr = Vec2.Dot(d, d);
            if (rr < 1e-12f)
            {
                return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
            }

            float sigma = b * b - rr * c;
            if (sigma < 0f)
            {
                return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
            }

            float a = -(b + MathF.Sqrt(sigma));
            float fraction = a / rr;
            if (fraction < 0f || fraction > input.MaxFraction)
            {
                return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
            }

            Vec2 point = p + fraction * d;
            Vec2 normal = (point - s).Normalize();
            return new CastOutput(normal, point, fraction, 1, true);
        }

        public static CastOutput RayCastSegment(Segment segment, RayCastInput input)
        {
            Vec2 p1 = input.Origin;
            Vec2 p2 = input.Origin + input.Translation;
            Vec2 s1 = segment.Point1;
            Vec2 s2 = segment.Point2;

            Vec2 r = p2 - p1;
            Vec2 s = s2 - s1;
            float rxs = Vec2.Cross(r, s);
            float qpxr = Vec2.Cross(s1 - p1, r);

            if (MathF.Abs(rxs) < 1e-12f)
            {
                return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
            }

            float t = Vec2.Cross(s1 - p1, s) / rxs;
            float u = qpxr / rxs;

            const float eps = 1e-6f;
            if (t >= -eps && t <= input.MaxFraction + eps && u >= -eps && u <= 1f + eps)
            {
                Vec2 point = p1 + t * r;
                Vec2 normal = MathFng.LeftPerp(s).Normalize();
                return new CastOutput(normal, point, t, 1, true);
            }

            return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
        }

        public static CastOutput RayCastPolygon(Polygon polygon, RayCastInput input)
        {
            Vec2 p1 = input.Origin;
            Vec2 d = input.Translation;

            float lower = 0f;
            float upper = input.MaxFraction;
            Vec2 normal = Vec2.Zero;

            for (int i = 0; i < polygon.Count; ++i)
            {
                Vec2 n = polygon.Normals[i];
                Vec2 v = polygon.Vertices[i];

                float numerator = Vec2.Dot(n, v - p1);
                float denominator = Vec2.Dot(n, d);

                if (MathF.Abs(denominator) < 1e-12f)
                {
                    if (numerator < 0f)
                    {
                        return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
                    }
                }
                else
                {
                    float t = numerator / denominator;
                    if (denominator < 0f)
                    {
                        if (t > lower)
                        {
                            lower = t;
                            normal = n;
                        }
                    }
                    else
                    {
                        if (t < upper)
                        {
                            upper = t;
                        }
                    }

                    if (upper < lower)
                    {
                        return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
                    }
                }
            }

            if (lower < 0f || lower > input.MaxFraction)
            {
                return new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
            }

            Vec2 point = p1 + lower * d;
            return new CastOutput(normal, point, lower, 1, true);
        }

        public static CastOutput RayCastCapsule(Capsule capsule, RayCastInput input)
        {
            Vec2 a = capsule.Center1;
            Vec2 b = capsule.Center2;
            float radius = capsule.Radius;

            Vec2 axis = b - a;
            float length = axis.Length;
            if (length <= Constants.Epsilon)
            {
                return RayCastCircle(new Circle(a, radius), input);
            }

            Vec2 u = axis / length;
            Vec2 v = new Vec2(-u.Y, u.X);

            Vec2 localOrigin = new Vec2(Vec2.Dot(input.Origin - a, u), Vec2.Dot(input.Origin - a, v));
            Vec2 localDir = new Vec2(Vec2.Dot(input.Translation, u), Vec2.Dot(input.Translation, v));

            CastOutput best = new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);

            if (TryRayCastAabb(localOrigin, localDir, input.MaxFraction, 0f, length, -radius, radius, out CastOutput rectHit))
            {
                best = rectHit;
            }

            CastOutput capA = RayCastCircle(new Circle(new Vec2(0f, 0f), radius), new RayCastInput(localOrigin, localDir, input.MaxFraction));
            if (capA.Hit && (!best.Hit || capA.Fraction < best.Fraction))
            {
                best = capA;
            }

            CastOutput capB = RayCastCircle(new Circle(new Vec2(length, 0f), radius), new RayCastInput(localOrigin, localDir, input.MaxFraction));
            if (capB.Hit && (!best.Hit || capB.Fraction < best.Fraction))
            {
                best = capB;
            }

            if (!best.Hit)
            {
                return best;
            }

            Vec2 worldPoint = a + u * best.Point.X + v * best.Point.Y;
            Vec2 worldNormal = (best.Normal.X * u + best.Normal.Y * v).Normalize();
            return new CastOutput(worldNormal, worldPoint, best.Fraction, best.Iterations, true);
        }

        public static CastOutput RayCastChainSegment(ChainSegment chainSegment, RayCastInput input)
        {
            return RayCastSegment(chainSegment.Segment, input);
        }

        public static CastOutput ShapeCastCircle(Circle shape, ShapeCastInput input)
        {
            ShapeCastPairInput pairInput = new ShapeCastPairInput(
                ShapeProxyFactory.FromCircle(shape),
                input.Proxy,
                Transform.Identity,
                Transform.Identity,
                input.Translation,
                input.MaxFraction,
                input.CanEncroach);
            return ShapeCast.Cast(pairInput);
        }

        public static CastOutput ShapeCastCapsule(Capsule shape, ShapeCastInput input)
        {
            ShapeCastPairInput pairInput = new ShapeCastPairInput(
                ShapeProxyFactory.FromCapsule(shape),
                input.Proxy,
                Transform.Identity,
                Transform.Identity,
                input.Translation,
                input.MaxFraction,
                input.CanEncroach);
            return ShapeCast.Cast(pairInput);
        }

        public static CastOutput ShapeCastSegment(Segment shape, ShapeCastInput input)
        {
            ShapeCastPairInput pairInput = new ShapeCastPairInput(
                ShapeProxyFactory.FromSegment(shape),
                input.Proxy,
                Transform.Identity,
                Transform.Identity,
                input.Translation,
                input.MaxFraction,
                input.CanEncroach);
            return ShapeCast.Cast(pairInput);
        }

        public static CastOutput ShapeCastPolygon(Polygon shape, ShapeCastInput input)
        {
            ShapeCastPairInput pairInput = new ShapeCastPairInput(
                ShapeProxyFactory.FromPolygon(shape),
                input.Proxy,
                Transform.Identity,
                Transform.Identity,
                input.Translation,
                input.MaxFraction,
                input.CanEncroach);
            return ShapeCast.Cast(pairInput);
        }

        private static bool TryRayCastAabb(Vec2 origin, Vec2 translation, float maxFraction, float minX, float maxX, float minY, float maxY, out CastOutput output)
        {
            float tMin = 0f;
            float tMax = maxFraction;
            Vec2 normal = Vec2.Zero;

            if (!ClipRay(ref tMin, ref tMax, origin.X, translation.X, minX, maxX, ref normal, new Vec2(-1f, 0f), new Vec2(1f, 0f)))
            {
                output = new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
                return false;
            }

            if (!ClipRay(ref tMin, ref tMax, origin.Y, translation.Y, minY, maxY, ref normal, new Vec2(0f, -1f), new Vec2(0f, 1f)))
            {
                output = new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
                return false;
            }

            Vec2 point = origin + tMin * translation;
            output = new CastOutput(normal, point, tMin, 1, true);
            return true;
        }

        private static bool ClipRay(ref float tMin, ref float tMax, float origin, float direction, float min, float max, ref Vec2 normal, Vec2 normalMin, Vec2 normalMax)
        {
            const float epsilon = 1e-12f;
            if (MathF.Abs(direction) < epsilon)
            {
                if (origin < min || origin > max)
                {
                    return false;
                }
                return true;
            }

            float inv = 1f / direction;
            float t1 = (min - origin) * inv;
            float t2 = (max - origin) * inv;
            Vec2 n = normalMin;

            if (t1 > t2)
            {
                (t1, t2) = (t2, t1);
                n = normalMax;
            }

            if (t1 > tMin)
            {
                tMin = t1;
                normal = n;
            }

            if (t2 < tMax)
            {
                tMax = t2;
            }

            return tMin <= tMax;
        }
    }
}
