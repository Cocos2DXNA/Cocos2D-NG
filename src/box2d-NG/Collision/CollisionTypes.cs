using System;

namespace Box2DNG
{
    public readonly struct RayCastInput
    {
        public readonly Vec2 Origin;
        public readonly Vec2 Translation;
        public readonly float MaxFraction;

        public RayCastInput(Vec2 origin, Vec2 translation, float maxFraction)
        {
            Origin = origin;
            Translation = translation;
            MaxFraction = maxFraction;
        }
    }

    public readonly struct RayCastOutput
    {
        public readonly Vec2 Point;
        public readonly Vec2 Normal;
        public readonly float Fraction;

        public RayCastOutput(Vec2 point, Vec2 normal, float fraction)
        {
            Point = point;
            Normal = normal;
            Fraction = fraction;
        }
    }

    public readonly struct MassData
    {
        public readonly float Mass;
        public readonly Vec2 Center;
        public readonly float Inertia;

        public MassData(float mass, Vec2 center, float inertia)
        {
            Mass = mass;
            Center = center;
            Inertia = inertia;
        }
    }
}
