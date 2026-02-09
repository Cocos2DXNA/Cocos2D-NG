namespace Box2DNG.Viewer.Samples
{
    public sealed class VaryingFrictionSample : BaseSample
    {
        public override string Name => "VaryingFriction";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            PolygonShape boxShape = new PolygonShape(BuildBoxVertices(0.5f, 0.5f, Vec2.Zero, 0f));
            float[] frictions = { 0.75f, 0.5f, 0.35f, 0.1f, 0.0f };

            for (int i = 0; i < frictions.Length; ++i)
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-10f + 5f * i, 2f + 2f * i));
                body.CreateFixture(new FixtureDef(boxShape).WithDensity(1f).WithFriction(frictions[i]));
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
