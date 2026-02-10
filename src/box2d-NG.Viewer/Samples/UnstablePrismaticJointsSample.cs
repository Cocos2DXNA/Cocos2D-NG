namespace Box2DNG.Viewer.Samples
{
    public sealed class UnstablePrismaticJointsSample : BaseSample
    {
        public override string Name => "Unstable Prismatic Joints";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-100f, 0f), new Vec2(100f, 0f))));

            Body center = world.CreateBody(new BodyDef().AsDynamic().At(0f, 3f));
            center.CreateFixture(new FixtureDef(new CircleShape(0.5f)));

            float hertz = 10f;
            float damping = 2f;
            Vec2 axis = new Vec2(1f, 0f);
            Vec2 anchor = new Vec2(0f, 3f);

            Body left = world.CreateBody(new BodyDef().AsDynamic().At(-3.5f, 3f));
            left.CreateFixture(new FixtureDef(new CircleShape(2f)));
            PrismaticJointDef leftJoint = new PrismaticJointDef(center, left, anchor, axis)
                .WithSpring(hertz, damping, -3f);
            world.CreateJoint(leftJoint);

            Body right = world.CreateBody(new BodyDef().AsDynamic().At(3.5f, 3f));
            right.CreateFixture(new FixtureDef(new CircleShape(2f)));
            PrismaticJointDef rightJoint = new PrismaticJointDef(center, right, anchor, axis)
                .WithSpring(hertz, damping, 3f);
            world.CreateJoint(rightJoint);
        }
    }
}
