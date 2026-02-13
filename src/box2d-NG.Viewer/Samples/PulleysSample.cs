namespace Box2DNG.Viewer.Samples
{
    public sealed class PulleysSample : BaseSample
    {
        public override string Name => "Pulleys";

        public override void Build(World world)
        {
            float y = 16f;
            float L = 12f;
            float a = 1f;
            float b = 2f;

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            CircleShape pulley = new CircleShape(2f);
            pulley = pulley.WithCenter(new Vec2(-10f, y + b + L));
            ground.CreateFixture(new FixtureDef(pulley));
            pulley = pulley.WithCenter(new Vec2(10f, y + b + L));
            ground.CreateFixture(new FixtureDef(pulley));

            Body box1 = world.CreateBody(new BodyDef().AsDynamic().At(-10f, y));
            Body box2 = world.CreateBody(new BodyDef().AsDynamic().At(10f, y));
            PolygonShape box = new PolygonShape(BuildBoxVertices(a, b, Vec2.Zero, 0f));
            box1.CreateFixture(new FixtureDef(box).WithDensity(5f));
            box2.CreateFixture(new FixtureDef(box).WithDensity(5f));

            Vec2 anchorA = new Vec2(-10f, y + b);
            Vec2 anchorB = new Vec2(10f, y + b);
            Vec2 groundAnchorA = new Vec2(-10f, y + b + L);
            Vec2 groundAnchorB = new Vec2(10f, y + b + L);

            PulleyJointDef jd = new PulleyJointDef(box1, box2, groundAnchorA, groundAnchorB, anchorA, anchorB, 1.5f);
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
