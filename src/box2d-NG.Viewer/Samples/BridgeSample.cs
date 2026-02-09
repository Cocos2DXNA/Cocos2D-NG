namespace Box2DNG.Viewer.Samples
{
    public sealed class BridgeSample : BaseSample
    {
        private const int Count = 30;
        private Body? _middle;

        public override string Name => "Bridge";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body startAnchorBody = world.CreateBody(new BodyDef().AsStatic().At(-15f, 5f));
            Body endAnchorBody = world.CreateBody(new BodyDef().AsStatic().At(-15f + Count, 5f));

            PolygonShape plankShape = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.125f),
                new Vec2(0.5f, -0.125f),
                new Vec2(0.5f, 0.125f),
                new Vec2(-0.5f, 0.125f)
            });

            FixtureDef plankDef = new FixtureDef(plankShape).WithDensity(20f).WithFriction(0.2f);
            Body prevBody = startAnchorBody;

            for (int i = 0; i < Count; ++i)
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-14.5f + i, 5f));
                body.CreateFixture(plankDef);

                Vec2 anchor = new Vec2(-15f + i, 5f);
                RevoluteJointDef jd = new RevoluteJointDef(prevBody, body, anchor);
                world.CreateJoint(jd);

                if (i == (Count >> 1))
                {
                    _middle = body;
                }

                prevBody = body;
            }

            Vec2 endAnchor = new Vec2(-15f + Count, 5f);
            RevoluteJointDef endJoint = new RevoluteJointDef(prevBody, endAnchorBody, endAnchor);
            world.CreateJoint(endJoint);

            for (int i = 0; i < 2; ++i)
            {
                PolygonShape triShape = new PolygonShape(new[]
                {
                    new Vec2(-0.5f, 0f),
                    new Vec2(0.5f, 0f),
                    new Vec2(0f, 1.5f)
                });
                FixtureDef triDef = new FixtureDef(triShape).WithDensity(1f);
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-8f + 8f * i, 12f));
                body.CreateFixture(triDef);
            }

            for (int i = 0; i < 3; ++i)
            {
                CircleShape shape = new CircleShape(0.5f);
                FixtureDef circleDef = new FixtureDef(shape).WithDensity(1f);
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-6f + 6f * i, 10f));
                body.CreateFixture(circleDef);
            }
        }
    }
}
