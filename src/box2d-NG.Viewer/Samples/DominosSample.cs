namespace Box2DNG.Viewer.Samples
{
    public sealed class DominosSample : BaseSample
    {
        public override string Name => "Dominos";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            {
                PolygonShape platform = new PolygonShape(BuildBoxVertices(6f, 0.25f, new Vec2(-1.5f, 10f), 0f));
                Body body = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
                body.CreateFixture(new FixtureDef(platform));
            }

            PolygonShape dominoShape = new PolygonShape(BuildBoxVertices(0.1f, 1f, Vec2.Zero, 0f));
            FixtureDef dominoDef = new FixtureDef(dominoShape).WithDensity(20f).WithFriction(0.1f);

            for (int i = 0; i < 10; ++i)
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-6f + 1f * i, 11.25f));
                body.CreateFixture(dominoDef);
            }

            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(7f, 0.25f, Vec2.Zero, 0.3f));
                Body body = world.CreateBody(new BodyDef().AsStatic().At(1f, 6f));
                body.CreateFixture(new FixtureDef(shape));
            }

            Body b2;
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.25f, 1.5f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsStatic().At(-7f, 4f));
                body.CreateFixture(new FixtureDef(shape));
                b2 = body;
            }

            Body b3;
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(6f, 0.125f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-0.9f, 1f).WithAngle(-0.15f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(10f));
                b3 = body;
            }

            world.CreateJoint(new RevoluteJointDef(ground, b3, new Vec2(-2f, 1f)).WithCollideConnected(true));

            Body b4;
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.25f, 0.25f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(-10f, 15f));
                body.CreateFixture(new FixtureDef(shape).WithDensity(10f));
                b4 = body;
            }

            world.CreateJoint(new RevoluteJointDef(b2, b4, new Vec2(-7f, 15f)));

            const float bucketShiftX = -1.0f;
            Body b5;
            {
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(6.5f + bucketShiftX, 3f).IsAwake(false));
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1f, 0.1f, new Vec2(0f, -0.9f), 0f));
                FixtureDef fd = new FixtureDef(shape).WithDensity(10f).WithFriction(0.1f);
                body.CreateFixture(fd);

                shape = new PolygonShape(BuildBoxVertices(0.1f, 1f, new Vec2(-0.9f, 0f), 0f));
                fd = new FixtureDef(shape).WithDensity(10f).WithFriction(0.1f);
                body.CreateFixture(fd);

                shape = new PolygonShape(BuildBoxVertices(0.1f, 1f, new Vec2(0.9f, 0f), 0f));
                fd = new FixtureDef(shape).WithDensity(10f).WithFriction(0.1f);
                body.CreateFixture(fd);
                b5 = body;
            }

            world.CreateJoint(new RevoluteJointDef(ground, b5, new Vec2(6f + bucketShiftX, 2f)));

            Body b6;
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(1f, 0.1f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(6.5f + bucketShiftX, 4.1f).IsAwake(false));
                body.CreateFixture(new FixtureDef(shape).WithDensity(30f));
                b6 = body;
            }

            world.CreateJoint(new RevoluteJointDef(b5, b6, new Vec2(7.5f + bucketShiftX, 4f)));

            Body b7;
            {
                PolygonShape shape = new PolygonShape(BuildBoxVertices(0.1f, 1f, Vec2.Zero, 0f));
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(7.4f + bucketShiftX, 1f).IsAwake(false));
                body.CreateFixture(new FixtureDef(shape).WithDensity(10f));
                b7 = body;
            }

            Vec2 anchorA = b3.GetWorldPoint(new Vec2(6f, 0f));
            Vec2 anchorB = b7.GetWorldPoint(new Vec2(0f, -1f));
            world.CreateJoint(new DistanceJointDef(b3, b7, anchorA, anchorB));

            {
                CircleShape shape = new CircleShape(0.2f);
                FixtureDef fd = new FixtureDef(shape).WithDensity(10f);
                for (int i = 0; i < 4; ++i)
                {
                    Body body = world.CreateBody(new BodyDef().AsDynamic().At(5.9f + bucketShiftX + 0.4f * i, 2.4f).IsAwake(false));
                    body.CreateFixture(fd);
                }
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
