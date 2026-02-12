using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class BulletTestSample : BaseSample
    {
        private readonly Random _rng = new Random(1234);
        private Body? _body;
        private Body? _bullet;
        private float _x;
        private int _stepCount;

        public override string Name => "BulletTest";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-10f, 0f), new Vec2(10f, 0f))));

            PolygonShape column = new PolygonShape(new[]
            {
                new Vec2(0.3f, 0f),
                new Vec2(0.7f, 0f),
                new Vec2(0.7f, 2f),
                new Vec2(0.3f, 2f)
            });
            ground.CreateFixture(new FixtureDef(column).WithDensity(0f));

            _body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 4f));
            PolygonShape platform = new PolygonShape(new[]
            {
                new Vec2(-2f, -0.1f),
                new Vec2(2f, -0.1f),
                new Vec2(2f, 0.1f),
                new Vec2(-2f, 0.1f)
            });
            _body.CreateFixture(new FixtureDef(platform).WithDensity(1f));

            PolygonShape bulletShape = new PolygonShape(new[]
            {
                new Vec2(-0.25f, -0.25f),
                new Vec2(0.25f, -0.25f),
                new Vec2(0.25f, 0.25f),
                new Vec2(-0.25f, 0.25f)
            });

            _x = 0.20352793f;
            _bullet = world.CreateBody(new BodyDef().AsDynamic().At(_x, 10f).IsBullet(true));
            _bullet.CreateFixture(new FixtureDef(bulletShape).WithDensity(100f));
            _bullet.LinearVelocity = new Vec2(0f, -50f);
        }

        public override void Step(World world, float dt)
        {
            _stepCount++;
            if (_stepCount % 60 == 0)
            {
                Launch();
            }
        }

        private void Launch()
        {
            if (_body == null || _bullet == null)
            {
                return;
            }

            _body.SetTransform(new Vec2(0f, 4f), 0f);
            _body.LinearVelocity = Vec2.Zero;
            _body.AngularVelocity = 0f;

            _x = RandomRange(-1f, 1f);
            _bullet.SetTransform(new Vec2(_x, 10f), 0f);
            _bullet.LinearVelocity = new Vec2(0f, -50f);
            _bullet.AngularVelocity = 0f;
        }

        private float RandomRange(float min, float max)
        {
            return (float)(min + _rng.NextDouble() * (max - min));
        }
    }
}
