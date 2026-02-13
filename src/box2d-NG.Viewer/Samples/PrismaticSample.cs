using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class PrismaticSample : BaseSample
    {
        public override string Name => "Prismatic";

        private PrismaticJoint? _joint;
        private float _motorSpeed = 2f;
        private float _motorForce = 25f;
        private float _hertz = 1f;
        private float _dampingRatio = 0.5f;
        private float _translation = 0f;
        private bool _enableSpring;
        private bool _enableMotor;
        private bool _enableLimit = true;

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body platform = world.CreateBody(new BodyDef().AsDynamic().At(0f, 10f));
            PolygonShape box = new PolygonShape(BuildBoxVertices(0.5f, 2f, Vec2.Zero, 0f));
            platform.CreateFixture(new FixtureDef(box));

            Vec2 pivot = new Vec2(0f, 9f);
            Vec2 axis = new Vec2(1f, 1f).Normalize();
            PrismaticJointDef jd = new PrismaticJointDef(ground, platform, pivot, axis)
                .WithLimit(-10f, 10f)
                .WithMotor(_motorSpeed, _motorForce);
            if (_enableSpring)
            {
                jd = jd.WithSpring(_hertz, _dampingRatio, _translation);
            }
            _joint = world.CreateJoint(jd);
            _joint.SetMotorEnabled(_enableMotor);
            _joint.SetLimitEnabled(_enableLimit);
        }

        public override void OnKey(char key)
        {
            if (_joint == null)
            {
                return;
            }

            switch (key)
            {
                case 'l':
                    _enableLimit = !_enableLimit;
                    _joint.SetLimitEnabled(_enableLimit);
                    WakeJointBodies();
                    break;
                case 'm':
                    _enableMotor = !_enableMotor;
                    _joint.SetMotorEnabled(_enableMotor);
                    WakeJointBodies();
                    break;
                case 's':
                    _enableSpring = !_enableSpring;
                    _joint.SetSpringEnabled(_enableSpring);
                    WakeJointBodies();
                    break;
                case 'a':
                    _motorSpeed = MathF.Max(-40f, _motorSpeed - 1f);
                    _joint.SetMotorSpeed(_motorSpeed);
                    WakeJointBodies();
                    break;
                case 'd':
                    _motorSpeed = MathF.Min(40f, _motorSpeed + 1f);
                    _joint.SetMotorSpeed(_motorSpeed);
                    WakeJointBodies();
                    break;
                case 'q':
                    _translation = MathF.Max(-15f, _translation - 0.5f);
                    _joint.SetTargetTranslation(_translation);
                    WakeJointBodies();
                    break;
                case 'e':
                    _translation = MathF.Min(15f, _translation + 0.5f);
                    _joint.SetTargetTranslation(_translation);
                    WakeJointBodies();
                    break;
            }
        }

        private void WakeJointBodies()
        {
            if (_joint == null)
            {
                return;
            }

            _joint.BodyA.SetAwake(true);
            _joint.BodyB.SetAwake(true);
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
