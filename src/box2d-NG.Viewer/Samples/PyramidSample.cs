namespace Box2DNG.Viewer.Samples
{
    public sealed class PyramidSample : BaseSample
    {
        private const int Count = 20;

        public override string Name => "Pyramid";
        public override int SubSteps => 1;

        public override WorldDef CreateWorldDef()
        {
            return new WorldDef().WithGravity(new Vec2(0f, -10f));
        }

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            float a = 0.5f;
            PolygonShape shape = new PolygonShape(BuildBoxVertices(a, a, Vec2.Zero, 0f));

            Vec2 x = new Vec2(-7f, 0.75f);
            Vec2 deltaX = new Vec2(0.5625f, 1.25f);
            Vec2 deltaY = new Vec2(1.125f, 0f);

            for (int i = 0; i < Count; ++i)
            {
                Vec2 y = x;
                for (int j = i; j < Count; ++j)
                {
                    Body body = world.CreateBody(new BodyDef().AsDynamic().At(y));
                    body.CreateFixture(new FixtureDef(shape).WithDensity(5f));
                    y += deltaY;
                }
                x += deltaX;
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
