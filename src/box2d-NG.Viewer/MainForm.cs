using System;
using System.Drawing;
using System.Windows.Forms;
using Timer= System.Windows.Forms.Timer;

namespace Box2DNG.Viewer
{
    public sealed class MainForm : Form
    {
        private World _world;
        private readonly Timer _timer;
        private readonly ComboBox _sampleSelector;
        private readonly Samples.ISample[] _samples;
        private Samples.ISample _sample;
        private const float PixelsPerMeter = 60f;

        public MainForm()
        {
            Text = "Box2D-NG Viewer";
            DoubleBuffered = true;
            ClientSize = new Size(1000, 700);

            _samples = (Samples.ISample[])Samples.SampleCatalog.All;
            _sample = _samples[0];
            _world = CreateWorld(_sample);

            _sampleSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = 10,
                Top = 10,
                Width = 200
            };
            foreach (Samples.ISample sample in _samples)
            {
                _sampleSelector.Items.Add(sample.Name);
            }
            _sampleSelector.SelectedIndex = 0;
            _sampleSelector.SelectedIndexChanged += (_, __) =>
            {
                int index = _sampleSelector.SelectedIndex;
                if (index < 0 || index >= _samples.Length)
                {
                    return;
                }
                _sample = _samples[index];
                _world = CreateWorld(_sample);
            };
            Controls.Add(_sampleSelector);

            _timer = new Timer { Interval = 16 };
            _timer.Tick += (_, __) =>
            {
                int subSteps = Math.Max(1, _sample.SubSteps);
                float dt = (1f / 60f) / subSteps;
                for (int i = 0; i < subSteps; ++i)
                {
                    _sample.Step(_world, dt);
                    _world.Step(dt);
                }
                Invalidate();
            };
            _timer.Start();

            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                char key = char.ToLowerInvariant((char)e.KeyCode);
                _sample.OnKey(key);
            };
        }

        private static World CreateWorld(Samples.ISample sample)
        {
            World world = new World(sample.CreateWorldDef());
            sample.Build(world);
            return world;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Pen pen = new Pen(Color.Black, 2f);
            using Brush brush = new SolidBrush(Color.FromArgb(120, 70, 140, 200));

            foreach (Body body in _world.Bodies)
            {
                foreach (Fixture fixture in body.Fixtures)
                {
                    DrawFixture(e.Graphics, pen, brush, body, fixture);
                }
            }

            DrawPrismaticJoints(e.Graphics);
            DrawDiagnostics(e.Graphics);
        }

        private void DrawFixture(Graphics g, Pen pen, Brush brush, Body body, Fixture fixture)
        {
            switch (fixture.Shape.Type)
            {
                case ShapeType.Circle:
                    DrawCircle(g, pen, brush, body, (CircleShape)fixture.Shape);
                    break;
                case ShapeType.Polygon:
                    DrawPolygon(g, pen, brush, body, (PolygonShape)fixture.Shape);
                    break;
                case ShapeType.Capsule:
                    DrawCapsule(g, pen, brush, body, (CapsuleShape)fixture.Shape);
                    break;
                case ShapeType.Segment:
                    DrawSegment(g, pen, body, (SegmentShape)fixture.Shape);
                    break;
                case ShapeType.ChainSegment:
                    DrawChainSegment(g, pen, body, (ChainSegmentShape)fixture.Shape);
                    break;
            }
        }

        private void DrawCircle(Graphics g, Pen pen, Brush brush, Body body, CircleShape shape)
        {
            Vec2 center = Transform.Mul(body.Transform, shape.Center);
            float radius = shape.Radius;
            PointF c = ToScreen(center);
            float r = radius * PixelsPerMeter;
            g.FillEllipse(brush, c.X - r, c.Y - r, r * 2f, r * 2f);
            g.DrawEllipse(pen, c.X - r, c.Y - r, r * 2f, r * 2f);
        }

        private void DrawPolygon(Graphics g, Pen pen, Brush brush, Body body, PolygonShape shape)
        {
            PointF[] points = new PointF[shape.Vertices.Count];
            for (int i = 0; i < shape.Vertices.Count; ++i)
            {
                Vec2 v = Transform.Mul(body.Transform, shape.Vertices[i]);
                points[i] = ToScreen(v);
            }
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }

        private void DrawCapsule(Graphics g, Pen pen, Brush brush, Body body, CapsuleShape shape)
        {
            Vec2 c1 = Transform.Mul(body.Transform, shape.Center1);
            Vec2 c2 = Transform.Mul(body.Transform, shape.Center2);
            float radius = shape.Radius;
            PointF p1 = ToScreen(c1);
            PointF p2 = ToScreen(c2);
            float r = radius * PixelsPerMeter;
            g.DrawLine(pen, p1, p2);
            g.FillEllipse(brush, p1.X - r, p1.Y - r, r * 2f, r * 2f);
            g.FillEllipse(brush, p2.X - r, p2.Y - r, r * 2f, r * 2f);
            g.DrawEllipse(pen, p1.X - r, p1.Y - r, r * 2f, r * 2f);
            g.DrawEllipse(pen, p2.X - r, p2.Y - r, r * 2f, r * 2f);
        }

        private void DrawSegment(Graphics g, Pen pen, Body body, SegmentShape shape)
        {
            Vec2 p1 = Transform.Mul(body.Transform, shape.Point1);
            Vec2 p2 = Transform.Mul(body.Transform, shape.Point2);
            g.DrawLine(pen, ToScreen(p1), ToScreen(p2));
        }

        private void DrawChainSegment(Graphics g, Pen pen, Body body, ChainSegmentShape shape)
        {
            Vec2 p1 = Transform.Mul(body.Transform, shape.Point1);
            Vec2 p2 = Transform.Mul(body.Transform, shape.Point2);
            g.DrawLine(pen, ToScreen(p1), ToScreen(p2));
        }

        private PointF ToScreen(Vec2 worldPoint)
        {
            float x = ClientSize.Width * 0.5f + worldPoint.X * PixelsPerMeter;
            float y = (ClientSize.Height * 0.75f) - worldPoint.Y * PixelsPerMeter;
            return new PointF(x, y);
        }

        private void DrawDiagnostics(Graphics g)
        {
            string text = $"Contacts: {_world.Contacts.Count}";
            using Font font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);
            SizeF size = g.MeasureString(text, font);
            RectangleF rect = new RectangleF(10f, 40f, size.Width + 8f, size.Height + 6f);
            using Brush bg = new SolidBrush(Color.FromArgb(180, 20, 20, 20));
            using Brush fg = new SolidBrush(Color.White);
            g.FillRectangle(bg, rect);
            g.DrawString(text, font, fg, rect.X + 4f, rect.Y + 3f);
        }

        private void DrawPrismaticJoints(Graphics g)
        {
            if (_world.PrismaticJoints.Count == 0)
            {
                return;
            }

            using Pen axisPen = new Pen(Color.FromArgb(120, 80, 80, 80), 2f);
            using Pen limitPen = new Pen(Color.FromArgb(180, 70, 160, 70), 2f);
            using Brush pointBrush = new SolidBrush(Color.FromArgb(200, 80, 80, 80));

            foreach (PrismaticJoint joint in _world.PrismaticJoints)
            {
                Vec2 anchorA = joint.BodyA.GetWorldPoint(joint.LocalAnchorA);
                Vec2 anchorB = joint.BodyB.GetWorldPoint(joint.LocalAnchorB);
                Vec2 axis = Rot.Mul(joint.BodyA.Transform.Q, joint.LocalAxisA).Normalize();

                PointF pA = ToScreen(anchorA);
                PointF pB = ToScreen(anchorB);
                g.DrawLine(axisPen, pA, pB);

                float scale = 0.25f;
                Vec2 perp = new Vec2(-axis.Y, axis.X);
                Vec2 p1 = anchorA - scale * perp;
                Vec2 p2 = anchorA + scale * perp;
                g.DrawLine(axisPen, ToScreen(p1), ToScreen(p2));

                if (joint.EnableLimit)
                {
                    Vec2 lower = anchorA + joint.LowerTranslation * axis;
                    Vec2 upper = anchorA + joint.UpperTranslation * axis;
                    g.DrawLine(limitPen, ToScreen(lower), ToScreen(upper));

                    Vec2 lp1 = lower - scale * perp;
                    Vec2 lp2 = lower + scale * perp;
                    Vec2 up1 = upper - scale * perp;
                    Vec2 up2 = upper + scale * perp;
                    g.DrawLine(limitPen, ToScreen(lp1), ToScreen(lp2));
                    g.DrawLine(limitPen, ToScreen(up1), ToScreen(up2));
                }

                float radius = 3f;
                g.FillEllipse(pointBrush, pA.X - radius, pA.Y - radius, radius * 2f, radius * 2f);
                g.FillEllipse(pointBrush, pB.X - radius, pB.Y - radius, radius * 2f, radius * 2f);
            }
        }
    }
}
