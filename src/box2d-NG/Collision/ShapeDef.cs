using System;

namespace Box2DNG
{
    public sealed class ShapeDef
    {
        public object? UserData { get; private set; }
        public SurfaceMaterial Material { get; private set; } = SurfaceMaterial.Default;
        public float Density { get; private set; } = 0f;
        public Filter Filter { get; private set; } = Filter.Default;
        public bool EnableCustomFiltering { get; private set; }
        public bool IsSensor { get; private set; }
        public bool EnableSensorEvents { get; private set; }
        public bool EnableContactEvents { get; private set; }

        public ShapeDef WithUserData(object? data) { UserData = data; return this; }
        public ShapeDef WithMaterial(SurfaceMaterial material) { Material = material; return this; }
        public ShapeDef WithDensity(float density) { Density = density; return this; }
        public ShapeDef WithFilter(Filter filter) { Filter = filter; return this; }
        public ShapeDef EnableCustomFilter(bool enable) { EnableCustomFiltering = enable; return this; }
        public ShapeDef AsSensor(bool isSensor) { IsSensor = isSensor; return this; }
        public ShapeDef EnableSensorEvent(bool enable) { EnableSensorEvents = enable; return this; }
        public ShapeDef EnableContactEvent(bool enable) { EnableContactEvents = enable; return this; }
    }
}
