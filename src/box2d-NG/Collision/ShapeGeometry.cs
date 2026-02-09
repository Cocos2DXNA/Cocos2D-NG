using System;
using System.Collections.Generic;

namespace Box2DNG
{
    public static class ShapeGeometry
    {
        public static Circle ToCircle(CircleShape shape) => new Circle(shape.Center, shape.Radius);

        public static Capsule ToCapsule(CapsuleShape shape) => new Capsule(shape.Center1, shape.Center2, shape.Radius);

        public static Segment ToSegment(SegmentShape shape) => new Segment(shape.Point1, shape.Point2);

        public static ChainSegment ToChainSegment(ChainSegmentShape shape)
        {
            Segment segment = new Segment(shape.Point1, shape.Point2);
            return new ChainSegment(shape.Ghost1, segment, shape.Ghost2, 0);
        }

        public static Polygon ToPolygon(PolygonShape shape, float? radiusOverride = null)
        {
            if (shape.Vertices == null)
            {
                throw new ArgumentNullException(nameof(shape.Vertices));
            }

            int count = Math.Min(shape.Vertices.Count, Constants.MaxPolygonVertices);
            if (count < 3)
            {
                throw new InvalidOperationException("Polygon requires at least 3 vertices.");
            }

            Vec2[] vertices = new Vec2[count];
            for (int i = 0; i < count; ++i)
            {
                vertices[i] = shape.Vertices[i];
            }

            Vec2[] normals = new Vec2[count];
            for (int i = 0; i < count; ++i)
            {
                int i2 = i + 1 < count ? i + 1 : 0;
                Vec2 edge = vertices[i2] - vertices[i];
                float lenSqr = edge.LengthSquared;
                if (lenSqr <= Constants.Epsilon * Constants.Epsilon)
                {
                    throw new InvalidOperationException("Polygon has a degenerate edge.");
                }
                // For CCW polygons, outward normals are the right-hand perpendicular.
                normals[i] = MathFng.RightPerp(edge).Normalize();
            }

            Vec2 centroid = ComputeCentroid(vertices, count);
            float radius = radiusOverride ?? shape.Radius;
            return new Polygon(vertices, normals, centroid, radius, count);
        }

        public static ShapeProxy ToProxy(Shape shape)
        {
            switch (shape.Type)
            {
                case ShapeType.Circle:
                    return ShapeProxyFactory.FromCircle(ToCircle((CircleShape)shape));
                case ShapeType.Capsule:
                    return ShapeProxyFactory.FromCapsule(ToCapsule((CapsuleShape)shape));
                case ShapeType.Segment:
                    return ShapeProxyFactory.FromSegment(ToSegment((SegmentShape)shape));
                case ShapeType.Polygon:
                    return ShapeProxyFactory.FromPolygon(ToPolygon((PolygonShape)shape));
                case ShapeType.ChainSegment:
                    return ShapeProxyFactory.FromSegment(ToChainSegment((ChainSegmentShape)shape).Segment);
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape.Type), shape.Type, "Unsupported shape type.");
            }
        }

        public static Aabb ComputeAabb(Shape shape, Transform transform)
        {
            switch (shape.Type)
            {
                case ShapeType.Circle:
                    return ComputeCircleAabb(ToCircle((CircleShape)shape), transform);
                case ShapeType.Capsule:
                    return ComputeCapsuleAabb(ToCapsule((CapsuleShape)shape), transform);
                case ShapeType.Segment:
                    return ComputeSegmentAabb(ToSegment((SegmentShape)shape), transform, Constants.PolygonRadius);
                case ShapeType.Polygon:
                    return ComputePolygonAabb(ToPolygon((PolygonShape)shape), transform);
                case ShapeType.ChainSegment:
                    return ComputeSegmentAabb(ToChainSegment((ChainSegmentShape)shape).Segment, transform, Constants.PolygonRadius);
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape.Type), shape.Type, "Unsupported shape type.");
            }
        }

        private static Vec2 ComputeCentroid(IReadOnlyList<Vec2> vertices, int count)
        {
            Vec2 center = Vec2.Zero;
            float area = 0f;

            Vec2 origin = vertices[0];
            const float inv3 = 1f / 3f;

            for (int i = 1; i < count - 1; ++i)
            {
                Vec2 e1 = vertices[i] - origin;
                Vec2 e2 = vertices[i + 1] - origin;
                float a = 0.5f * Vec2.Cross(e1, e2);
                center += a * inv3 * (e1 + e2);
                area += a;
            }

            if (MathF.Abs(area) <= Constants.Epsilon)
            {
                Vec2 sum = Vec2.Zero;
                for (int i = 0; i < count; ++i)
                {
                    sum += vertices[i];
                }
                return (1f / count) * sum;
            }

            float invArea = 1f / area;
            center = new Vec2(center.X * invArea, center.Y * invArea);
            center += origin;
            return center;
        }

        private static Aabb ComputeCircleAabb(Circle circle, Transform transform)
        {
            Vec2 center = Transform.Mul(transform, circle.Center);
            Vec2 r = new Vec2(circle.Radius, circle.Radius);
            return new Aabb(center - r, center + r);
        }

        private static Aabb ComputeCapsuleAabb(Capsule capsule, Transform transform)
        {
            Vec2 p1 = Transform.Mul(transform, capsule.Center1);
            Vec2 p2 = Transform.Mul(transform, capsule.Center2);
            Vec2 lower = new Vec2(MathF.Min(p1.X, p2.X), MathF.Min(p1.Y, p2.Y));
            Vec2 upper = new Vec2(MathF.Max(p1.X, p2.X), MathF.Max(p1.Y, p2.Y));
            Vec2 r = new Vec2(capsule.Radius, capsule.Radius);
            return new Aabb(lower - r, upper + r);
        }

        private static Aabb ComputeSegmentAabb(Segment segment, Transform transform, float radius)
        {
            Vec2 p1 = Transform.Mul(transform, segment.Point1);
            Vec2 p2 = Transform.Mul(transform, segment.Point2);
            Vec2 lower = new Vec2(MathF.Min(p1.X, p2.X), MathF.Min(p1.Y, p2.Y));
            Vec2 upper = new Vec2(MathF.Max(p1.X, p2.X), MathF.Max(p1.Y, p2.Y));
            Vec2 r = new Vec2(radius, radius);
            return new Aabb(lower - r, upper + r);
        }

        private static Aabb ComputePolygonAabb(Polygon polygon, Transform transform)
        {
            Vec2 p0 = Transform.Mul(transform, polygon.Vertices[0]);
            Vec2 lower = p0;
            Vec2 upper = p0;
            for (int i = 1; i < polygon.Count; ++i)
            {
                Vec2 p = Transform.Mul(transform, polygon.Vertices[i]);
                lower = new Vec2(MathF.Min(lower.X, p.X), MathF.Min(lower.Y, p.Y));
                upper = new Vec2(MathF.Max(upper.X, p.X), MathF.Max(upper.Y, p.Y));
            }
            Vec2 r = new Vec2(polygon.Radius, polygon.Radius);
            return new Aabb(lower - r, upper + r);
        }
    }
}
