using System;

namespace Box2DNG
{
    public enum ToiState
    {
        Unknown = 0,
        Failed,
        Overlapped,
        Hit,
        Separated
    }

    public readonly struct ToiInput
    {
        public readonly ShapeProxy ProxyA;
        public readonly ShapeProxy ProxyB;
        public readonly Sweep SweepA;
        public readonly Sweep SweepB;
        public readonly float MaxFraction;

        public ToiInput(ShapeProxy proxyA, ShapeProxy proxyB, Sweep sweepA, Sweep sweepB, float maxFraction)
        {
            ProxyA = proxyA;
            ProxyB = proxyB;
            SweepA = sweepA;
            SweepB = sweepB;
            MaxFraction = maxFraction;
        }
    }

    public readonly struct ToiOutput
    {
        public readonly ToiState State;
        public readonly float Fraction;
        public readonly Vec2 Point;
        public readonly Vec2 Normal;

        public ToiOutput(ToiState state, float fraction, Vec2 point, Vec2 normal)
        {
            State = state;
            Fraction = fraction;
            Point = point;
            Normal = normal;
        }
    }

    public static class TimeOfImpact
    {
        private enum SeparationType
        {
            Points,
            FaceA,
            FaceB
        }

        private sealed class SeparationFunction
        {
            public SeparationType Type;
            public Vec2 Axis;
            public Vec2 LocalPoint;
            public ShapeProxy ProxyA = default!;
            public ShapeProxy ProxyB = default!;
            public Sweep SweepA;
            public Sweep SweepB;

            public float Evaluate(int indexA, int indexB, float t)
            {
                Transform xfA = SweepA.GetTransform(t);
                Transform xfB = SweepB.GetTransform(t);

                switch (Type)
                {
                    case SeparationType.Points:
                        {
                            Vec2 localPointA = ProxyA.Points[indexA];
                            Vec2 localPointB = ProxyB.Points[indexB];
                            Vec2 pointA = Transform.Mul(xfA, localPointA);
                            Vec2 pointB = Transform.Mul(xfB, localPointB);
                            return Vec2.Dot(pointB - pointA, Axis);
                        }
                    case SeparationType.FaceA:
                        {
                            Vec2 normal = Rot.Mul(xfA.Q, Axis);
                            Vec2 pointA = Transform.Mul(xfA, LocalPoint);
                            Vec2 pointB = Transform.Mul(xfB, ProxyB.Points[indexB]);
                            return Vec2.Dot(pointB - pointA, normal);
                        }
                    case SeparationType.FaceB:
                        {
                            Vec2 normal = Rot.Mul(xfB.Q, Axis);
                            Vec2 pointB = Transform.Mul(xfB, LocalPoint);
                            Vec2 pointA = Transform.Mul(xfA, ProxyA.Points[indexA]);
                            return Vec2.Dot(pointA - pointB, normal);
                        }
                }

                return 0f;
            }

            public float FindMinSeparation(out int indexA, out int indexB, float t)
            {
                Transform xfA = SweepA.GetTransform(t);
                Transform xfB = SweepB.GetTransform(t);

                switch (Type)
                {
                    case SeparationType.Points:
                        {
                            Vec2 axisA = Rot.MulT(xfA.Q, Axis);
                            Vec2 axisB = Rot.MulT(xfB.Q, -Axis);

                            indexA = GetSupport(ProxyA, axisA);
                            indexB = GetSupport(ProxyB, axisB);

                            Vec2 pointA = Transform.Mul(xfA, ProxyA.Points[indexA]);
                            Vec2 pointB = Transform.Mul(xfB, ProxyB.Points[indexB]);
                            return Vec2.Dot(pointB - pointA, Axis);
                        }
                    case SeparationType.FaceA:
                        {
                            Vec2 normal = Rot.Mul(xfA.Q, Axis);
                            Vec2 axisB = Rot.MulT(xfB.Q, -normal);
                            indexA = -1;
                            indexB = GetSupport(ProxyB, axisB);
                            Vec2 pointA = Transform.Mul(xfA, LocalPoint);
                            Vec2 pointB = Transform.Mul(xfB, ProxyB.Points[indexB]);
                            return Vec2.Dot(pointB - pointA, normal);
                        }
                    case SeparationType.FaceB:
                        {
                            Vec2 normal = Rot.Mul(xfB.Q, Axis);
                            Vec2 axisA = Rot.MulT(xfA.Q, -normal);
                            indexB = -1;
                            indexA = GetSupport(ProxyA, axisA);
                            Vec2 pointB = Transform.Mul(xfB, LocalPoint);
                            Vec2 pointA = Transform.Mul(xfA, ProxyA.Points[indexA]);
                            return Vec2.Dot(pointA - pointB, normal);
                        }
                }

                indexA = 0;
                indexB = 0;
                return 0f;
            }

            private static int GetSupport(ShapeProxy proxy, Vec2 direction)
            {
                int bestIndex = 0;
                float bestValue = Vec2.Dot(proxy.Points[0], direction);
                for (int i = 1; i < proxy.Count; ++i)
                {
                    float value = Vec2.Dot(proxy.Points[i], direction);
                    if (value > bestValue)
                    {
                        bestIndex = i;
                        bestValue = value;
                    }
                }
                return bestIndex;
            }
        }

        public static ToiOutput Compute(ToiInput input)
        {
            ShapeProxy proxyA = input.ProxyA;
            ShapeProxy proxyB = input.ProxyB;

            float tMax = input.MaxFraction;
            float totalRadius = proxyA.Radius + proxyB.Radius;
            float target = MathF.Max(Constants.LinearSlop, totalRadius - Constants.LinearSlop);
            float tolerance = 0.25f * Constants.LinearSlop;

            float t1 = 0f;
            const int maxIterations = 20;
            int distanceIterations = 0;

            SimplexCache cache = new SimplexCache(0f, 0, 0, 0, 0, 0, 0, 0);

            for (;;)
            {
                Transform xfA = input.SweepA.GetTransform(t1);
                Transform xfB = input.SweepB.GetTransform(t1);

                DistanceInput distanceInput = new DistanceInput(proxyA, proxyB, xfA, xfB, false);
                DistanceOutput distanceOutput = Distance.Compute(distanceInput, ref cache);

                distanceIterations++;

                if (distanceOutput.Distance <= 0f)
                {
                    return new ToiOutput(ToiState.Overlapped, 0f, Vec2.Zero, Vec2.Zero);
                }

                Vec2 normal = (distanceOutput.PointB - distanceOutput.PointA).Normalize();

                if (distanceOutput.Distance <= target + tolerance)
                {
                    Vec2 pA = distanceOutput.PointA + proxyA.Radius * normal;
                    Vec2 pB = distanceOutput.PointB - proxyB.Radius * normal;
                    Vec2 point = 0.5f * (pA + pB);
                    return new ToiOutput(ToiState.Hit, t1, point, normal);
                }

                SeparationFunction fcn = MakeSeparationFunction(cache, proxyA, input.SweepA, proxyB, input.SweepB, t1);

                float t2 = tMax;
                int pushBackIterations = 0;
                for (;;)
                {
                    float s2 = fcn.FindMinSeparation(out int indexA, out int indexB, t2);

                    if (s2 > target + tolerance)
                    {
                        return new ToiOutput(ToiState.Separated, tMax, Vec2.Zero, Vec2.Zero);
                    }

                    if (s2 > target - tolerance)
                    {
                        t1 = t2;
                        break;
                    }

                    float s1 = fcn.Evaluate(indexA, indexB, t1);
                    if (s1 < target - tolerance)
                    {
                        return new ToiOutput(ToiState.Failed, t1, Vec2.Zero, Vec2.Zero);
                    }

                    if (s1 <= target + tolerance)
                    {
                        Vec2 pA = distanceOutput.PointA + proxyA.Radius * normal;
                        Vec2 pB = distanceOutput.PointB - proxyB.Radius * normal;
                        Vec2 point = 0.5f * (pA + pB);
                        return new ToiOutput(ToiState.Hit, t1, point, normal);
                    }

                    int rootIterations = 0;
                    float a1 = t1;
                    float a2 = t2;
                    for (;;)
                    {
                        float t;
                        if ((rootIterations & 1) != 0)
                        {
                            float denom = s2 - s1;
                            t = denom != 0f ? a1 + (target - s1) * (a2 - a1) / denom : 0.5f * (a1 + a2);
                        }
                        else
                        {
                            t = 0.5f * (a1 + a2);
                        }

                        rootIterations++;
                        float s = fcn.Evaluate(indexA, indexB, t);

                        if (MathF.Abs(s - target) < tolerance)
                        {
                            t2 = t;
                            break;
                        }

                        if (s > target)
                        {
                            a1 = t;
                            s1 = s;
                        }
                        else
                        {
                            a2 = t;
                            s2 = s;
                        }

                        if (rootIterations == 50)
                        {
                            break;
                        }
                    }

                    pushBackIterations++;
                    if (pushBackIterations == Constants.MaxPolygonVertices)
                    {
                        break;
                    }
                }

                if (distanceIterations == maxIterations)
                {
                    Vec2 pA = distanceOutput.PointA + proxyA.Radius * normal;
                    Vec2 pB = distanceOutput.PointB - proxyB.Radius * normal;
                    Vec2 point = 0.5f * (pA + pB);
                    return new ToiOutput(ToiState.Failed, t1, point, normal);
                }
            }

        }

        private static SeparationFunction MakeSeparationFunction(SimplexCache cache, ShapeProxy proxyA, Sweep sweepA, ShapeProxy proxyB, Sweep sweepB, float t1)
        {
            SeparationFunction fcn = new SeparationFunction
            {
                ProxyA = proxyA,
                ProxyB = proxyB,
                SweepA = sweepA,
                SweepB = sweepB
            };

            Transform xfA = sweepA.GetTransform(t1);
            Transform xfB = sweepB.GetTransform(t1);

            if (cache.Count == 1)
            {
                fcn.Type = SeparationType.Points;
                Vec2 localPointA = proxyA.Points[cache.IndexA0];
                Vec2 localPointB = proxyB.Points[cache.IndexB0];
                Vec2 pointA = Transform.Mul(xfA, localPointA);
                Vec2 pointB = Transform.Mul(xfB, localPointB);
                Vec2 axis = pointB - pointA;
                fcn.Axis = axis.Normalize();
                fcn.LocalPoint = Vec2.Zero;
                return fcn;
            }

            if (cache.Count == 2 && cache.IndexA0 == cache.IndexA1)
            {
                fcn.Type = SeparationType.FaceB;
                Vec2 localPointB1 = proxyB.Points[cache.IndexB0];
                Vec2 localPointB2 = proxyB.Points[cache.IndexB1];
                Vec2 axis = MathFng.LeftPerp(localPointB2 - localPointB1).Normalize();
                Vec2 normal = Rot.Mul(xfB.Q, axis);
                Vec2 pointB = 0.5f * (localPointB1 + localPointB2);
                Vec2 pointA = Transform.Mul(xfA, proxyA.Points[cache.IndexA0]);
                if (Vec2.Dot(pointA - Transform.Mul(xfB, pointB), normal) < 0f)
                {
                    axis = -axis;
                }
                fcn.Axis = axis;
                fcn.LocalPoint = pointB;
                return fcn;
            }

            fcn.Type = SeparationType.FaceA;
            Vec2 localPointA1 = proxyA.Points[cache.IndexA0];
            Vec2 localPointA2 = proxyA.Points[cache.IndexA1];
            Vec2 axisA = MathFng.LeftPerp(localPointA2 - localPointA1).Normalize();
            Vec2 normalA = Rot.Mul(xfA.Q, axisA);
            Vec2 pointA2 = 0.5f * (localPointA1 + localPointA2);
            Vec2 pointB2 = Transform.Mul(xfB, proxyB.Points[cache.IndexB0]);
            if (Vec2.Dot(pointB2 - Transform.Mul(xfA, pointA2), normalA) < 0f)
            {
                axisA = -axisA;
            }
            fcn.Axis = axisA;
            fcn.LocalPoint = pointA2;
            return fcn;
        }
    }
}
