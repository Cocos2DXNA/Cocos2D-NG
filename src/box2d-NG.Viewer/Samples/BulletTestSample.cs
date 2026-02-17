using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class BulletTestSample : BaseSample
    {
        private const float BreakImpulse = 35f;
        private readonly Random _rng = new Random(1234);
        private World? _world;
        private Body? _body;
        private Body? _bullet;
        private Fixture? _singlePlank;
        private Fixture? _leftPiece;
        private Fixture? _centerPiece;
        private Fixture? _rightPiece;
        private float _x;
        private int _stepCount;
        private bool _breakableMode;
        private bool _breakPending;

        public override string Name => "BulletTest";

        public override void Build(World world)
        {
            _world = world;
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
            ConfigureTargetPlank(_rng.Next(2) == 0);

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

            world.Events.ContactImpulseEvents += events =>
            {
                if (!_breakableMode || _breakPending || events.Events == null)
                {
                    return;
                }

                for (int i = 0; i < events.Events.Length; ++i)
                {
                    ContactImpulseEvent impulse = events.Events[i];
                    if (impulse.NormalImpulse < BreakImpulse)
                    {
                        continue;
                    }

                    if (IsBreakableTargetTag(impulse.UserDataA) || IsBreakableTargetTag(impulse.UserDataB))
                    {
                        _breakPending = true;
                        break;
                    }
                }
            };
        }

        public override void Step(World world, float dt)
        {
            if (_breakPending)
            {
                BreakTargetPlank();
                _breakPending = false;
            }

            _stepCount++;
            if (_stepCount % 60 == 0)
            {
                Launch();
            }
        }

        private void Launch()
        {
            if (_body == null || _bullet == null || _world == null)
            {
                return;
            }

            _body.SetTransform(new Vec2(0f, 4f), 0f);
            _body.LinearVelocity = Vec2.Zero;
            _body.AngularVelocity = 0f;

            ConfigureTargetPlank(_rng.Next(2) == 0);

            _x = RandomRange(-1f, 1f);
            _bullet.SetTransform(new Vec2(_x, 10f), 0f);
            _bullet.LinearVelocity = new Vec2(0f, -50f);
            _bullet.AngularVelocity = 0f;
        }

        private void ConfigureTargetPlank(bool breakable)
        {
            if (_body == null)
            {
                return;
            }

            while (_body.Fixtures.Count > 0)
            {
                _body.DestroyFixture(_body.Fixtures[0]);
            }

            _singlePlank = null;
            _leftPiece = null;
            _centerPiece = null;
            _rightPiece = null;
            _breakPending = false;
            _breakableMode = breakable;

            if (!breakable)
            {
                PolygonShape platform = new PolygonShape(new[]
                {
                    new Vec2(-2f, -0.1f),
                    new Vec2(2f, -0.1f),
                    new Vec2(2f, 0.1f),
                    new Vec2(-2f, 0.1f)
                });
                _singlePlank = _body.CreateFixture(new FixtureDef(platform).WithDensity(1f).WithUserData("target-single"));
                return;
            }

            float hx = 2f / 3f;
            float hy = 0.1f;
            _leftPiece = _body.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(hx, hy, new Vec2(-4f / 3f, 0f), 0f)))
                .WithDensity(1f)
                .WithUserData("target-break-left"));
            _centerPiece = _body.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(hx, hy, Vec2.Zero, 0f)))
                .WithDensity(1f)
                .WithUserData("target-break-center"));
            _rightPiece = _body.CreateFixture(new FixtureDef(new PolygonShape(BuildBoxVertices(hx, hy, new Vec2(4f / 3f, 0f), 0f)))
                .WithDensity(1f)
                .WithUserData("target-break-right"));
        }

        private void BreakTargetPlank()
        {
            if (_body == null || !_breakableMode)
            {
                return;
            }

            if (_leftPiece != null)
            {
                _body.DestroyFixture(_leftPiece);
                _leftPiece = null;
            }

            if (_rightPiece != null)
            {
                _body.DestroyFixture(_rightPiece);
                _rightPiece = null;
            }

            _breakableMode = false;
        }

        private static bool IsBreakableTargetTag(object? userData)
        {
            return Equals(userData, "target-break-left") ||
                   Equals(userData, "target-break-center") ||
                   Equals(userData, "target-break-right");
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

        private float RandomRange(float min, float max)
        {
            return (float)(min + _rng.NextDouble() * (max - min));
        }
    }
}
