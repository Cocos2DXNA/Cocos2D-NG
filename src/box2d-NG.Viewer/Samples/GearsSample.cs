namespace Box2DNG.Viewer.Samples
{
    public sealed class GearsSample : BaseSample
    {
        public override string Name => "Gears";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-40f, 0f), new Vec2(40f, 0f))));

            // First gear assembly (static anchor + dynamic bar + dynamic gear).
            {
                CircleShape circle1 = new CircleShape(1f);
                CircleShape circle2 = new CircleShape(2f);
                PolygonShape box = new PolygonShape(BuildBoxVertices(0.5f, 5f, Vec2.Zero, 0f));

                Body body1 = world.CreateBody(new BodyDef().AsStatic().At(10f, 9f));
                body1.CreateFixture(new FixtureDef(circle1));

                Body body2 = world.CreateBody(new BodyDef().AsDynamic().At(10f, 8f));
                body2.CreateFixture(new FixtureDef(box).WithDensity(5f));

                Body body3 = world.CreateBody(new BodyDef().AsDynamic().At(10f, 6f));
                body3.CreateFixture(new FixtureDef(circle2).WithDensity(5f));

                RevoluteJoint joint1 = world.CreateJoint(new RevoluteJointDef(body2, body1, new Vec2(10f, 9f)));
                RevoluteJoint joint2 = world.CreateJoint(new RevoluteJointDef(body2, body3, new Vec2(10f, 6f)));

                GearJointDef gear = new GearJointDef(joint1, joint2, circle2.Radius / circle1.Radius);
                world.CreateJoint(gear);
            }

            // Second gear assembly (two revolute joints + prismatic joint).
            {
                CircleShape circle1 = new CircleShape(1f);
                CircleShape circle2 = new CircleShape(2f);
                PolygonShape box = new PolygonShape(BuildBoxVertices(0.5f, 5f, Vec2.Zero, 0f));

                Body body1 = world.CreateBody(new BodyDef().AsDynamic().At(-3f, 12f));
                body1.CreateFixture(new FixtureDef(circle1).WithDensity(5f));

                RevoluteJoint joint1 = world.CreateJoint(new RevoluteJointDef(ground, body1, new Vec2(-3f, 12f)));

                Body body2 = world.CreateBody(new BodyDef().AsDynamic().At(0f, 12f));
                body2.CreateFixture(new FixtureDef(circle2).WithDensity(5f));

                RevoluteJoint joint2 = world.CreateJoint(new RevoluteJointDef(ground, body2, new Vec2(0f, 12f)));

                Body body3 = world.CreateBody(new BodyDef().AsDynamic().At(2.5f, 12f));
                body3.CreateFixture(new FixtureDef(box).WithDensity(5f));

                PrismaticJointDef pjd = new PrismaticJointDef(ground, body3, new Vec2(2.5f, 12f), new Vec2(0f, 1f))
                    .WithLimit(-5f, 5f);
                PrismaticJoint joint3 = world.CreateJoint(pjd);

                GearJointDef gear1 = new GearJointDef(joint1, joint2, circle2.Radius / circle1.Radius);
                world.CreateJoint(gear1);

                GearJointDef gear2 = new GearJointDef(joint2, joint3, -1f / circle2.Radius);
                world.CreateJoint(gear2);
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
