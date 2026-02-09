namespace Box2DNG.Viewer.Samples
{
    public sealed class MotorJointSample : BaseSample
    {
        public override string Name => "MotorJoint";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 8f));
            PolygonShape box = new PolygonShape(BuildBoxVertices(1f, 1f, Vec2.Zero, 0f));
            body.CreateFixture(new FixtureDef(box).WithDensity(2f));

            MotorJointDef jd = new MotorJointDef(ground, body)
                .WithLinearOffset(new Vec2(0f, 8f))
                .WithAngularOffset(0f)
                .WithMaxForce(1000f)
                .WithMaxTorque(1000f);
            world.CreateJoint(jd);
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
