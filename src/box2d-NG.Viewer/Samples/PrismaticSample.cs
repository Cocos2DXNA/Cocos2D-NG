namespace Box2DNG.Viewer.Samples
{
    public sealed class PrismaticSample : BaseSample
    {
        public override string Name => "Prismatic";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body platform = world.CreateBody(new BodyDef().AsDynamic().At(0f, 5f));
            PolygonShape box = new PolygonShape(BuildBoxVertices(2f, 0.5f, Vec2.Zero, 0f));
            platform.CreateFixture(new FixtureDef(box).WithDensity(2f).WithFriction(0.3f));

            PrismaticJointDef jd = new PrismaticJointDef(ground, platform, platform.Transform.P, new Vec2(1f, 0f))
                .WithLimit(-10f, 10f)
                .WithMotor(2f, 200f);
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
