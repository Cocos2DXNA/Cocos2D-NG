using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class CircleStressSample : BaseSample
    {
        private readonly Random _rng = new Random(1234);
        private RevoluteJoint? _joint;

        public override string Name => "CircleStress";

        public override WorldDef CreateWorldDef() => new WorldDef().WithGravity(new Vec2(0f, -50f));

        public override void Build(World world)
        {
            {
                PolygonShape groundShape = new PolygonShape(BuildBoxVertices(50f, 10f, Vec2.Zero, 0f));
                Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, -10f));
                ground.CreateFixture(new FixtureDef(groundShape).WithFriction(1f));

                PolygonShape wallShape = new PolygonShape(BuildBoxVertices(3f, 50f, Vec2.Zero, 0f));
                Body rightWall = world.CreateBody(new BodyDef().AsStatic().At(45f, 25f));
                rightWall.CreateFixture(new FixtureDef(wallShape));
                Body leftWall = world.CreateBody(new BodyDef().AsStatic().At(-45f, 25f));
                leftWall.CreateFixture(new FixtureDef(wallShape));

                PolygonShape cornerShape = new PolygonShape(BuildBoxVertices(20f, 3f, Vec2.Zero, 0f));
                Body corner = world.CreateBody(new BodyDef().AsStatic().At(-35f, 8f).WithAngle(-MathF.PI / 4f));
                corner.CreateFixture(new FixtureDef(cornerShape));
                corner = world.CreateBody(new BodyDef().AsStatic().At(35f, 8f).WithAngle(MathF.PI / 4f));
                corner.CreateFixture(new FixtureDef(cornerShape));

                Body top = world.CreateBody(new BodyDef().AsStatic().At(0f, 75f));
                top.CreateFixture(new FixtureDef(groundShape).WithFriction(1f));
            }

            {
                int numPieces = 5;
                float radius = 6f;
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 10f));

                for (int i = 0; i < numPieces; i++)
                {
                    CircleShape cd = new CircleShape(new Vec2(0f, 0f), 1.2f);
                    float angle = 2f * MathF.PI * (i / (float)numPieces);
                    float xPos = radius * MathF.Cos(angle);
                    float yPos = radius * MathF.Sin(angle);
                    cd.WithCenter(new Vec2(xPos, yPos));

                    FixtureDef fd = new FixtureDef(cd)
                        .WithDensity(25f)
                        .WithFriction(0.1f)
                        .WithRestitution(0.9f);

                    body.CreateFixture(fd);
                }

                RevoluteJointDef rjd = new RevoluteJointDef(body, world.Bodies[0], body.Transform.P)
                    .WithMotor(true, MathF.PI, 1000000f);
                _joint = world.CreateJoint(rjd);
            }

            {
                int loadSize = 41;
                for (int j = 0; j < 15; j++)
                {
                    for (int i = 0; i < loadSize; i++)
                    {
                        float radius = 1f + (i % 2 == 0 ? 1f : -1f) * 0.5f * RandomRange(0.5f, 1f);
                        CircleShape circ = new CircleShape(radius);
                        FixtureDef fd2 = new FixtureDef(circ)
                            .WithDensity(radius * 1.5f)
                            .WithFriction(0.5f)
                            .WithRestitution(0.7f);

                        float xPos = -39f + 2 * i;
                        float yPos = 50f + j;
                        Body body = world.CreateBody(new BodyDef().AsDynamic().At(xPos, yPos));
                        body.CreateFixture(fd2);
                    }
                }
            }
        }

        public override void OnKey(char key)
        {
            if (_joint == null)
            {
                return;
            }

            switch (key)
            {
                case 's':
                    _joint.SetMotorSpeed(0f);
                    break;
                case '1':
                    _joint.SetMotorSpeed(MathF.PI);
                    break;
                case '2':
                    _joint.SetMotorSpeed(MathF.PI * 2f);
                    break;
                case '3':
                    _joint.SetMotorSpeed(MathF.PI * 3f);
                    break;
                case '4':
                    _joint.SetMotorSpeed(MathF.PI * 6f);
                    break;
                case '5':
                    _joint.SetMotorSpeed(MathF.PI * 10f);
                    break;
            }
        }

        private float RandomRange(float min, float max)
        {
            return (float)(min + _rng.NextDouble() * (max - min));
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
