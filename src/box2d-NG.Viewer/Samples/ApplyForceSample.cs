using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class ApplyForceSample : BaseSample
    {
        private Body? _body;
        private bool _applyToCenter;

        public override string Name => "ApplyForce";

        public override WorldDef CreateWorldDef() => new WorldDef().WithGravity(new Vec2(0f, 0f));

        public override void Build(World world)
        {
            float restitution = 0.4f;

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 20f));
            FixtureDef wallDef = new FixtureDef(new SegmentShape(new Vec2(-20f, -20f), new Vec2(-20f, 20f)))
                .WithDensity(0f)
                .WithRestitution(restitution);
            ground.CreateFixture(wallDef);
            wallDef = new FixtureDef(new SegmentShape(new Vec2(20f, -20f), new Vec2(20f, 20f)))
                .WithDensity(0f)
                .WithRestitution(restitution);
            ground.CreateFixture(wallDef);
            wallDef = new FixtureDef(new SegmentShape(new Vec2(-20f, 20f), new Vec2(20f, 20f)))
                .WithDensity(0f)
                .WithRestitution(restitution);
            ground.CreateFixture(wallDef);
            wallDef = new FixtureDef(new SegmentShape(new Vec2(-20f, -20f), new Vec2(20f, -20f)))
                .WithDensity(0f)
                .WithRestitution(restitution);
            ground.CreateFixture(wallDef);

            Vec2[] tri1 = BuildTriangle(0.3524f * MathFng.Pi, 1f, 0f, 0f, 0.5f);
            Vec2[] tri2 = BuildTriangle(-0.3524f * MathFng.Pi, -1f, 0f, 0f, 0.5f);

            FixtureDef fd1 = new FixtureDef(new PolygonShape(tri1)).WithDensity(4f);
            FixtureDef fd2 = new FixtureDef(new PolygonShape(tri2)).WithDensity(2f);

            _body = world.CreateBody(new BodyDef()
                .AsDynamic()
                .At(0f, 2f)
                .WithAngle(MathFng.Pi)
                .WithAngularDamping(5f)
                .WithLinearDamping(0.1f)
                .AllowSleeping(false));
            _body.CreateFixture(fd1);
            _body.CreateFixture(fd2);

            PolygonShape box = new PolygonShape(new[]
            {
                new Vec2(-0.5f, -0.5f),
                new Vec2(0.5f, -0.5f),
                new Vec2(0.5f, 0.5f),
                new Vec2(-0.5f, 0.5f)
            });

            FixtureDef boxDef = new FixtureDef(box).WithDensity(1f).WithFriction(0.3f);

            for (int i = 0; i < 10; ++i)
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 5f + 1.54f * i));
                body.CreateFixture(boxDef);

                float gravity = 10f;
                float I = body.Inertia;
                float mass = body.Mass;
                float radius = MathF.Sqrt(2f * I / mass);

                FrictionJointDef jd = new FrictionJointDef(ground, body, body.Transform.P)
                    .WithMaxForce(mass * gravity)
                    .WithMaxTorque(mass * radius * gravity);
                world.CreateJoint(jd);
            }
        }

        public override void OnKey(char key)
        {
            if (_body == null)
            {
                return;
            }

            switch (key)
            {
                case 'c':
                    _applyToCenter = !_applyToCenter;
                    break;
                case 'w':
                    {
                        Vec2 f = _body.GetWorldVector(new Vec2(0f, -200f));
                        Vec2 p = _body.GetWorldPoint(new Vec2(0f, 2f));
                        if (_applyToCenter)
                        {
                            _body.ApplyForce(f);
                        }
                        else
                        {
                            _body.ApplyForce(f, p);
                        }
                    }
                    break;
                case 'x':
                    {
                        Vec2 f = _body.GetWorldVector(new Vec2(0f, 200f));
                        Vec2 p = _body.GetWorldPoint(new Vec2(0f, 2f));
                        if (_applyToCenter)
                        {
                            _body.ApplyForce(f);
                        }
                        else
                        {
                            _body.ApplyForce(f, p);
                        }
                    }
                    break;
                case 'a':
                    _body.ApplyTorque(50f);
                    break;
                case 'd':
                    _body.ApplyTorque(-50f);
                    break;
            }
        }

        private static Vec2[] BuildTriangle(float angle, float xAxis, float yAxis, float topX, float topY)
        {
            Rot rot = new Rot(angle);
            Vec2 v0 = Rot.Mul(rot, new Vec2(-1f, 0f)) + new Vec2(xAxis, yAxis);
            Vec2 v1 = Rot.Mul(rot, new Vec2(1f, 0f)) + new Vec2(xAxis, yAxis);
            Vec2 v2 = Rot.Mul(rot, new Vec2(topX, topY)) + new Vec2(xAxis, yAxis);
            return new[] { v0, v1, v2 };
        }
    }
}
