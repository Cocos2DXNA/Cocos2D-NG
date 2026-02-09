using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class CharacterCollisionSample : BaseSample
    {
        private Body? _character;

        public override string Name => "CharacterCollision";

        public override void Build(World world)
        {
            {
                Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
                ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-20f, 0f), new Vec2(20f, 0f))));
            }

            {
                Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
                ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-8f, 1f), new Vec2(-6f, 1f))));
                ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-6f, 1f), new Vec2(-4f, 1f))));
                ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-4f, 1f), new Vec2(-2f, 1f))));
            }

            {
                Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f).WithAngle(0.25f * MathFng.Pi));
                Vec2[] vs =
                {
                    new Vec2(5f, 7f),
                    new Vec2(6f, 8f),
                    new Vec2(7f, 8f),
                    new Vec2(8f, 7f)
                };
                AddChain(world, ground, vs, loop: false);
            }

            {
                Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
                AddBoxTile(ground, new Vec2(4f, 3f));
                AddBoxTile(ground, new Vec2(6f, 3f));
                AddBoxTile(ground, new Vec2(8f, 3f));
            }

            {
                Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
                Vec2[] vs =
                {
                    new Vec2(-1f, 3f),
                    new Vec2(1f, 3f),
                    new Vec2(1f, 5f),
                    new Vec2(-1f, 5f)
                };
                AddChain(world, ground, vs, loop: true);
            }

            {
                Body ground = world.CreateBody(new BodyDef().AsStatic().At(-10f, 4f));
                Vec2[] vs =
                {
                    new Vec2(0f, 0f),
                    new Vec2(6f, 0f),
                    new Vec2(6f, 2f),
                    new Vec2(4f, 1f),
                    new Vec2(2f, 2f),
                    new Vec2(0f, 2f),
                    new Vec2(-2f, 2f),
                    new Vec2(-4f, 3f),
                    new Vec2(-6f, 2f),
                    new Vec2(-6f, 0f)
                };
                AddChain(world, ground, vs, loop: true);
            }

            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-3f, 8f).LockRotation(true).AllowSleeping(false));
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.5f, 0.5f, Vec2.Zero, 0f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(20f));
            }

            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 5f).LockRotation(true).AllowSleeping(false));
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.25f, 0.25f, Vec2.Zero, 0f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(20f));
            }

            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 8f).LockRotation(true).AllowSleeping(false));
                Vec2[] vertices = new Vec2[6];
                float angle = 0f;
                float delta = MathFng.Pi / 3f;
                for (int i = 0; i < 6; ++i)
                {
                    vertices[i] = new Vec2(0.5f * MathF.Cos(angle), 0.5f * MathF.Sin(angle));
                    angle += delta;
                }
                PolygonShape shape = new PolygonShape(vertices);
                body.CreateFixture(new FixtureDef(shape).WithDensity(20f));
            }

            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(3f, 5f).LockRotation(true).AllowSleeping(false));
                body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(20f));
            }

            {
                _character = world.CreateBody(new BodyDef().AsDynamic().At(-7f, 6f).AllowSleeping(false));
                FixtureDef fd = new FixtureDef(new CircleShape(0.25f)).WithDensity(20f).WithFriction(1f);
                _character.CreateFixture(fd);
            }
        }

        public override void Step(World world, float dt)
        {
            if (_character == null)
            {
                return;
            }

            Vec2 v = _character.LinearVelocity;
            v = new Vec2(-5f, v.Y);
            _character.LinearVelocity = v;
        }

        private static void AddBoxTile(Body ground, Vec2 center)
        {
            PolygonShape shape = new PolygonShape(BuildBoxVertices(1f, 1f, center, 0f));
            ground.CreateFixture(new FixtureDef(shape).WithDensity(0f));
        }

        private static void AddChain(World world, Body body, Vec2[] vertices, bool loop)
        {
            if (vertices.Length < 2)
            {
                return;
            }

            int count = vertices.Length;
            int segmentCount = loop ? count : count - 1;
            for (int i = 0; i < segmentCount; ++i)
            {
                int i1 = i;
                int i2 = (i + 1) % count;
                Vec2 p1 = vertices[i1];
                Vec2 p2 = vertices[i2];

                Vec2 ghost1 = vertices[(i1 - 1 + count) % count];
                Vec2 ghost2 = vertices[(i2 + 1) % count];

                ChainSegmentShape seg = new ChainSegmentShape(p1, p2, ghost1, ghost2);
                body.CreateFixture(new FixtureDef(seg).WithDensity(0f));
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
