using System;

namespace Box2DNG
{
    public readonly struct CosSin : IEquatable<CosSin>
    {
        public readonly float Cosine;
        public readonly float Sine;

        public CosSin(float cosine, float sine)
        {
            Cosine = cosine;
            Sine = sine;
        }

        public bool Equals(CosSin other) => Cosine == other.Cosine && Sine == other.Sine;

        public override bool Equals(object? obj) => obj is CosSin other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + Cosine.GetHashCode();
                hash = (hash * 31) + Sine.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"(cos:{Cosine}, sin:{Sine})";
    }
}
