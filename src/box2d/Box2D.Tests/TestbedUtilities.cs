using System;
using System.Diagnostics;
using Box2D.Collision;
using Box2D.Common;

namespace Box2D.TestBed
{
    public sealed class DebugDraw : b2Draw
    {
        public readonly struct DebugText
        {
            public DebugText(int x, int y, string text)
            {
                X = x;
                Y = y;
                Text = text;
            }

            public int X { get; }
            public int Y { get; }
            public string Text { get; }
        }

        public readonly struct DebugSegment
        {
            public DebugSegment(b2Vec2 p1, b2Vec2 p2, b2Color color)
            {
                P1 = p1;
                P2 = p2;
                Color = color;
            }

            public b2Vec2 P1 { get; }
            public b2Vec2 P2 { get; }
            public b2Color Color { get; }
        }

        public readonly struct DebugPoint
        {
            public DebugPoint(b2Vec2 p, float size, b2Color color)
            {
                P = p;
                Size = size;
                Color = color;
            }

            public b2Vec2 P { get; }
            public float Size { get; }
            public b2Color Color { get; }
        }

        private readonly System.Collections.Generic.List<DebugText> _text = new System.Collections.Generic.List<DebugText>();
        private readonly System.Collections.Generic.List<DebugSegment> _segments = new System.Collections.Generic.List<DebugSegment>();
        private readonly System.Collections.Generic.List<DebugPoint> _points = new System.Collections.Generic.List<DebugPoint>();

        public DebugDraw() : base(1)
        {
        }

        public void BeginFrame()
        {
            _text.Clear();
            _segments.Clear();
            _points.Clear();
        }

        public DebugText[] GetTextSnapshot()
        {
            return _text.ToArray();
        }

        public DebugSegment[] GetSegmentSnapshot()
        {
            return _segments.ToArray();
        }

        public DebugPoint[] GetPointSnapshot()
        {
            return _points.ToArray();
        }

        public override void DrawPolygon(b2Vec2[] vertices, int vertexCount, b2Color color)
        {
        }

        public override void DrawSolidPolygon(b2Vec2[] vertices, int vertexCount, b2Color color)
        {
        }

        public override void DrawCircle(b2Vec2 center, float radius, b2Color color)
        {
        }

        public override void DrawSolidCircle(b2Vec2 center, float radius, b2Vec2 axis, b2Color color)
        {
        }

        public override void DrawSegment(b2Vec2 p1, b2Vec2 p2, b2Color color)
        {
            _segments.Add(new DebugSegment(p1, p2, color));
        }

        public override void DrawTransform(b2Transform xf)
        {
        }

        public void DrawPoint(b2Vec2 p, float size, b2Color color)
        {
            _points.Add(new DebugPoint(p, size, color));
        }

        public void DrawAABB(b2AABB aabb, b2Color color)
        {
        }

        public void DrawString(int x, int y, string format, params object[] args)
        {
            string text = (args == null || args.Length == 0)
                ? format
                : string.Format(format, args);

            _text.Add(new DebugText(x, y, text));

            if (args == null || args.Length == 0)
            {
                Debug.WriteLine(format);
                return;
            }

            Debug.WriteLine(string.Format(format, args));
        }
    }

    public static class Rand
    {
        private static Random _random = new Random(12345);

        public static Random Random => _random;

        public static void Randomize(int seed)
        {
            _random = new Random(seed);
        }

        public static float RandomFloat()
        {
            return (float)_random.NextDouble();
        }

        public static float RandomFloat(float lo, float hi)
        {
            return lo + (float)_random.NextDouble() * (hi - lo);
        }
    }
}
