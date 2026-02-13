namespace Box2DNG.Viewer.Samples
{
    public sealed class ChainSample : BaseSample
    {
        public override string Name => "Chain";
        public override int SubSteps => 4;

        public override WorldDef CreateWorldDef()
        {
            return new WorldDef()
                .WithGravity(new Vec2(0f, -10f))
                .WithVelocityIterations(12)
                .WithPositionIterations(6)
                .WithMaximumTranslation(0.25f);
        }

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            PolygonShape linkShape = new PolygonShape(new[]
            {
                new Vec2(-0.6f, -0.125f),
                new Vec2(0.6f, -0.125f),
                new Vec2(0.6f, 0.125f),
                new Vec2(-0.6f, 0.125f)
            });
            Filter chainFilter = new Filter(1, ulong.MaxValue, -1);
            FixtureDef fd = new FixtureDef(linkShape)
                .WithDensity(20f)
                .WithFriction(0.4f)
                .WithFilter(chainFilter);

            const float y = 25f;
            Body anchorBody = world.CreateBody(new BodyDef().AsStatic().At(0f, y));
            Body prevBody = anchorBody;
            for (int i = 0; i < 30; ++i)
            {
                Body body = world.CreateBody(new BodyDef()
                    .AsDynamic()
                    .At(0.5f + i, y)
                    .WithLinearDamping(0.1f)
                    .WithAngularDamping(0.1f));
                body.CreateFixture(fd);

                Vec2 anchor = new Vec2(i, y);
                RevoluteJointDef jd = new RevoluteJointDef(prevBody, body, anchor);
                world.CreateJoint(jd);

                prevBody = body;
            }
        }
    }
}
