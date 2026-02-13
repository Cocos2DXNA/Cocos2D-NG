namespace Box2DNG.Viewer.Samples
{
    public sealed class EdgeShapesSample : BaseSample
    {
        public override string Name => "EdgeShapes";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-20f, 0f), new Vec2(20f, 0f))));

            Vec2[] chain =
            {
                new Vec2(-20f, 0f),
                new Vec2(-10f, 5f),
                new Vec2(0f, 0f),
                new Vec2(10f, -5f),
                new Vec2(20f, 0f)
            };

            for (int i = 0; i < chain.Length - 1; ++i)
            {
                Vec2 ghost1 = i > 0 ? chain[i - 1] : chain[i];
                Vec2 ghost2 = i + 2 < chain.Length ? chain[i + 2] : chain[i + 1];
                ChainSegmentShape seg = new ChainSegmentShape(chain[i], chain[i + 1], ghost1, ghost2);
                ground.CreateFixture(new FixtureDef(seg));
            }

            CircleShape circle = new CircleShape(0.5f);
            for (int i = 0; i < 5; ++i)
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-15f + 6f * i, 10f));
                body.CreateFixture(new FixtureDef(circle).WithDensity(1f));
            }
        }
    }
}
