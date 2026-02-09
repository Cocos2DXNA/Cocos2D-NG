namespace Box2DNG.Viewer.Samples
{
    public sealed class BodyTypesSample : BaseSample
    {
        private Body? _attachment;
        private Body? _platform;
        private float _speed = 3f;

        public override string Name => "BodyTypes";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-20f, 0f), new Vec2(20f, 0f))));

            _attachment = world.CreateBody(new BodyDef().AsDynamic().At(0f, 3f));
            PolygonShape attachmentShape = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -2f),
                new Vec2(0.5f, -2f),
                new Vec2(0.5f, 2f),
                new Vec2(-0.5f, 2f)
            });
            _attachment.CreateFixture(new FixtureDef(attachmentShape).WithDensity(2f));

            _platform = world.CreateBody(new BodyDef().AsDynamic().At(-4f, 5f));
            PolygonShape platformShape = new PolygonShape(BuildBoxVertices(0.5f, 4f, new Vec2(4f, 0f), 0.5f * MathFng.Pi));
            _platform.CreateFixture(new FixtureDef(platformShape).WithDensity(2f).WithFriction(0.6f));

            RevoluteJointDef rjd = new RevoluteJointDef(_attachment, _platform, new Vec2(0f, 5f))
                .WithMotor(true, 0f, 50f);
            world.CreateJoint(rjd);

            PrismaticJointDef pjd = new PrismaticJointDef(ground, _platform, new Vec2(0f, 5f), new Vec2(1f, 0f))
                .WithMotor(0f, 1000f)
                .WithLimit(-10f, 10f);
            world.CreateJoint(pjd);

            Body payload = world.CreateBody(new BodyDef().AsDynamic().At(0f, 8f));
            PolygonShape payloadShape = new PolygonShape(BuildBoxVertices(0.75f, 0.75f, Vec2.Zero, 0f));
            payload.CreateFixture(new FixtureDef(payloadShape).WithDensity(2f).WithFriction(0.6f));
        }

        public override void Step(World world, float dt)
        {
            if (_platform == null)
            {
                return;
            }

            if (_platform.Type == BodyType.Kinematic)
            {
                Vec2 p = _platform.Transform.P;
                Vec2 v = _platform.LinearVelocity;
                if ((p.X < -10f && v.X < 0f) || (p.X > 10f && v.X > 0f))
                {
                    v = new Vec2(-v.X, v.Y);
                    _platform.LinearVelocity = v;
                }
            }
        }

        public override void OnKey(char key)
        {
            if (_platform == null)
            {
                return;
            }

            switch (key)
            {
                case 'd':
                    _platform.SetType(BodyType.Dynamic);
                    break;
                case 's':
                    _platform.SetType(BodyType.Static);
                    break;
                case 'k':
                    _platform.SetType(BodyType.Kinematic);
                    _platform.LinearVelocity = new Vec2(-_speed, 0f);
                    _platform.AngularVelocity = 0f;
                    break;
            }
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
