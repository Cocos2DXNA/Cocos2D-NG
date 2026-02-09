using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class TheoJansenSample : BaseSample
    {
        private Vec2 _offset = new Vec2(0f, 8f);
        private Body? _chassis;
        private Body? _wheel;
        private RevoluteJoint? _motorJoint;
        private float _motorSpeed = 2f;
        private bool _motorOn = true;

        public override string Name => "TheoJansen";

        public override void Build(World world)
        {
            Vec2 pivot = new Vec2(0f, 0.8f);

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-50f, 0f), new Vec2(50f, 0f))));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-50f, 0f), new Vec2(-50f, 10f))));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(50f, 0f), new Vec2(50f, 10f))));

            for (int i = 0; i < 40; ++i)
            {
                CircleShape shape = new CircleShape(0.25f);
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-40f + 2f * i, 0.5f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(1f));
            }

            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(2.5f, 1f, Vec2.Zero, 0f));
                FixtureDef fd = new FixtureDef(shape).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(pivot + _offset));
                body.CreateFixture(fd);
                _chassis = body;
            }

            {
                CircleShape shape = new CircleShape(1.6f);
                FixtureDef fd = new FixtureDef(shape).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(pivot + _offset));
                body.CreateFixture(fd);
                _wheel = body;
            }

            if (_wheel == null || _chassis == null)
            {
                return;
            }

            RevoluteJointDef jd = new RevoluteJointDef(_wheel, _chassis, pivot + _offset)
                .WithMotor(_motorOn, _motorSpeed, 400f);
            _motorJoint = world.CreateJoint(jd);

            Vec2 wheelAnchor = pivot + new Vec2(0f, -0.8f);

            CreateLeg(world, -1f, wheelAnchor);
            CreateLeg(world, 1f, wheelAnchor);

            _wheel.SetTransform(_wheel.Transform.P, 120f * MathFng.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor);
            CreateLeg(world, 1f, wheelAnchor);

            _wheel.SetTransform(_wheel.Transform.P, -120f * MathFng.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor);
            CreateLeg(world, 1f, wheelAnchor);
        }

        public override void OnKey(char key)
        {
            if (_motorJoint == null)
            {
                return;
            }

            switch (key)
            {
                case 'a':
                    _motorJoint.SetMotorSpeed(-_motorSpeed);
                    break;
                case 's':
                    _motorJoint.SetMotorSpeed(0f);
                    break;
                case 'd':
                    _motorJoint.SetMotorSpeed(_motorSpeed);
                    break;
                case 'm':
                    _motorOn = !_motorOn;
                    _motorJoint.SetMotorEnabled(_motorOn);
                    break;
            }
        }

        private void CreateLeg(World world, float s, Vec2 wheelAnchor)
        {
            if (_chassis == null || _wheel == null)
            {
                return;
            }

            Vec2 p1 = new Vec2(5.4f * s, -6.1f);
            Vec2 p2 = new Vec2(7.2f * s, -1.2f);
            Vec2 p3 = new Vec2(4.3f * s, -1.9f);
            Vec2 p4 = new Vec2(3.1f * s, 0.8f);
            Vec2 p5 = new Vec2(6.0f * s, 1.5f);
            Vec2 p6 = new Vec2(2.5f * s, 3.7f);

            FixtureDef fd1 = new FixtureDef(new PolygonShape(Array.Empty<Vec2>()))
                .WithFilter(new Filter(1, ulong.MaxValue, -1))
                .WithDensity(1f);
            FixtureDef fd2 = new FixtureDef(new PolygonShape(Array.Empty<Vec2>()))
                .WithFilter(new Filter(1, ulong.MaxValue, -1))
                .WithDensity(1f);

            PolygonShape poly1;
            PolygonShape poly2;

            if (s > 0f)
            {
                poly1 = new PolygonShape(new[] { p1, p2, p3 });
                poly2 = new PolygonShape(new[] { Vec2.Zero, p5 - p4, p6 - p4 });
            }
            else
            {
                poly1 = new PolygonShape(new[] { p1, p3, p2 });
                poly2 = new PolygonShape(new[] { Vec2.Zero, p6 - p4, p5 - p4 });
            }

            fd1 = new FixtureDef(poly1).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1));
            fd2 = new FixtureDef(poly2).WithDensity(1f).WithFilter(new Filter(1, ulong.MaxValue, -1));

            Body body1 = world.CreateBody(new BodyDef().AsDynamic().At(_offset).WithAngularDamping(10f));
            Body body2 = world.CreateBody(new BodyDef().AsDynamic().At(p4 + _offset).WithAngularDamping(10f));
            body1.CreateFixture(fd1);
            body2.CreateFixture(fd2);

            DistanceJointDef djd = new DistanceJointDef(body1, body2, p2 + _offset, p5 + _offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            djd = new DistanceJointDef(body1, body2, p3 + _offset, p4 + _offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            djd = new DistanceJointDef(body1, _wheel, p3 + _offset, wheelAnchor + _offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            djd = new DistanceJointDef(body2, _wheel, p6 + _offset, wheelAnchor + _offset)
                .WithDampingRatio(0.5f)
                .WithFrequency(10f);
            world.CreateJoint(djd);

            RevoluteJointDef rjd = new RevoluteJointDef(body2, _chassis, p4 + _offset);
            world.CreateJoint(rjd);
        }

        private static Vec2[] BuildBoxVertices(float hx, float hy, Vec2 center, float angle)
        {
            Vec2[] verts =
            {
                new Vec2(-hx, -hy),
                new Vec2(hx, -hy),
                new Vec2(hx, hy),
                new Vec2(-hx, hy)
            };
            if (angle == 0f)
            {
                for (int i = 0; i < verts.Length; ++i)
                {
                    verts[i] = verts[i] + center;
                }
                return verts;
            }

            Rot rot = new Rot(angle);
            for (int i = 0; i < verts.Length; ++i)
            {
                verts[i] = Rot.Mul(rot, verts[i]) + center;
            }
            return verts;
        }
    }
}
