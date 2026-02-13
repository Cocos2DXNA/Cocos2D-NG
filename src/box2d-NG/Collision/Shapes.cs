using System;
using System.Collections.Generic;

namespace Box2DNG
{
    public abstract class Shape
    {
        public abstract ShapeType Type { get; }
    }

    public sealed class CircleShape : Shape
    {
        public override ShapeType Type => ShapeType.Circle;
        public Vec2 Center { get; private set; }
        public float Radius { get; private set; }

        public CircleShape(float radius) : this(Vec2.Zero, radius)
        {
        }

        public CircleShape(Vec2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public CircleShape WithCenter(Vec2 center) { Center = center; return this; }
        public CircleShape WithRadius(float radius) { Radius = radius; return this; }
    }

    public sealed class CapsuleShape : Shape
    {
        public override ShapeType Type => ShapeType.Capsule;
        public Vec2 Center1 { get; private set; }
        public Vec2 Center2 { get; private set; }
        public float Radius { get; private set; }

        public CapsuleShape(Vec2 center1, Vec2 center2, float radius)
        {
            Center1 = center1;
            Center2 = center2;
            Radius = radius;
        }

        public CapsuleShape WithCenters(Vec2 center1, Vec2 center2) { Center1 = center1; Center2 = center2; return this; }
        public CapsuleShape WithRadius(float radius) { Radius = radius; return this; }
    }

    public sealed class SegmentShape : Shape
    {
        public override ShapeType Type => ShapeType.Segment;
        public Vec2 Point1 { get; private set; }
        public Vec2 Point2 { get; private set; }

        public SegmentShape(Vec2 point1, Vec2 point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public SegmentShape WithPoints(Vec2 point1, Vec2 point2) { Point1 = point1; Point2 = point2; return this; }
    }

    public sealed class PolygonShape : Shape
    {
        public override ShapeType Type => ShapeType.Polygon;
        public IReadOnlyList<Vec2> Vertices { get; private set; }
        public float Radius { get; private set; } = Constants.PolygonRadius;

        public PolygonShape(IReadOnlyList<Vec2> vertices)
        {
            Vertices = vertices;
        }

        public PolygonShape WithVertices(IReadOnlyList<Vec2> vertices) { Vertices = vertices; return this; }
        public PolygonShape WithRadius(float radius) { Radius = radius; return this; }
    }

    public sealed class ChainSegmentShape : Shape
    {
        public override ShapeType Type => ShapeType.ChainSegment;
        public Vec2 Point1 { get; private set; }
        public Vec2 Point2 { get; private set; }
        public Vec2 Ghost1 { get; private set; }
        public Vec2 Ghost2 { get; private set; }

        public ChainSegmentShape(Vec2 point1, Vec2 point2, Vec2 ghost1, Vec2 ghost2)
        {
            Point1 = point1;
            Point2 = point2;
            Ghost1 = ghost1;
            Ghost2 = ghost2;
        }

        public ChainSegmentShape WithPoints(Vec2 point1, Vec2 point2) { Point1 = point1; Point2 = point2; return this; }
        public ChainSegmentShape WithGhosts(Vec2 ghost1, Vec2 ghost2) { Ghost1 = ghost1; Ghost2 = ghost2; return this; }
    }
}
