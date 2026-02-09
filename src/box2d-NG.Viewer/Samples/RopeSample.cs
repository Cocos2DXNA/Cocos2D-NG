namespace Box2DNG.Viewer.Samples
{
    public sealed class RopeSample : BaseSample
    {
        public override string Name => "Rope";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body anchor = world.CreateBody(new BodyDef().AsStatic().At(0f, 15f));
            Body box = world.CreateBody(new BodyDef().AsDynamic().At(0f, 5f));
            PolygonShape boxShape = new PolygonShape(BuildBoxVertices(1f, 1f, Vec2.Zero, 0f));
            box.CreateFixture(new FixtureDef(boxShape).WithDensity(5f));

            RopeJointDef jd = new RopeJointDef(anchor, box, anchor.Transform.P, box.Transform.P)
                .WithMaxLength(12f);
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
