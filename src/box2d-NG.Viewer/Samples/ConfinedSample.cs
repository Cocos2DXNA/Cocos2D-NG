namespace Box2DNG.Viewer.Samples
{
    public sealed class ConfinedSample : BaseSample
    {
        private World? _world;
        private int _spawnCount;

        public override string Name => "Confined";

        public override WorldDef CreateWorldDef() => new WorldDef().WithGravity(Vec2.Zero);

        public override void Build(World world)
        {
            _world = world;

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-10f, 0f), new Vec2(10f, 0f))));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-10f, 0f), new Vec2(-10f, 20f))));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(10f, 0f), new Vec2(10f, 20f))));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-10f, 20f), new Vec2(10f, 20f))));
        }

        public override void OnKey(char key)
        {
            if (key == 'c')
            {
                CreateCircle();
            }
        }

        private void CreateCircle()
        {
            if (_world == null)
            {
                return;
            }

            const float radius = 2f;
            CircleShape shape = new CircleShape(radius);
            FixtureDef fd = new FixtureDef(shape).WithDensity(1f).WithFriction(0f);

            float x = -9f + 18f * (_spawnCount % 5) / 4f;
            float y = 3f + 1.5f * (_spawnCount / 5);
            _spawnCount++;

            Body body = _world.CreateBody(new BodyDef().AsDynamic().At(x, y));
            body.CreateFixture(fd);
        }
    }
}
