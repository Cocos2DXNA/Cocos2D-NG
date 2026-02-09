namespace Box2DNG.Viewer.Samples
{
    public sealed class DistanceJointSample : BaseSample
    {
        public override string Name => "DistanceJoint";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body boxA = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 10f));
            Body boxB = world.CreateBody(new BodyDef().AsDynamic().At(5f, 10f));
            PolygonShape box = new PolygonShape(BuildBoxVertices(1f, 1f, Vec2.Zero, 0f));
            boxA.CreateFixture(new FixtureDef(box).WithDensity(2f));
            boxB.CreateFixture(new FixtureDef(box).WithDensity(2f));

            DistanceJointDef jd = new DistanceJointDef(boxA, boxB, boxA.Transform.P, boxB.Transform.P)
                .WithLength(10f)
                .WithFrequency(2f)
                .WithDampingRatio(0.7f);
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
