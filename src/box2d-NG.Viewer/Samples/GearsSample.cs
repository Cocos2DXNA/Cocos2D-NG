namespace Box2DNG.Viewer.Samples
{
    public sealed class GearsSample : BaseSample
    {
        public override string Name => "Gears";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-5f, 5f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 5f));
            Body bodyC = world.CreateBody(new BodyDef().AsDynamic().At(5f, 5f));

            CircleShape circleSmall = new CircleShape(1f);
            CircleShape circleLarge = new CircleShape(2f);

            bodyA.CreateFixture(new FixtureDef(circleSmall).WithDensity(5f));
            bodyB.CreateFixture(new FixtureDef(circleLarge).WithDensity(5f));
            bodyC.CreateFixture(new FixtureDef(circleSmall).WithDensity(5f));

            RevoluteJoint revA = world.CreateJoint(new RevoluteJointDef(ground, bodyA, bodyA.Transform.P));
            RevoluteJoint revB = world.CreateJoint(new RevoluteJointDef(ground, bodyB, bodyB.Transform.P));
            RevoluteJoint revC = world.CreateJoint(new RevoluteJointDef(ground, bodyC, bodyC.Transform.P));

            GearJointDef gd1 = new GearJointDef(revA, revB, -circleLarge.Radius / circleSmall.Radius);
            world.CreateJoint(gd1);

            GearJointDef gd2 = new GearJointDef(revB, revC, -circleSmall.Radius / circleLarge.Radius);
            world.CreateJoint(gd2);
        }
    }
}
