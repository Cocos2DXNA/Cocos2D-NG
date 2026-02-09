using System;

namespace Box2DNG
{
    public static class ProxyFactory
    {
        public static ShapeProxy MakeProxy(ReadOnlySpan<Vec2> points, float radius)
        {
            int count = Math.Min(points.Length, Constants.MaxPolygonVertices);
            Vec2[] result = new Vec2[count];
            for (int i = 0; i < count; ++i)
            {
                result[i] = points[i];
            }
            return new ShapeProxy(result, count, radius);
        }

        public static ShapeProxy MakeOffsetProxy(ReadOnlySpan<Vec2> points, float radius, Vec2 position, Rot rotation)
        {
            int count = Math.Min(points.Length, Constants.MaxPolygonVertices);
            Vec2[] result = new Vec2[count];
            Transform transform = new Transform(position, rotation);
            for (int i = 0; i < count; ++i)
            {
                result[i] = Transform.Mul(transform, points[i]);
            }
            return new ShapeProxy(result, count, radius);
        }
    }
}
