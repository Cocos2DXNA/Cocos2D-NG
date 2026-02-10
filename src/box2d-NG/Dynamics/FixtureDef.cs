using System;

namespace Box2DNG
{
    public sealed class FixtureDef
    {
        public Shape Shape { get; private set; }
        public float Friction { get; private set; } = 0.2f;
        public float Restitution { get; private set; }
        public float Density { get; private set; } = 1f;
        public bool IsSensor { get; private set; }
        public bool EnableSensorEvents { get; private set; } = true;
        public Filter Filter { get; private set; } = Filter.Default;
        public object? UserData { get; private set; }

        public FixtureDef(Shape shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public FixtureDef WithShape(Shape shape) { Shape = shape; return this; }
        public FixtureDef WithFriction(float friction) { Friction = friction; return this; }
        public FixtureDef WithRestitution(float restitution) { Restitution = restitution; return this; }
        public FixtureDef WithDensity(float density) { Density = density; return this; }
        public FixtureDef AsSensor(bool isSensor = true) { IsSensor = isSensor; return this; }
        public FixtureDef WithSensorEvents(bool enable = true) { EnableSensorEvents = enable; return this; }
        public FixtureDef WithFilter(Filter filter) { Filter = filter; return this; }
        public FixtureDef WithUserData(object? userData) { UserData = userData; return this; }
    }
}
