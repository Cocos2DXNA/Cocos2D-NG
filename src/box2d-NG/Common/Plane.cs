using System;

namespace Box2DNG
{
    public readonly struct Plane : IEquatable<Plane>
    {
        public readonly Vec2 Normal;
        public readonly float Offset;

        public Plane(Vec2 normal, float offset)
        {
            Normal = normal;
            Offset = offset;
        }

        public bool Equals(Plane other) => Normal.Equals(other.Normal) && Offset == other.Offset;

        public override bool Equals(object? obj) => obj is Plane other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + Normal.GetHashCode();
                hash = (hash * 31) + Offset.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"normal:{Normal} offset:{Offset}";
    }
}
