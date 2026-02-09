namespace Box2DNG.Viewer.Samples
{
    public sealed class RevoluteSample : BaseSample
    {
        public override string Name => "Revolute";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body bar = world.CreateBody(new BodyDef().AsDynamic().At(0f, 10f));
            PolygonShape barShape = new PolygonShape(BuildBoxVertices(5f, 0.25f, Vec2.Zero, 0f));
            bar.CreateFixture(new FixtureDef(barShape).WithDensity(2f));

            RevoluteJointDef jd = new RevoluteJointDef(ground, bar, new Vec2(0f, 10f))
                .WithMotor(true, 1.5f, 500f);
            world.CreateJoint(jd);
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
