namespace Box2DNG.Viewer.Samples
{
    public sealed class CarSample : BaseSample
    {
        private Body? _car;
        private Body? _wheel1;
        private Body? _wheel2;
        private float _hz = 12f;
        private float _zeta = 1.0f;
        private float _speed = 100f;
        private WheelJoint? _spring1;
        private WheelJoint? _spring2;

        public override string Name => "Car";

        public override WorldDef CreateWorldDef() => new WorldDef().WithGravity(new Vec2(0f, -10f));

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            FixtureDef groundDef = new FixtureDef(new SegmentShape(new Vec2(-20f, 0f), new Vec2(20f, 0f)))
                .WithDensity(0f)
                .WithFriction(0.6f);
            ground.CreateFixture(groundDef);

            float[] hs = { 0.25f, 1f, 4f, 0f, 0f, -1f, -2f, -2f, -1.25f, 0f };
            float x = 20f;
            float y1 = 0f;
            float dx = 5f;

            for (int i = 0; i < 10; ++i)
            {
                float y2 = hs[i];
                ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(x, y1), new Vec2(x + dx, y2)))
                    .WithDensity(0f)
                    .WithFriction(0.6f));
                y1 = y2;
                x += dx;
            }

            for (int i = 0; i < 10; ++i)
            {
                float y2 = hs[i];
                ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(x, y1), new Vec2(x + dx, y2)))
                    .WithDensity(0f)
                    .WithFriction(0.6f));
                y1 = y2;
                x += dx;
            }

            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(x, 0f), new Vec2(x + 40f, 0f))).WithDensity(0f).WithFriction(0.6f));
            x += 80f;
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(x, 0f), new Vec2(x + 40f, 0f))).WithDensity(0f).WithFriction(0.6f));
            x += 40f;
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(x, 0f), new Vec2(x + 10f, 5f))).WithDensity(0f).WithFriction(0.6f));
            x += 20f;
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(x, 0f), new Vec2(x + 40f, 0f))).WithDensity(0f).WithFriction(0.6f));
            x += 40f;
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(x, 0f), new Vec2(x, 20f))).WithDensity(0f).WithFriction(0.6f));

            {
                Body teeter = world.CreateBody(new BodyDef().AsDynamic().At(140f, 1f));
                PolygonShape box = new PolygonShape(BuildBoxVertices(10f, 0.25f, Vec2.Zero, 0f));
                teeter.CreateFixture(new FixtureDef(box).WithDensity(1f));

                RevoluteJointDef jd = new RevoluteJointDef(ground, teeter, teeter.Transform.P);
                world.CreateJoint(jd);
                teeter.ApplyAngularImpulse(100f);
            }

            {
                int N = 20;
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1f, 0.125f, Vec2.Zero, 0f));
                FixtureDef fd = new FixtureDef(shape).WithDensity(1f).WithFriction(0.6f);
                Body prevBody = ground;
                for (int i = 0; i < N; ++i)
                {
                    Body body = world.CreateBody(new BodyDef().AsDynamic().At(161f + 2f * i, -0.125f));
                    body.CreateFixture(fd);
                    Vec2 anchor = new Vec2(160f + 2f * i, -0.125f);
                    RevoluteJointDef jd = new RevoluteJointDef(prevBody, body, anchor);
                    world.CreateJoint(jd);
                    prevBody = body;
                }
                Vec2 endAnchor = new Vec2(160f + 2f * N, -0.125f);
                world.CreateJoint(new RevoluteJointDef(prevBody, ground, endAnchor));
            }

            {
                PolygonShape box = new PolygonShape(BuildBoxVertices(0.5f, 0.5f, Vec2.Zero, 0f));
                for (int i = 0; i < 5; ++i)
                {
                    Body body = world.CreateBody(new BodyDef().AsDynamic().At(230f, 0.5f + i));
                    body.CreateFixture(new FixtureDef(box).WithDensity(0.5f));
                }
            }

            {
                Filter carFilter = new Filter(1, ulong.MaxValue, -1);
                PolygonShape chassis = new PolygonShape(new[]
                {
                    new Vec2(-1.5f, -0.5f),
                    new Vec2(1.5f, -0.5f),
                    new Vec2(1.5f, 0f),
                    new Vec2(0f, 0.9f),
                    new Vec2(-1.15f, 0.9f),
                    new Vec2(-1.5f, 0.2f)
                });
                CircleShape wheel = new CircleShape(0.4f);

                _car = world.CreateBody(new BodyDef()
                    .AsDynamic()
                    .At(0f, 1f)
                    .WithLinearDamping(0.6f)
                    .WithAngularDamping(0.2f));
                _car.CreateFixture(new FixtureDef(chassis).WithDensity(1f).WithFilter(carFilter));

                FixtureDef wheelDef = new FixtureDef(wheel).WithDensity(1f).WithFriction(0.9f).WithFilter(carFilter);
                _wheel1 = world.CreateBody(new BodyDef()
                    .AsDynamic()
                    .At(-1f, 0.35f)
                    .WithLinearDamping(0.6f)
                    .WithAngularDamping(0.2f));
                _wheel1.CreateFixture(wheelDef);
                _wheel2 = world.CreateBody(new BodyDef()
                    .AsDynamic()
                    .At(1f, 0.4f)
                    .WithLinearDamping(0.6f)
                    .WithAngularDamping(0.2f));
                _wheel2.CreateFixture(wheelDef);

                Vec2 axis = new Vec2(0f, 1f);
                WheelJointDef jd = new WheelJointDef(_car, _wheel1, _wheel1.Transform.P, axis)
                    .WithMotor(true, 0f, 400f)
                    .WithSpring(_hz, _zeta)
                    .WithLimits(-0.3f, 0.3f);
                _spring1 = world.CreateJoint(jd);

                jd = new WheelJointDef(_car, _wheel2, _wheel2.Transform.P, axis)
                    .WithMotor(false, 0f, 200f)
                    .WithSpring(_hz, _zeta)
                    .WithLimits(-0.3f, 0.3f);
                _spring2 = world.CreateJoint(jd);
            }
        }

        public override void OnKey(char key)
        {
            if (_spring1 == null || _spring2 == null)
            {
                return;
            }

            switch (key)
            {
                case 'a':
                    _spring1.SetMotorSpeed(-_speed);
                    break;
                case 's':
                    _spring1.SetMotorSpeed(0f);
                    break;
                case 'd':
                    _spring1.SetMotorSpeed(_speed);
                    break;
                case 'q':
                    _hz = MathF.Max(0f, _hz - 1f);
                    _spring1.SetSpringFrequencyHz(_hz);
                    _spring2.SetSpringFrequencyHz(_hz);
                    break;
                case 'e':
                    _hz += 1f;
                    _spring1.SetSpringFrequencyHz(_hz);
                    _spring2.SetSpringFrequencyHz(_hz);
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
