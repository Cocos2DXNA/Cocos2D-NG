namespace Box2DNG.Viewer.Samples
{
    public sealed class RopeJointSample : BaseSample
    {
        private RopeJointDef? _ropeDef;

        public override string Name => "RopeJoint";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 0.125f, Vec2.Zero, 0f));
            FixtureDef fd = new FixtureDef(shape)
                .WithDensity(20f)
                .WithFriction(0.2f)
                .WithFilter(new Filter(0x0001, 0xFFFF & ~0x0002, 0));

            const int count = 10;
            const float y = 15f;

            Body prevBody = ground;
            Body lastBody = ground;

            for (int i = 0; i < count; ++i)
            {
                BodyDef bd = new BodyDef().AsDynamic().At(0.5f + 1.0f * i, y);
                if (i == count - 1)
                {
                    shape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                    fd = new FixtureDef(shape)
                        .WithDensity(100f)
                        .WithFilter(new Filter(0x0002, 0xFFFF, 0));
                    bd = bd.At(1.0f * i, y).WithAngularDamping(0.4f);
                }

                Body body = world.CreateBody(bd);
                body.CreateFixture(fd);

                Vec2 anchor = new Vec2(i, y);
                RevoluteJointDef jd = new RevoluteJointDef(prevBody, body, anchor);
                world.CreateJoint(jd);

                prevBody = body;
                lastBody = body;
            }

            _ropeDef = new RopeJointDef(ground, lastBody, new Vec2(0f, y), Vec2.Zero)
                .WithMaxLength(count - 1.0f + 0.01f);
            world.CreateJoint(_ropeDef);
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
