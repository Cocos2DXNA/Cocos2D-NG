namespace Box2DNG.Viewer.Samples
{
    public sealed class DominosSample : BaseSample
    {
        public override string Name => "Dominos";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            {
                PolygonShape platform = new PolygonShape(BuildBoxVertices(6f, 0.25f, new Vec2(-1.5f, 10f), 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
                body.CreateFixture(new FixtureDef(platform).WithDensity(1f));
            }

            PolygonShape dominoShape = new PolygonShape(BuildBoxVertices(0.1f, 1f, Vec2.Zero, 0f));
            FixtureDef dominoDef = new FixtureDef(dominoShape).WithDensity(10f).WithFriction(0.1f);

            for (int i = 0; i < 10; ++i)
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-6f + 1f * i, 11.25f));
                body.CreateFixture(dominoDef);
            }

            Body baseBody = world.CreateBody(new BodyDef().AsDynamic().At(0f, 11f));
            PolygonShape box = new PolygonShape(BuildBoxVertices(0.5f, 0.5f, Vec2.Zero, 0f));
            baseBody.CreateFixture(new FixtureDef(box).WithDensity(5f));

            Body pendulum = world.CreateBody(new BodyDef().AsDynamic().At(2f, 12.5f));
            PolygonShape rod = new PolygonShape(BuildBoxVertices(0.1f, 1.5f, Vec2.Zero, 0f));
            pendulum.CreateFixture(new FixtureDef(rod).WithDensity(2f));
            world.CreateJoint(new RevoluteJointDef(baseBody, pendulum, new Vec2(2f, 11f)));
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
