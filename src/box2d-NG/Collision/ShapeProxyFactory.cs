using System;

namespace Box2DNG
{
    public static class ShapeProxyFactory
    {
        public static ShapeProxy FromCircle(Circle circle)
        {
            return new ShapeProxy(new[] { circle.Center }, 1, circle.Radius);
        }

        public static ShapeProxy FromCapsule(Capsule capsule)
        {
            return new ShapeProxy(new[] { capsule.Center1, capsule.Center2 }, 2, capsule.Radius);
        }

        public static ShapeProxy FromSegment(Segment segment)
        {
            return new ShapeProxy(new[] { segment.Point1, segment.Point2 }, 2, Constants.PolygonRadius);
        }

        public static ShapeProxy FromPolygon(Polygon polygon)
        {
            return new ShapeProxy(polygon.Vertices, polygon.Count, polygon.Radius);
        }
    }
}
