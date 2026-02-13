using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Box2DNG.Tests
{
    [TestClass]
    public class JointTests
    {
        [TestMethod]
        public void DistanceJoint_MaintainsLength()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(2f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            DistanceJointDef def = new DistanceJointDef(bodyA, bodyB, bodyA.Transform.P, bodyB.Transform.P).WithLength(2f);
            world.CreateJoint(def);

            for (int i = 0; i < 60; ++i)
            {
                world.Step(1f / 60f);
            }

            float dist = (bodyB.Transform.P - bodyA.Transform.P).Length;
            Assert.IsTrue(MathF.Abs(dist - 2f) < 0.1f, $"Distance joint length drifted: {dist}");
        }

        [TestMethod]
        public void RevoluteJoint_MotorDrivesRotation()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            RevoluteJointDef def = new RevoluteJointDef(bodyA, bodyB, bodyA.Transform.P)
                .WithMotor(true, 4f, 10f);
            world.CreateJoint(def);

            for (int i = 0; i < 60; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(MathF.Abs(bodyB.AngularVelocity) > 0.1f, "Expected motor to drive angular velocity.");
        }

        [TestMethod]
        public void PrismaticJoint_ConstrainsPerpMotion()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            PrismaticJointDef def = new PrismaticJointDef(bodyA, bodyB, bodyA.Transform.P, new Vec2(1f, 0f))
                .WithMotor(2f, 20f);
            world.CreateJoint(def);

            bodyB.ApplyForce(new Vec2(0f, 10f));

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(MathF.Abs(bodyB.Transform.P.Y) < 0.2f, $"Expected prismatic joint to block Y translation, got {bodyB.Transform.P.Y}.");
            Assert.IsTrue(bodyB.Transform.P.X > 0.2f, "Expected prismatic motor to move along X.");
        }

        [TestMethod]
        public void PrismaticJoint_SpringTracksTargetTranslation()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            float target = 3f;
            PrismaticJointDef def = new PrismaticJointDef(bodyA, bodyB, bodyA.Transform.P, new Vec2(1f, 0f))
                .WithSpring(5f, 0.7f, target);
            PrismaticJoint joint = world.CreateJoint(def);

            for (int i = 0; i < 240; ++i)
            {
                world.Step(1f / 60f);
            }

            float translation = joint.GetTranslation();
            Assert.IsTrue(MathF.Abs(translation - target) < 0.6f, $"Expected translation near target. target={target} actual={translation}");
        }

        [TestMethod]
        public void PrismaticJoint_ReportsSpeedAndMotorForce()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            PrismaticJointDef def = new PrismaticJointDef(bodyA, bodyB, bodyA.Transform.P, new Vec2(1f, 0f))
                .WithMotor(5f, 50f);
            PrismaticJoint joint = world.CreateJoint(def);

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            float speed = joint.GetSpeed();
            Assert.IsTrue(speed > 0.01f, $"Expected positive prismatic speed, got {speed}");
        }

        [TestMethod]
        public void PrismaticJoint_UnstableSampleRemainsFinite()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            ground.CreateFixture(new FixtureDef(new SegmentShape(new Vec2(-100f, 0f), new Vec2(100f, 0f))));

            Body center = world.CreateBody(new BodyDef().AsDynamic().At(0f, 3f));
            center.CreateFixture(new FixtureDef(new CircleShape(0.5f)));

            Body left = world.CreateBody(new BodyDef().AsDynamic().At(-3.5f, 3f));
            left.CreateFixture(new FixtureDef(new CircleShape(2f)));
            world.CreateJoint(new PrismaticJointDef(center, left, center.Transform.P, new Vec2(1f, 0f))
                .WithSpring(10f, 2f, -3f));

            Body right = world.CreateBody(new BodyDef().AsDynamic().At(3.5f, 3f));
            right.CreateFixture(new FixtureDef(new CircleShape(2f)));
            world.CreateJoint(new PrismaticJointDef(center, right, center.Transform.P, new Vec2(1f, 0f))
                .WithSpring(10f, 2f, 3f));

            for (int i = 0; i < 600; ++i)
            {
                world.Step(1f / 60f);
            }

            foreach (Body body in world.Bodies)
            {
                Assert.IsTrue(MathFng.IsValidVec2(body.Transform.P), $"Invalid position for body {body.Transform.P}");
                Assert.IsTrue(MathFng.IsValidFloat(body.AngularVelocity), "Invalid angular velocity.");
                Assert.IsTrue(MathFng.IsValidVec2(body.LinearVelocity), "Invalid linear velocity.");
            }
        }

        [TestMethod]
        public void PrismaticJoint_MultipleChainRemainsFinite()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));

            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            Body previous = ground;
            Vec2 axis = new Vec2(0f, 1f);
            for (int i = 0; i < 6; ++i)
            {
                float y = 0.6f + 1.2f * i;
                Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, y));
                PolygonShape box = new PolygonShape(BuildBoxVertices(0.5f, 0.5f, Vec2.Zero, 0f));
                body.CreateFixture(new FixtureDef(box));

                Vec2 anchor = new Vec2(0f, y - 0.6f);
                PrismaticJointDef jd = new PrismaticJointDef(previous, body, anchor, axis)
                    .WithLimit(-6f, 6f);
                world.CreateJoint(jd);

                previous = body;
            }

            for (int i = 0; i < 600; ++i)
            {
                world.Step(1f / 60f);
            }

            foreach (Body body in world.Bodies)
            {
                Assert.IsTrue(MathFng.IsValidVec2(body.Transform.P), $"Invalid position for body {body.Transform.P}");
                Assert.IsTrue(MathFng.IsValidFloat(body.AngularVelocity), "Invalid angular velocity.");
                Assert.IsTrue(MathFng.IsValidVec2(body.LinearVelocity), "Invalid linear velocity.");
            }
        }

        [TestMethod]
        public void WheelJoint_SpringReducesAxisDisplacement()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, -2f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            WheelJointDef def = new WheelJointDef(bodyA, bodyB, bodyA.Transform.P, new Vec2(0f, 1f))
                .WithSpring(4f, 0.7f);
            world.CreateJoint(def);

            float initialSpeed = 5f;
            bodyB.LinearVelocity = new Vec2(0f, initialSpeed);
            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(MathF.Abs(bodyB.LinearVelocity.Y) < initialSpeed, "Expected spring to reduce axis velocity.");
        }

        [TestMethod]
        public void WheelJoint_MotorDrivesRotation()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            WheelJointDef def = new WheelJointDef(bodyA, bodyB, bodyB.Transform.P, new Vec2(1f, 0f))
                .WithMotor(true, 5f, 10f);
            world.CreateJoint(def);

            for (int i = 0; i < 60; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(MathF.Abs(bodyB.AngularVelocity) > 0.1f, "Expected motor to drive angular velocity.");
        }

        [TestMethod]
        public void PulleyJoint_MaintainsCombinedLength()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-1f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            Vec2 groundA = new Vec2(-1f, 2f);
            Vec2 groundB = new Vec2(1f, 2f);
            PulleyJointDef def = new PulleyJointDef(bodyA, bodyB, groundA, groundB, bodyA.Transform.P, bodyB.Transform.P, 1f);
            world.CreateJoint(def);

            bodyA.ApplyForce(new Vec2(0f, 10f));
            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            float lenA = (bodyA.Transform.P - groundA).Length;
            float lenB = (bodyB.Transform.P - groundB).Length;
            float sum = lenA + lenB;
            float target = def.LengthA + def.LengthB;
            Assert.IsTrue(MathF.Abs(sum - target) < 0.2f, $"Pulley length drifted: {sum} vs {target}");
        }

        [TestMethod]
        public void WeldJoint_LocksRelativeTransform()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            WeldJointDef def = new WeldJointDef(bodyA, bodyB, bodyA.Transform.P);
            world.CreateJoint(def);

            bodyB.ApplyForce(new Vec2(10f, 5f));
            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            Vec2 delta = bodyB.Transform.P - bodyA.Transform.P;
            float angleDelta = bodyB.Transform.Q.Angle - bodyA.Transform.Q.Angle - def.ReferenceAngle;
            Assert.IsTrue(MathF.Abs(delta.Length - 1f) < 0.2f, "Expected welded bodies to keep distance.");
            Assert.IsTrue(MathF.Abs(angleDelta) < 0.2f, "Expected welded bodies to keep relative angle.");
        }

        [TestMethod]
        public void MotorJoint_DrivesTowardOffsets()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(3f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            MotorJointDef def = new MotorJointDef(bodyA, bodyB)
                .WithLinearOffset(new Vec2(1f, 0f))
                .WithAngularOffset(0.2f)
                .WithMaxForce(50f)
                .WithMaxTorque(50f);
            world.CreateJoint(def);

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            float distance = (bodyB.Transform.P - (bodyA.Transform.P + new Vec2(1f, 0f))).Length;
            Assert.IsTrue(distance < 0.6f, "Expected motor joint to move body toward linear offset.");
        }

        [TestMethod]
        public void GearJoint_LinksRevoluteAngles()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(-1f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(1f, 0f));

            ground.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            RevoluteJointDef revA = new RevoluteJointDef(ground, bodyA, bodyA.Transform.P);
            RevoluteJointDef revB = new RevoluteJointDef(ground, bodyB, bodyB.Transform.P);
            RevoluteJoint jointA = world.CreateJoint(revA);
            RevoluteJoint jointB = world.CreateJoint(revB);

            GearJointDef gearDef = new GearJointDef(jointA, jointB, 1f);
            world.CreateJoint(gearDef);

            bodyA.AngularVelocity = 2f;

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            float angleA = bodyA.Transform.Q.Angle - ground.Transform.Q.Angle - jointA.ReferenceAngle;
            float angleB = bodyB.Transform.Q.Angle - ground.Transform.Q.Angle - jointB.ReferenceAngle;
            float sum = angleA + angleB;
            Assert.IsTrue(MathF.Abs(sum) < 0.2f, $"Expected gear constraint, got {sum}.");
            Assert.IsTrue(MathF.Abs(bodyB.AngularVelocity) > 0.1f, "Expected gear joint to transfer motion.");
        }

        [TestMethod]
        public void RopeJoint_EnforcesMaxLength()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body bodyA = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            Body bodyB = world.CreateBody(new BodyDef().AsDynamic().At(2f, 0f));

            bodyA.CreateFixture(new FixtureDef(new CircleShape(0.2f)));
            bodyB.CreateFixture(new FixtureDef(new CircleShape(0.2f)));

            RopeJointDef def = new RopeJointDef(bodyA, bodyB, bodyA.Transform.P, bodyB.Transform.P)
                .WithMaxLength(1f);
            world.CreateJoint(def);

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            float dist = (bodyB.Transform.P - bodyA.Transform.P).Length;
            Assert.IsTrue(dist <= 1.05f, $"Expected rope max length, got {dist}");
        }

        [TestMethod]
        public void RopeJoint_ChainRemainsConnected()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, -10f)));
            Body anchor = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));

            PolygonShape linkShape = new PolygonShape(BuildBoxVertices(0.5f, 0.125f, Vec2.Zero, 0f));
            FixtureDef linkFixture = new FixtureDef(linkShape)
                .WithDensity(20f)
                .WithFriction(0.2f)
                .WithFilter(new Filter(1, ulong.MaxValue, -1));

            const int count = 10;
            const float y = 0f;
            Body prevBody = anchor;
            Body lastBody = anchor;
            List<RevoluteJoint> joints = new List<RevoluteJoint>();

            for (int i = 0; i < count; ++i)
            {
                BodyDef bodyDef = new BodyDef().AsDynamic().At(0.5f + i, y);
                Body body;
                if (i == count - 1)
                {
                    PolygonShape endShape = new PolygonShape(BuildBoxVertices(1.5f, 1.5f, Vec2.Zero, 0f));
                    FixtureDef endFixture = new FixtureDef(endShape)
                        .WithDensity(100f)
                        .WithFilter(new Filter(2, ulong.MaxValue, -1));
                    bodyDef = bodyDef.At(1.0f * i, y).WithAngularDamping(0.4f);
                    body = world.CreateBody(bodyDef);
                    body.CreateFixture(endFixture);
                }
                else
                {
                    body = world.CreateBody(bodyDef);
                    body.CreateFixture(linkFixture);
                }

                Vec2 jointAnchor = new Vec2(i, y);
                RevoluteJoint joint = world.CreateJoint(new RevoluteJointDef(prevBody, body, jointAnchor));
                joints.Add(joint);

                prevBody = body;
                lastBody = body;
            }

            RopeJoint rope = world.CreateJoint(new RopeJointDef(anchor, lastBody, new Vec2(0f, y), lastBody.Transform.P)
                .WithMaxLength(count - 1.0f + 0.01f));

            float maxAnchorError = 0f;
            float ropeLength = 0f;
            for (int i = 0; i < 240; ++i)
            {
                world.Step(1f / 60f);
            }

            for (int j = 0; j < joints.Count; ++j)
            {
                RevoluteJoint joint = joints[j];
                Vec2 anchorA = joint.BodyA.GetWorldPoint(joint.LocalAnchorA);
                Vec2 anchorB = joint.BodyB.GetWorldPoint(joint.LocalAnchorB);
                float error = (anchorB - anchorA).Length;
                if (error > maxAnchorError)
                {
                    maxAnchorError = error;
                }
            }

            Vec2 ropeAnchorA = rope.BodyA.GetWorldPoint(rope.LocalAnchorA);
            Vec2 ropeAnchorB = rope.BodyB.GetWorldPoint(rope.LocalAnchorB);
            ropeLength = (ropeAnchorB - ropeAnchorA).Length;

            Assert.IsTrue(maxAnchorError < 0.02f, $"Rope chain anchors drifted: {maxAnchorError}");
            Assert.IsTrue(ropeLength <= rope.MaxLength + 0.02f, $"Rope joint exceeded max length: {ropeLength}");
        }

        [TestMethod]
        public void FrictionJoint_DampsMotion()
        {
            World world = new World(new WorldDef().WithGravity(new Vec2(0f, 0f)));
            Body ground = world.CreateBody(new BodyDef().AsStatic().At(0f, 0f));
            Body body = world.CreateBody(new BodyDef().AsDynamic().At(0f, 0f));
            body.CreateFixture(new FixtureDef(new CircleShape(0.5f)).WithDensity(1f));

            body.LinearVelocity = new Vec2(10f, 0f);
            body.AngularVelocity = 5f;

            FrictionJointDef def = new FrictionJointDef(ground, body, body.Transform.P)
                .WithMaxForce(5f)
                .WithMaxTorque(5f);
            world.CreateJoint(def);

            for (int i = 0; i < 120; ++i)
            {
                world.Step(1f / 60f);
            }

            Assert.IsTrue(body.LinearVelocity.Length < 10f, "Expected friction joint to reduce linear velocity.");
            Assert.IsTrue(MathF.Abs(body.AngularVelocity) < 5f, "Expected friction joint to reduce angular velocity.");
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
