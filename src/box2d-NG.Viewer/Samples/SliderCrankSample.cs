namespace Box2DNG.Viewer.Samples
{
    public sealed class SliderCrankSample : BaseSample
    {
        public override string Name => "SliderCrank";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body prevBody = ground;

            Filter connectedFilter = new Filter(1, ulong.MaxValue, -1);

            // Crank
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 2f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 7f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 5f))
                    .WithMotor(true, MathFng.Pi, 10000f);
                world.CreateJoint(rjd);

                prevBody = body;
            }

            // Follower
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 4f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 13f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 9f));
                world.CreateJoint(rjd);

                prevBody = body;
            }

            // Piston
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 17f).LockRotation(true));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f).WithFilter(connectedFilter));

                RevoluteJointDef rjd = new RevoluteJointDef(prevBody, body, new Vec2(0f, 17f));
                world.CreateJoint(rjd);

                PrismaticJointDef pjd = new PrismaticJointDef(ground, body, new Vec2(0f, 17f), new Vec2(0f, 1f))
                    .WithMotor(0f, 1000f);
                world.CreateJoint(pjd);
            }

            // Payload
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 23f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(2f));
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
