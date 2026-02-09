namespace Box2DNG.Viewer.Samples
{
    public sealed class VaryingRestitutionSample : BaseSample
    {
        public override string Name => "VaryingRestitution";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            CircleShape circle = new CircleShape(1f);
            float[] restitutions = { 0f, 0.1f, 0.3f, 0.5f, 0.75f, 0.9f, 1f };

            for (int i = 0; i < restitutions.Length; ++i)
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-12f + 4f * i, 10f));
                body.CreateFixture(new FixtureDef(circle).WithDensity(1f).WithRestitution(restitutions[i]));
            }
        }
    }
}
