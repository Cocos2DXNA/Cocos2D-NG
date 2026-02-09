using System;

namespace Box2DNG
{
    public readonly struct ShapeProxy
    {
        public readonly Vec2[] Points;
        public readonly int Count;
        public readonly float Radius;

        public ShapeProxy(Vec2[] points, int count, float radius)
        {
            Points = points ?? throw new ArgumentNullException(nameof(points));
            Count = count;
            Radius = radius;
        }
    }

    public readonly struct ShapeCastInput
    {
        public readonly ShapeProxy Proxy;
        public readonly Vec2 Translation;
        public readonly float MaxFraction;
        public readonly bool CanEncroach;

        public ShapeCastInput(ShapeProxy proxy, Vec2 translation, float maxFraction, bool canEncroach)
        {
            Proxy = proxy;
            Translation = translation;
            MaxFraction = maxFraction;
            CanEncroach = canEncroach;
        }
    }

    public readonly struct ShapeCastPairInput
    {
        public readonly ShapeProxy ProxyA;
        public readonly ShapeProxy ProxyB;
        public readonly Transform TransformA;
        public readonly Transform TransformB;
        public readonly Vec2 TranslationB;
        public readonly float MaxFraction;
        public readonly bool CanEncroach;

        public ShapeCastPairInput(ShapeProxy proxyA, ShapeProxy proxyB, Transform transformA, Transform transformB, Vec2 translationB, float maxFraction, bool canEncroach)
        {
            ProxyA = proxyA;
            ProxyB = proxyB;
            TransformA = transformA;
            TransformB = transformB;
            TranslationB = translationB;
            MaxFraction = maxFraction;
            CanEncroach = canEncroach;
        }
    }

    public readonly struct CastOutput
    {
        public readonly Vec2 Normal;
        public readonly Vec2 Point;
        public readonly float Fraction;
        public readonly int Iterations;
        public readonly bool Hit;

        public CastOutput(Vec2 normal, Vec2 point, float fraction, int iterations, bool hit)
        {
            Normal = normal;
            Point = point;
            Fraction = fraction;
            Iterations = iterations;
            Hit = hit;
        }
    }

    public readonly struct Circle
    {
        public readonly Vec2 Center;
        public readonly float Radius;

        public Circle(Vec2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }

    public readonly struct Capsule
    {
        public readonly Vec2 Center1;
        public readonly Vec2 Center2;
        public readonly float Radius;

        public Capsule(Vec2 center1, Vec2 center2, float radius)
        {
            Center1 = center1;
            Center2 = center2;
            Radius = radius;
        }
    }

    public readonly struct Polygon
    {
        public readonly Vec2[] Vertices;
        public readonly Vec2[] Normals;
        public readonly Vec2 Centroid;
        public readonly float Radius;
        public readonly int Count;

        public Polygon(Vec2[] vertices, Vec2[] normals, Vec2 centroid, float radius, int count)
        {
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Normals = normals ?? throw new ArgumentNullException(nameof(normals));
            Centroid = centroid;
            Radius = radius;
            Count = count;
        }
    }

    public readonly struct Segment
    {
        public readonly Vec2 Point1;
        public readonly Vec2 Point2;

        public Segment(Vec2 point1, Vec2 point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
    }

    public readonly struct ChainSegment
    {
        public readonly Vec2 Ghost1;
        public readonly Segment Segment;
        public readonly Vec2 Ghost2;
        public readonly int ChainId;

        public ChainSegment(Vec2 ghost1, Segment segment, Vec2 ghost2, int chainId)
        {
            Ghost1 = ghost1;
            Segment = segment;
            Ghost2 = ghost2;
            ChainId = chainId;
        }
    }
}
