using System;

namespace Box2DNG
{
    public static class MassProperties
    {
        public static MassData Compute(Shape shape, float density)
        {
            if (density <= 0f)
            {
                return new MassData(0f, Vec2.Zero, 0f);
            }

            switch (shape.Type)
            {
                case ShapeType.Circle:
                    return ComputeCircle(ShapeGeometry.ToCircle((CircleShape)shape), density);
                case ShapeType.Capsule:
                    return ComputeCapsule(ShapeGeometry.ToCapsule((CapsuleShape)shape), density);
                case ShapeType.Segment:
                    return new MassData(0f, 0.5f * (((SegmentShape)shape).Point1 + ((SegmentShape)shape).Point2), 0f);
                case ShapeType.Polygon:
                    return ComputePolygon(ShapeGeometry.ToPolygon((PolygonShape)shape), density);
                case ShapeType.ChainSegment:
                    return new MassData(0f, 0.5f * (((ChainSegmentShape)shape).Point1 + ((ChainSegmentShape)shape).Point2), 0f);
                default:
                    return new MassData(0f, Vec2.Zero, 0f);
            }
        }

        public static MassData ComputeCircle(Circle shape, float density)
        {
            float rr = shape.Radius * shape.Radius;
            float mass = density * MathF.PI * rr;
            float inertia = mass * 0.5f * rr;
            return new MassData(mass, shape.Center, inertia);
        }

        public static MassData ComputeCapsule(Capsule shape, float density)
        {
            float radius = shape.Radius;
            float rr = radius * radius;
            Vec2 p1 = shape.Center1;
            Vec2 p2 = shape.Center2;
            float length = (p2 - p1).Length;
            float ll = length * length;

            float circleMass = density * (MathF.PI * radius * radius);
            float boxMass = density * (2f * radius * length);

            float mass = circleMass + boxMass;
            Vec2 center = 0.5f * (p1 + p2);

            float lc = 4f * radius / (3f * MathF.PI);
            float h = 0.5f * length;
            float circleInertia = circleMass * (0.5f * rr + h * h + 2f * h * lc);
            float boxInertia = boxMass * (4f * rr + ll) / 12f;
            float inertia = circleInertia + boxInertia;

            return new MassData(mass, center, inertia);
        }

        public static MassData ComputePolygon(Polygon shape, float density)
        {
            if (shape.Count == 1)
            {
                Circle circle = new Circle(shape.Vertices[0], shape.Radius);
                return ComputeCircle(circle, density);
            }

            if (shape.Count == 2)
            {
                Capsule capsule = new Capsule(shape.Vertices[0], shape.Vertices[1], shape.Radius);
                return ComputeCapsule(capsule, density);
            }

            Vec2[] vertices = new Vec2[shape.Count];
            if (shape.Radius > 0f)
            {
                float sqrt2 = 1.412f;
                for (int i = 0; i < shape.Count; ++i)
                {
                    int j = i == 0 ? shape.Count - 1 : i - 1;
                    Vec2 n1 = shape.Normals[j];
                    Vec2 n2 = shape.Normals[i];
                    Vec2 mid = (n1 + n2).Normalize();
                    vertices[i] = shape.Vertices[i] + sqrt2 * shape.Radius * mid;
                }
            }
            else
            {
                for (int i = 0; i < shape.Count; ++i)
                {
                    vertices[i] = shape.Vertices[i];
                }
            }

            // If polygon radius is non-zero, slightly enlarge the area by the rounded
            // perimeter contribution to avoid under-estimating mass/inertia.
            float radiusArea = 0f;
            float radiusInertia = 0f;
            if (shape.Radius > 0f)
            {
                float perimeter = 0f;
                for (int i = 0; i < shape.Count; ++i)
                {
                    int j = i + 1 < shape.Count ? i + 1 : 0;
                    perimeter += (shape.Vertices[j] - shape.Vertices[i]).Length;
                }

                radiusArea = perimeter * shape.Radius + MathF.PI * shape.Radius * shape.Radius;
                radiusInertia = 0.5f * radiusArea * shape.Radius * shape.Radius;
            }

            Vec2 center = Vec2.Zero;
            float area = 0f;
            float rotationalInertia = 0f;

            Vec2 r = vertices[0];
            const float inv3 = 1f / 3f;

            for (int i = 1; i < shape.Count - 1; ++i)
            {
                Vec2 e1 = vertices[i] - r;
                Vec2 e2 = vertices[i + 1] - r;
                float D = Vec2.Cross(e1, e2);
                float triangleArea = 0.5f * D;
                area += triangleArea;
                center += triangleArea * inv3 * (e1 + e2);

                float ex1 = e1.X;
                float ey1 = e1.Y;
                float ex2 = e2.X;
                float ey2 = e2.Y;

                float intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
                float inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;
                rotationalInertia += (0.25f * inv3 * D) * (intx2 + inty2);
            }

            if (MathF.Abs(area) <= Constants.Epsilon)
            {
                return new MassData(0f, Vec2.Zero, 0f);
            }

            float mass = density * (area + radiusArea);
            float invArea = 1f / area;
            center = new Vec2(center.X * invArea, center.Y * invArea);
            Vec2 centroid = r + center;

            float inertia = density * rotationalInertia - (density * area) * Vec2.Dot(center, center) + density * radiusInertia;
            if (inertia < 0f)
            {
                inertia = 0f;
            }

            return new MassData(mass, centroid, inertia);
        }
    }
}
