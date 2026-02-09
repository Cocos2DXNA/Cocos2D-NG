using System;

namespace Box2DNG
{
    public enum ManifoldType
    {
        Circles = 0,
        FaceA = 1,
        FaceB = 2
    }

    public readonly struct ContactFeature : IEquatable<ContactFeature>
    {
        public readonly byte TypeA;
        public readonly byte TypeB;
        public readonly byte IndexA;
        public readonly byte IndexB;

        public ContactFeature(byte typeA, byte typeB, byte indexA, byte indexB)
        {
            TypeA = typeA;
            TypeB = typeB;
            IndexA = indexA;
            IndexB = indexB;
        }

        public bool Equals(ContactFeature other)
        {
            return TypeA == other.TypeA && TypeB == other.TypeB && IndexA == other.IndexA && IndexB == other.IndexB;
        }

        public override bool Equals(object? obj) => obj is ContactFeature other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + TypeA.GetHashCode();
                hash = (hash * 31) + TypeB.GetHashCode();
                hash = (hash * 31) + IndexA.GetHashCode();
                hash = (hash * 31) + IndexB.GetHashCode();
                return hash;
            }
        }
    }

    public readonly struct ManifoldPoint
    {
        public readonly Vec2 LocalPoint;
        public readonly float NormalImpulse;
        public readonly float TangentImpulse;
        public readonly ContactFeature Id;

        public ManifoldPoint(Vec2 localPoint, float normalImpulse, float tangentImpulse, ContactFeature id)
        {
            LocalPoint = localPoint;
            NormalImpulse = normalImpulse;
            TangentImpulse = tangentImpulse;
            Id = id;
        }
    }

    public sealed class Manifold
    {
        public ManifoldType Type { get; set; }
        public Vec2 LocalNormal { get; set; }
        public Vec2 LocalPoint { get; set; }
        public int PointCount { get; set; }
        public ManifoldPoint[] Points { get; }

        public Manifold(int maxPoints = 2)
        {
            Points = new ManifoldPoint[maxPoints];
            PointCount = 0;
        }
    }

    public sealed class WorldManifold
    {
        public Vec2 Normal { get; private set; } = Vec2.Zero;
        public Vec2[] Points { get; }

        public WorldManifold(int maxPoints = 2)
        {
            Points = new Vec2[maxPoints];
        }

        public void Initialize(Manifold manifold, Transform xfA, float radiusA, Transform xfB, float radiusB)
        {
            if (manifold.PointCount == 0)
            {
                Normal = Vec2.Zero;
                return;
            }

            switch (manifold.Type)
            {
                case ManifoldType.Circles:
                    {
                        Normal = new Vec2(1f, 0f);
                        Vec2 pointA = Transform.Mul(xfA, manifold.LocalPoint);
                        Vec2 pointB = Transform.Mul(xfB, manifold.Points[0].LocalPoint);
                        Vec2 diff = pointB - pointA;
                        if (diff.LengthSquared > 0f)
                        {
                            Normal = diff.Normalize();
                        }

                        Vec2 cA = pointA + radiusA * Normal;
                        Vec2 cB = pointB - radiusB * Normal;
                        Points[0] = 0.5f * (cA + cB);
                    }
                    break;
                case ManifoldType.FaceA:
                    {
                        Normal = Rot.Mul(xfA.Q, manifold.LocalNormal);
                        Vec2 planePoint = Transform.Mul(xfA, manifold.LocalPoint);
                        for (int i = 0; i < manifold.PointCount; ++i)
                        {
                            Vec2 clipPoint = Transform.Mul(xfB, manifold.Points[i].LocalPoint);
                            float separation = Vec2.Dot(clipPoint - planePoint, Normal);
                            Vec2 cA = clipPoint + (radiusA - separation) * Normal;
                            Vec2 cB = clipPoint - radiusB * Normal;
                            Points[i] = 0.5f * (cA + cB);
                        }
                    }
                    break;
                case ManifoldType.FaceB:
                    {
                        Normal = Rot.Mul(xfB.Q, manifold.LocalNormal);
                        Vec2 planePoint = Transform.Mul(xfB, manifold.LocalPoint);
                        for (int i = 0; i < manifold.PointCount; ++i)
                        {
                            Vec2 clipPoint = Transform.Mul(xfA, manifold.Points[i].LocalPoint);
                            float separation = Vec2.Dot(clipPoint - planePoint, Normal);
                            Vec2 cB = clipPoint + (radiusB - separation) * Normal;
                            Vec2 cA = clipPoint - radiusA * Normal;
                            Points[i] = 0.5f * (cA + cB);
                        }
                        Normal = -Normal;
                    }
                    break;
            }
        }
    }
}
