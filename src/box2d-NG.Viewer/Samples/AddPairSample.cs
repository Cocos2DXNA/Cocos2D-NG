using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class AddPairSample : BaseSample
    {
        private bool _doCircle;

        public override string Name => "AddPair";

        public override WorldDef CreateWorldDef() => new WorldDef().WithGravity(new Vec2(0f, 0f));

        public override void Build(World world)
        {
            CircleShape small = new CircleShape(0.1f);

            float minX = -6f;
            float maxX = 0f;
            float minY = 4f;
            float maxY = 6f;

            Random rand = new Random(1234);
            for (int i = 0; i < 400; ++i)
            {
                Body body = world.CreateBody(new BodyDef()
                    .AsDynamic()
                    .At(RandomRange(rand, minX, maxX), RandomRange(rand, minY, maxY)));
                body.CreateFixture(new FixtureDef(small).WithDensity(0.01f));
            }

            Shape bigShape = _doCircle
                ? new CircleShape(1.5f)
                : new PolygonShape(new[]
                {
                    new Vec2(-1.6f, -1.6f),
                    new Vec2(1.6f, -1.6f),
                    new Vec2(1.6f, 1.6f),
                    new Vec2(-1.6f, 1.6f)
                });

            Body bullet = world.CreateBody(new BodyDef()
                .AsDynamic()
                .At(-40f, 5f)
                .IsBullet(true));
            bullet.CreateFixture(new FixtureDef(bigShape).WithDensity(1f));
            bullet.LinearVelocity = new Vec2(150f, 0f);
        }

        public override void OnKey(char key)
        {
            if (key == 'c')
            {
                _doCircle = true;
            }
            else if (key == 'b')
            {
                _doCircle = false;
            }
        }

        private static float RandomRange(Random rand, float min, float max)
        {
            return (float)(min + rand.NextDouble() * (max - min));
        }
    }
}
