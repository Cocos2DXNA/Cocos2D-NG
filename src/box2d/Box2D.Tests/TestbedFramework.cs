using Box2D.Collision;
using Box2D.Common;
using Box2D.Dynamics;
using Box2D.Dynamics.Contacts;
using Box2D.Dynamics.Joints;

namespace Box2D.TestBed
{
    public class Settings
    {
        public float hz = 60.0f;
        public int velocityIterations = 8;
        public int positionIterations = 3;
        public bool pause = false;
        public bool singleStep = false;
        public b2Vec2 viewCenter = new b2Vec2(0.0f, 20.0f);
    }

    public struct ContactPoint
    {
        public b2Fixture fixtureA;
        public b2Fixture fixtureB;
        public b2Vec2 position;
        public b2Vec2 normal;
        public b2PointState state;
    }

    public class Test : b2ContactListener, b2DestructionListener
    {
        public const int k_maxContactPoints = 2048;

        protected b2World m_world;
        protected b2Body m_groundBody;
        protected DebugDraw m_debugDraw;
        protected int m_textLine;

        protected ContactPoint[] m_points = new ContactPoint[k_maxContactPoints];
        protected int m_pointCount;

        protected b2Body m_bomb;
        public b2World World => m_world;
        protected int m_stepCount;

        protected Test()
        {
            m_world = new b2World(new b2Vec2(0.0f, -10.0f));
            m_groundBody = m_world.CreateBody(new b2BodyDef());

            m_debugDraw = new DebugDraw();
            m_debugDraw.AppendFlags(b2DrawFlags.e_shapeBit);

            m_world.SetDebugDraw(m_debugDraw);
            m_world.SetContactListener(this);
            m_world.SetDestructionListener(this);
        }

        public DebugDraw DebugDraw => m_debugDraw;

        public virtual void Step(Settings settings)
        {
            float timeStep = settings.hz > 0.0f ? 1.0f / settings.hz : 0.0f;
            if (settings.pause)
            {
                if (settings.singleStep)
                {
                    settings.singleStep = false;
                }
                else
                {
                    timeStep = 0.0f;
                }
            }

            m_pointCount = 0;
            m_world.Step(timeStep, settings.velocityIterations, settings.positionIterations);

            Draw(settings);
        }

        protected virtual void Draw(Settings settings)
        {
            m_textLine = 15;
        }

        public virtual void Keyboard(char key)
        {
        }

        public virtual void KeyboardUp(char key)
        {
        }

        public virtual void MouseDown(b2Vec2 p)
        {
        }

        public virtual void MouseUp(b2Vec2 p)
        {
        }

        public virtual void MouseMove(b2Vec2 p)
        {
        }

        public virtual void BeginContact(b2Contact contact)
        {
        }

        public virtual void EndContact(b2Contact contact)
        {
        }

        public virtual void PreSolve(b2Contact contact, b2Manifold oldManifold)
        {
            b2Manifold manifold = contact.GetManifold();
            if (manifold.pointCount == 0)
            {
                return;
            }

            b2PointState[] state1 = new b2PointState[b2Settings.b2_maxManifoldPoints];
            b2PointState[] state2 = new b2PointState[b2Settings.b2_maxManifoldPoints];
            b2Collision.b2GetPointStates(state1, state2, oldManifold, manifold);

            b2WorldManifold worldManifold = new b2WorldManifold();
            contact.GetWorldManifold(ref worldManifold);

            for (int i = 0; i < manifold.pointCount && m_pointCount < k_maxContactPoints; ++i)
            {
                ContactPoint cp = new ContactPoint();
                cp.fixtureA = contact.FixtureA;
                cp.fixtureB = contact.FixtureB;
                cp.position = worldManifold.points[i];
                cp.normal = worldManifold.normal;
                cp.state = state2[i];
                m_points[m_pointCount++] = cp;
            }
        }

        public virtual void PostSolve(b2Contact contact, ref b2ContactImpulse impulse)
        {
        }

        public virtual void JointDestroyed(b2Joint joint)
        {
        }

        public virtual void FixtureDestroyed(b2Fixture fixture)
        {
        }

        public void SayGoodbye(b2Joint joint)
        {
            JointDestroyed(joint);
        }

        public void SayGoodbye(b2Fixture fixture)
        {
            FixtureDestroyed(fixture);
        }
    }
}
