namespace Box2DNG.Viewer.Samples
{
    public sealed class TumblerSample : BaseSample
    {
        private const int Count = 800;
        private int _spawned;

        public override string Name => "Tumbler";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            Body tumbler = world.CreateBody(new BodyDef()
                .AsDynamic()
                .AllowSleeping(false)
                .At(0f, 10f));

            PolygonShape wall = new PolygonShape(BuildBoxVertices(0.5f, 10f, new Vec2(10f, 0f), 0f));
            tumbler.CreateFixture(new FixtureDef(wall).WithDensity(5f));
            wall = new PolygonShape(BuildBoxVertices(0.5f, 10f, new Vec2(-10f, 0f), 0f));
            tumbler.CreateFixture(new FixtureDef(wall).WithDensity(5f));
            wall = new PolygonShape(BuildBoxVertices(10f, 0.5f, new Vec2(0f, 10f), 0f));
            tumbler.CreateFixture(new FixtureDef(wall).WithDensity(5f));
            wall = new PolygonShape(BuildBoxVertices(10f, 0.5f, new Vec2(0f, -10f), 0f));
            tumbler.CreateFixture(new FixtureDef(wall).WithDensity(5f));

            RevoluteJointDef jd = new RevoluteJointDef(ground, tumbler, new Vec2(0f, 10f))
                .WithMotor(true, 0.05f * MathFng.Pi, 1e8f);
            world.CreateJoint(jd);

            _spawned = 0;
        }

        public override void Step(World world, float dt)
        {
            if (_spawned >= Count)
            {
                return;
            }

            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 10f));
            PolygonShape box = new PolygonShape(BuildBoxVertices(0.125f, 0.125f, Vec2.Zero, 0f));
            body.CreateFixture(new FixtureDef(box).WithDensity(1f));
            _spawned++;
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
