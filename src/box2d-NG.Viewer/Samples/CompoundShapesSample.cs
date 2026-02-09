using System;

namespace Box2DNG.Viewer.Samples
{
    public sealed class CompoundShapesSample : BaseSample
    {
        public override string Name => "CompoundShapes";

        public override void Build(World world)
        {
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(50f, 0f), new Vec2(-50f, 0f))));

            {
                CircleShape circle1 = new CircleShape(0.5f).WithCenter(new Vec2(-0.5f, 0.5f));
                CircleShape circle2 = new CircleShape(0.5f).WithCenter(new Vec2(0.5f, 0.5f));

                for (int i = 0; i < 10; ++i)
                {
                    float x = RandomRange(-0.1f, 0.1f);
                    Body body = world.CreateBody(new BodyDef()
                        .AsDynamic()
                        .At(x + 5f, 1.05f + 2.5f * i)
                        .WithAngle(RandomRange(-MathFng.Pi, MathFng.Pi)));
                    body.CreateFixture(new FixtureDef(circle1).WithDensity(2f));
                    body.CreateFixture(new FixtureDef(circle2).WithDensity(0f));
                }
            }

            {
                PolygonShape polygon1 = new PolygonShape(BuildBoxVertices(0.25f, 0.5f, Vec2.Zero, 0f));
                PolygonShape polygon2 = new PolygonShape(BuildBoxVertices(0.25f, 0.5f, new Vec2(0f, -0.5f), 0.5f * MathFng.Pi));

                for (int i = 0; i < 10; ++i)
                {
                    float x = RandomRange(-0.1f, 0.1f);
                    Body body = world.CreateBody(new BodyDef()
                        .AsDynamic()
                        .At(x - 5f, 1.05f + 2.5f * i)
                        .WithAngle(RandomRange(-MathFng.Pi, MathFng.Pi)));
                    body.CreateFixture(new FixtureDef(polygon1).WithDensity(2f));
                    body.CreateFixture(new FixtureDef(polygon2).WithDensity(2f));
                }
            }

            {
                Rot rot1 = new Rot(0.3524f * MathFng.Pi);
                Vec2[] vertices = new Vec2[3];
                vertices[0] = Rot.Mul(rot1, new Vec2(-1f, 0f)) + rot1.GetXAxis();
                vertices[1] = Rot.Mul(rot1, new Vec2(1f, 0f)) + rot1.GetXAxis();
                vertices[2] = Rot.Mul(rot1, new Vec2(0f, 0.5f)) + rot1.GetXAxis();
                PolygonShape triangle1 = new PolygonShape(vertices);

                Rot rot2 = new Rot(-0.3524f * MathFng.Pi);
                vertices[0] = Rot.Mul(rot2, new Vec2(-1f, 0f)) - rot2.GetXAxis();
                vertices[1] = Rot.Mul(rot2, new Vec2(1f, 0f)) - rot2.GetXAxis();
                vertices[2] = Rot.Mul(rot2, new Vec2(0f, 0.5f)) - rot2.GetXAxis();
                PolygonShape triangle2 = new PolygonShape(vertices);

                for (int i = 0; i < 10; ++i)
                {
                    float x = RandomRange(-0.1f, 0.1f);
                    Body body = world.CreateBody(new BodyDef()
                        .AsDynamic()
                        .At(x, 2.05f + 2.5f * i));
                    body.CreateFixture(new FixtureDef(triangle1).WithDensity(2f));
                    body.CreateFixture(new FixtureDef(triangle2).WithDensity(2f));
                }
            }

            {
                PolygonShape bottom = new PolygonShape(BuildBoxVertices(1.5f, 0.15f, Vec2.Zero, 0f));
                PolygonShape left = new PolygonShape(BuildBoxVertices(0.15f, 2.7f, new Vec2(-1.45f, 2.35f), 0.2f));
                PolygonShape right = new PolygonShape(BuildBoxVertices(0.15f, 2.7f, new Vec2(1.45f, 2.35f), -0.2f));

                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 2f));
                body.CreateFixture(new FixtureDef(bottom).WithDensity(4f));
                body.CreateFixture(new FixtureDef(left).WithDensity(4f));
                body.CreateFixture(new FixtureDef(right).WithDensity(4f));
            }
        }

        private static float RandomRange(float min, float max)
        {
            return (float)(min + Random.Shared.NextDouble() * (max - min));
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
