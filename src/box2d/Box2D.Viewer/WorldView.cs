using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Box2D.Common;
using Box2D.Collision.Shapes;
using Box2D.Dynamics;
using Box2D.TestBed;

namespace Box2D.Viewer
{
    public sealed class WorldView : Control
    {
        private const float PixelsPerMeter = 30.0f;
        private readonly Pen _staticPen = new Pen(Color.FromArgb(140, 160, 165), 1.5f);
        private readonly Pen _dynamicPen = new Pen(Color.FromArgb(82, 170, 255), 1.5f);
        private readonly Brush _dynamicFill = new SolidBrush(Color.FromArgb(70, 82, 170, 255));
        private readonly Brush _staticFill = new SolidBrush(Color.FromArgb(50, 140, 160, 165));
        private readonly Font _overlayFont = new Font("Consolas", 10.0f, FontStyle.Regular);
        private readonly Brush _overlayBrush = new SolidBrush(Color.FromArgb(230, 235, 240));
        private readonly Pen _debugPen = new Pen(Color.FromArgb(220, 200, 200, 200), 1.5f);

        public b2World World { get; set; }
        public DebugDraw.DebugText[] OverlayText { get; set; }
        public DebugDraw.DebugSegment[] DebugSegments { get; set; }
        public DebugDraw.DebugPoint[] DebugPoints { get; set; }

        public WorldView()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (World == null)
            {
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (b2Body body = World.BodyList; body != null; body = body.Next)
            {
                for (b2Fixture fixture = body.FixtureList; fixture != null; fixture = fixture.Next)
                {
                    DrawFixture(g, body, fixture);
                }
            }

            DrawDebugSegments(g);
            DrawDebugPoints(g);
            DrawOverlayText(g);
        }

        private void DrawFixture(Graphics g, b2Body body, b2Fixture fixture)
        {
            b2Transform xf = body.Transform;
            bool isStatic = body.BodyType == b2BodyType.b2_staticBody;
            Pen pen = isStatic ? _staticPen : _dynamicPen;
            Brush fill = isStatic ? _staticFill : _dynamicFill;

            switch (fixture.ShapeType)
            {
                case b2ShapeType.e_circle:
                    DrawCircle(g, xf, (b2CircleShape)fixture.Shape, pen, fill);
                    break;
                case b2ShapeType.e_polygon:
                    DrawPolygon(g, xf, (b2PolygonShape)fixture.Shape, pen, fill);
                    break;
                case b2ShapeType.e_edge:
                    DrawEdge(g, xf, (b2EdgeShape)fixture.Shape, pen);
                    break;
                case b2ShapeType.e_chain:
                    DrawChain(g, xf, (b2ChainShape)fixture.Shape, pen);
                    break;
            }
        }

        private void DrawCircle(Graphics g, b2Transform xf, b2CircleShape circle, Pen pen, Brush fill)
        {
            b2Vec2 center = b2Math.b2Mul(ref xf, ref circle.Position);
            PointF screen = ToScreen(center);
            float radius = circle.Radius * PixelsPerMeter;
            RectangleF rect = new RectangleF(screen.X - radius, screen.Y - radius, radius * 2.0f, radius * 2.0f);
            g.FillEllipse(fill, rect);
            g.DrawEllipse(pen, rect);
        }

        private void DrawPolygon(Graphics g, b2Transform xf, b2PolygonShape polygon, Pen pen, Brush fill)
        {
            int count = polygon.VertexCount;
            if (count <= 0)
            {
                return;
            }

            PointF[] points = new PointF[count];
            for (int i = 0; i < count; ++i)
            {
                b2Vec2 vertex = polygon.Vertices[i];
                b2Vec2 worldVertex = b2Math.b2Mul(ref xf, ref vertex);
                points[i] = ToScreen(worldVertex);
            }

            g.FillPolygon(fill, points);
            g.DrawPolygon(pen, points);
        }

        private void DrawEdge(Graphics g, b2Transform xf, b2EdgeShape edge, Pen pen)
        {
            b2Vec2 v1 = b2Math.b2Mul(ref xf, ref edge.Vertex1);
            b2Vec2 v2 = b2Math.b2Mul(ref xf, ref edge.Vertex2);
            PointF p1 = ToScreen(v1);
            PointF p2 = ToScreen(v2);
            g.DrawLine(pen, p1, p2);
        }

        private void DrawChain(Graphics g, b2Transform xf, b2ChainShape chain, Pen pen)
        {
            if (chain.Vertices == null || chain.Count < 2)
            {
                return;
            }

            for (int i = 1; i < chain.Count; ++i)
            {
                b2Vec2 v1 = b2Math.b2Mul(ref xf, ref chain.Vertices[i - 1]);
                b2Vec2 v2 = b2Math.b2Mul(ref xf, ref chain.Vertices[i]);
                PointF p1 = ToScreen(v1);
                PointF p2 = ToScreen(v2);
                g.DrawLine(pen, p1, p2);
            }
        }

        private PointF ToScreen(b2Vec2 world)
        {
            float originX = ClientSize.Width * 0.5f;
            float originY = ClientSize.Height * 0.85f;
            return new PointF(originX + world.x * PixelsPerMeter, originY - world.y * PixelsPerMeter);
        }

        public b2Vec2 ScreenToWorld(Point point)
        {
            float originX = ClientSize.Width * 0.5f;
            float originY = ClientSize.Height * 0.85f;
            return new b2Vec2((point.X - originX) / PixelsPerMeter, (originY - point.Y) / PixelsPerMeter);
        }

        private void DrawDebugSegments(Graphics g)
        {
            if (DebugSegments == null || DebugSegments.Length == 0)
            {
                return;
            }

            foreach (var segment in DebugSegments)
            {
                _debugPen.Color = ToColor(segment.Color, 200);
                PointF p1 = ToScreen(segment.P1);
                PointF p2 = ToScreen(segment.P2);
                g.DrawLine(_debugPen, p1, p2);
            }
        }

        private void DrawDebugPoints(Graphics g)
        {
            if (DebugPoints == null || DebugPoints.Length == 0)
            {
                return;
            }

            foreach (var point in DebugPoints)
            {
                using (Brush brush = new SolidBrush(ToColor(point.Color, 230)))
                {
                    PointF screen = ToScreen(point.P);
                    float size = point.Size;
                    g.FillEllipse(brush, screen.X - size * 0.5f, screen.Y - size * 0.5f, size, size);
                }
            }
        }

        private static Color ToColor(b2Color color, int alpha)
        {
            int r = ClampToByte(color.r * 255.0f);
            int g = ClampToByte(color.g * 255.0f);
            int b = ClampToByte(color.b * 255.0f);
            return Color.FromArgb(alpha, r, g, b);
        }

        private static int ClampToByte(float value)
        {
            if (value < 0.0f)
            {
                return 0;
            }
            if (value > 255.0f)
            {
                return 255;
            }
            return (int)value;
        }

        private void DrawOverlayText(Graphics g)
        {
            if (OverlayText == null || OverlayText.Length == 0)
            {
                return;
            }

            foreach (var line in OverlayText)
            {
                g.DrawString(line.Text, _overlayFont, _overlayBrush, line.X, line.Y);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _staticPen.Dispose();
                _dynamicPen.Dispose();
                _dynamicFill.Dispose();
                _staticFill.Dispose();
                _overlayFont.Dispose();
                _overlayBrush.Dispose();
                _debugPen.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
