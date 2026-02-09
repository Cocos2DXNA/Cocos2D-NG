using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2D.Collision.Shapes;
using Box2D.Common;
using Box2D.Dynamics;
using Box2D.Dynamics.Joints;

namespace Box2D.TestBed.Tests
{
    public class RopeJoint : Test
    {
        private const int TraceEvery = 10;
        private readonly List<b2RevoluteJoint> m_chainJoints = new List<b2RevoluteJoint>();
        private b2Body m_endBody;
        private int m_traceStep;

        public RopeJoint()
        {
            b2Body anchorBody = m_groundBody;

            {
                b2PolygonShape shape = new b2PolygonShape();
                shape.SetAsBox(0.5f, 0.125f);

                b2FixtureDef fd = new b2FixtureDef();
                fd.shape = shape;
                fd.density = 20.0f;
                fd.friction = 0.2f;
                fd.filter.categoryBits = 0x0001;
                fd.filter.maskBits = 0xFFFF & ~0x0002;

                b2RevoluteJointDef jd = new b2RevoluteJointDef();
                jd.CollideConnected = false;

                const int N = 10;
                const float y = 0.0f;
                m_ropeDef.localAnchorA.Set(0.0f, y);

                b2Body prevBody = anchorBody;
                for (int i = 0; i < N; ++i)
                {
                    b2BodyDef bd  = new b2BodyDef();
                    bd.type = b2BodyType.b2_dynamicBody;
                    bd.position.Set(0.5f + 1.0f * i, y);
                    if (i == N - 1)
                    {
                        shape.SetAsBox(1.5f, 1.5f);
                        fd.density = 100.0f;
                        fd.filter.categoryBits = 0x0002;
                        bd.position.Set(1.0f * i, y);
                        bd.angularDamping = 0.4f;
                    }

                    b2Body body = m_world.CreateBody(bd);

                    body.CreateFixture(fd);

                    b2Vec2 anchor = new b2Vec2(i, y);
                    jd.Initialize(prevBody, body, anchor);
                    b2RevoluteJoint joint = (b2RevoluteJoint)m_world.CreateJoint(jd);
                    m_chainJoints.Add(joint);

                    prevBody = body;
                    if (i == N - 1)
                    {
                        m_endBody = body;
                    }
                }

                m_ropeDef.localAnchorB.SetZero();

                float extraLength = 0.01f;
                m_ropeDef.maxLength = N - 1.0f + extraLength;
                m_ropeDef.BodyB = prevBody;
            }

            {
                m_ropeDef.BodyA = anchorBody;
                m_rope = m_world.CreateJoint(m_ropeDef);
            }
        }

        private string[] telemetry = new string[2];

        public override void Step(Settings settings)
        {
            base.Step(settings);

            if (settings.hz <= 0.0f)
            {
                return;
            }

            if ((m_traceStep++ % TraceEvery) != 0)
            {
                return;
            }

            float maxAnchorError = 0.0f;
            for (int i = 0; i < m_chainJoints.Count; ++i)
            {
                b2Vec2 anchorA = m_chainJoints[i].GetAnchorA();
                b2Vec2 anchorB = m_chainJoints[i].GetAnchorB();
                float error = (anchorB - anchorA).Length;
                if (error > maxAnchorError)
                {
                    maxAnchorError = error;
                }
            }

            float ropeLength = 0.0f;
            float ropeError = 0.0f;
            if (m_rope != null)
            {
                b2Vec2 ropeA = m_rope.GetAnchorA();
                b2Vec2 ropeB = m_rope.GetAnchorB();
                ropeLength = (ropeB - ropeA).Length;
                ropeError = ropeLength - m_ropeDef.maxLength;
            }

            b2Vec2 endPos = m_endBody != null ? m_endBody.Position : b2Vec2.Zero;
            float endSpeed = m_endBody != null ? m_endBody.LinearVelocity.Length : 0.0f;
            float endAngSpeed = m_endBody != null ? Math.Abs(m_endBody.AngularVelocity) : 0.0f;

            telemetry[0] = $"ropejoint trace step={m_traceStep} ropeLen={ropeLength:0.###} maxLen={m_ropeDef.maxLength:0.###} ropeErr={ropeError:0.###} ";
            telemetry[1] = $"maxAnchorErr={maxAnchorError:0.###} endPos=({endPos.x:0.###},{endPos.y:0.###}) endSpeed={endSpeed:0.###} endAngSpeed={endAngSpeed:0.###}";
        }

        public override void Keyboard(char key)
        {
            switch (key)
            {
                case 'j':
                    if (m_rope != null)
                    {
                        m_world.DestroyJoint(m_rope);
                        m_rope = null;
                    }
                    else
                    {
                        m_rope = m_world.CreateJoint(m_ropeDef);
                    }
                    break;
            }
        }

        protected override void Draw(Settings settings)
        {
            base.Draw(settings);
            m_debugDraw.DrawString(5, m_textLine, "Press (j) to toggle the rope joint.");
            m_textLine += 15;
            if (m_rope != null)
            {
                m_debugDraw.DrawString(5, m_textLine, "Rope ON");
            }
            else
            {
                m_debugDraw.DrawString(5, m_textLine, "Rope OFF");
            }
            m_textLine += 15;
            m_debugDraw.DrawString(5, m_textLine, telemetry[0]);
            m_textLine += 15;
            m_debugDraw.DrawString(5, m_textLine, telemetry[1]);
        }

        public b2RopeJointDef m_ropeDef = new b2RopeJointDef();
        public b2Joint m_rope;
    }
}
