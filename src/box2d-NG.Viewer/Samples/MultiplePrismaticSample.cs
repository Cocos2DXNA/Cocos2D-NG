namespace Box2DNG.Viewer.Samples
{
    public sealed class MultiplePrismaticSample : BaseSample
    {
        public override string Name => "Multiple Prismatic";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            Body previous = ground;
            Vec2 axis = new Vec2(0f, 1f);
            for (int i = 0; i < 6; ++i)
            {
                float y = 0.6f + 1.2f * i;
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, y));
                PolygonShape box = new PolygonShape(BuildBoxVertices(0.5f, 0.5f, Vec2.Zero, 0f));
                body.CreateFixture(new FixtureDef(box));

                Vec2 anchor = new Vec2(0f, y - 0.6f);
                PrismaticJointDef jointDef = new PrismaticJointDef(previous, body, anchor, axis)
                    .WithLimit(-6f, 6f);
                world.CreateJoint(jointDef);

                previous = body;
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
