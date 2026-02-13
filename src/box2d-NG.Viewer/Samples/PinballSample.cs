using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class PinballSample : BaseSample
    {
        private RevoluteJoint? _leftJoint;
        private RevoluteJoint? _rightJoint;
        private bool _button;

        public override string Name => "Pinball";

        public override WorldDef CreateWorldDef()
        {
            return new WorldDef()
                .WithGravity(new Vec2(0f, -10f))
                .EnableContinuousCollision(true)
                .WithMaxSubSteps(16)
                .WithVelocityIterations(20)
                .WithPositionIterations(8);
        }

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            Vec2[] loop =
            {
                new Vec2(0f, -2f),
                new Vec2(8f, 6f),
                new Vec2(8f, 20f),
                new Vec2(-8f, 20f),
                new Vec2(-8f, 6f)
            };

            for (int i = 0; i < loop.Length; ++i)
            {
                int j = (i + 1) % loop.Length;
                Vec2 p1 = loop[i];
                Vec2 p2 = loop[j];
                Vec2 ghost1 = loop[i == 0 ? loop.Length - 1 : i - 1];
                Vec2 ghost2 = loop[(j + 1) % loop.Length];
                ChainSegmentShape seg = new ChainSegmentShape(p1, p2, ghost1, ghost2);
                ground.CreateFixture(new FixtureDef(seg));
            }

            Vec2 pLeft = new Vec2(-2f, 0f);
            Vec2 pRight = new Vec2(2f, 0f);

            Body leftFlipper = world.CreateBody(new BodyDef().AsDynamic().At(pLeft));
            Body rightFlipper = world.CreateBody(new BodyDef().AsDynamic().At(pRight));

            PolygonShape flipperShape = new PolygonShape(BuildBoxVertices(1.75f, 0.1f, Vec2.Zero, 0f));
            FixtureDef fd = new FixtureDef(flipperShape).WithDensity(1f);
            leftFlipper.CreateFixture(fd);
            rightFlipper.CreateFixture(fd);

            _leftJoint = world.CreateJoint(new RevoluteJointDef(ground, leftFlipper, pLeft)
                .WithMotor(true, 0f, 1000f)
                .WithLimit(-30f * MathFng.Pi / 180f, 5f * MathFng.Pi / 180f));

            _rightJoint = world.CreateJoint(new RevoluteJointDef(ground, rightFlipper, pRight)
                .WithMotor(true, 0f, 1000f)
                .WithLimit(-5f * MathFng.Pi / 180f, 30f * MathFng.Pi / 180f));

            Body ball = world.CreateBody(new BodyDef().AsDynamic().At(1f, 15f).IsBullet(true));
            ball.CreateFixture(new FixtureDef(new CircleShape(0.2f)).WithDensity(1f));
        }

        public override void Step(World world, float dt)
        {
            if (_leftJoint == null || _rightJoint == null)
            {
                return;
            }

            if (_button)
            {
                _leftJoint.SetMotorSpeed(20f);
                _rightJoint.SetMotorSpeed(-20f);
            }
            else
            {
                _leftJoint.SetMotorSpeed(-10f);
                _rightJoint.SetMotorSpeed(10f);
            }
        }

        public override void OnKey(char key)
        {
            if (key == 'a' || key == 'A')
            {
                _button = !_button;
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
