using System;
using Box2D.Common;
using Box2D.Collision.Shapes;
using Box2D.Dynamics;

namespace Box2D
{
    public static class Box2DScenarios
    {
        public static b2World CreateStackOfBoxes(out b2Body topBox, out b2Body pusher)
        {
            b2World world = new b2World(new b2Vec2(0.0f, -10.0f));

            b2BodyDef groundDef = new b2BodyDef();
            groundDef.position.Set(0.0f, -10.0f);
            b2Body ground = world.CreateBody(groundDef);
            b2PolygonShape groundShape = new b2PolygonShape();
            groundShape.SetAsBox(50.0f, 10.0f);
            ground.CreateFixture(groundShape, 0.0f);

            b2PolygonShape boxShape = new b2PolygonShape();
            boxShape.SetAsBox(0.5f, 0.5f);
            b2FixtureDef boxFixture = new b2FixtureDef();
            boxFixture.shape = boxShape;
            boxFixture.density = 1.0f;
            boxFixture.friction = 0.4f;

            topBox = null;
            for (int i = 0; i < 10; ++i)
            {
                b2BodyDef bodyDef = new b2BodyDef();
                bodyDef.type = b2BodyType.b2_dynamicBody;
                bodyDef.position.Set(0.0f, 0.5f + i * 1.05f);
                b2Body body = world.CreateBody(bodyDef);
                body.CreateFixture(boxFixture);
                topBox = body;
            }

            b2BodyDef pusherDef = new b2BodyDef();
            pusherDef.type = b2BodyType.b2_kinematicBody;
            pusherDef.position.Set(-8.0f, 1.0f);
            pusher = world.CreateBody(pusherDef);
            b2PolygonShape pusherShape = new b2PolygonShape();
            pusherShape.SetAsBox(1.0f, 1.0f);
            b2FixtureDef pusherFixture = new b2FixtureDef();
            pusherFixture.shape = pusherShape;
            pusherFixture.density = 2.0f;
            pusherFixture.friction = 0.4f;
            pusher.CreateFixture(pusherFixture);
            pusher.LinearVelocity = new b2Vec2(10.0f, 0.0f);

            return world;
        }

        public static b2World CreateRollerCoaster(out b2Body cart)
        {
            b2World world = new b2World(new b2Vec2(0.0f, -10.0f));

            b2BodyDef groundDef = new b2BodyDef();
            b2Body ground = world.CreateBody(groundDef);

            b2ChainShape chain = new b2ChainShape();
            b2Vec2[] vertices = new b2Vec2[41];
            float startX = -20.0f;
            float step = 1.0f;
            for (int i = 0; i < vertices.Length; ++i)
            {
                float x = startX + i * step;
                float y = -1.5f + (float)Math.Sin(i * 0.35f) * 2.0f;
                vertices[i] = new b2Vec2(x, y);
            }
            chain.CreateChain(vertices, vertices.Length);

            b2FixtureDef chainFixture = new b2FixtureDef();
            chainFixture.shape = chain;
            chainFixture.friction = 0.8f;
            ground.CreateFixture(chainFixture);

            b2BodyDef cartDef = new b2BodyDef();
            cartDef.type = b2BodyType.b2_dynamicBody;
            cartDef.position.Set(-18.0f, 3.0f);
            cart = world.CreateBody(cartDef);
            b2CircleShape wheel = new b2CircleShape();
            wheel.Radius = 0.6f;
            b2FixtureDef cartFixture = new b2FixtureDef();
            cartFixture.shape = wheel;
            cartFixture.density = 1.0f;
            cartFixture.friction = 0.7f;
            cart.CreateFixture(cartFixture);
            cart.LinearVelocity = new b2Vec2(6.0f, 0.0f);

            return world;
        }

        public static b2World CreateProjectile(out b2Body projectile)
        {
            b2World world = new b2World(new b2Vec2(0.0f, -10.0f));

            b2BodyDef groundDef = new b2BodyDef();
            groundDef.position.Set(0.0f, -10.0f);
            b2Body ground = world.CreateBody(groundDef);
            b2PolygonShape groundShape = new b2PolygonShape();
            groundShape.SetAsBox(50.0f, 10.0f);
            ground.CreateFixture(groundShape, 0.0f);

            b2BodyDef projDef = new b2BodyDef();
            projDef.type = b2BodyType.b2_dynamicBody;
            projDef.position.Set(-15.0f, 2.0f);
            projectile = world.CreateBody(projDef);
            b2CircleShape projShape = new b2CircleShape();
            projShape.Radius = 0.4f;
            b2FixtureDef projFixture = new b2FixtureDef();
            projFixture.shape = projShape;
            projFixture.density = 1.0f;
            projFixture.restitution = 0.1f;
            projectile.CreateFixture(projFixture);
            projectile.LinearVelocity = new b2Vec2(12.0f, 8.0f);

            return world;
        }
    }
}
