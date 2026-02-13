using System;

namespace Box2DNG
{
    public enum ShapeType
    {
        Circle = 0,
        Capsule = 1,
        Segment = 2,
        Polygon = 3,
        ChainSegment = 4,
        ShapeTypeCount = 5
    }

    public readonly struct Filter : IEquatable<Filter>
    {
        public readonly ulong CategoryBits;
        public readonly ulong MaskBits;
        public readonly int GroupIndex;

        public static readonly Filter Default = new Filter(1, ulong.MaxValue, 0);

        public Filter(ulong categoryBits, ulong maskBits, int groupIndex)
        {
            CategoryBits = categoryBits;
            MaskBits = maskBits;
            GroupIndex = groupIndex;
        }

        public bool Equals(Filter other)
        {
            return CategoryBits == other.CategoryBits && MaskBits == other.MaskBits && GroupIndex == other.GroupIndex;
        }

        public override bool Equals(object? obj) => obj is Filter other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + CategoryBits.GetHashCode();
                hash = (hash * 31) + MaskBits.GetHashCode();
                hash = (hash * 31) + GroupIndex.GetHashCode();
                return hash;
            }
        }
    }

    public readonly struct QueryFilter : IEquatable<QueryFilter>
    {
        public readonly ulong CategoryBits;
        public readonly ulong MaskBits;

        public static readonly QueryFilter Default = new QueryFilter(1, ulong.MaxValue);

        public QueryFilter(ulong categoryBits, ulong maskBits)
        {
            CategoryBits = categoryBits;
            MaskBits = maskBits;
        }

        public bool Equals(QueryFilter other) => CategoryBits == other.CategoryBits && MaskBits == other.MaskBits;

        public override bool Equals(object? obj) => obj is QueryFilter other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + CategoryBits.GetHashCode();
                hash = (hash * 31) + MaskBits.GetHashCode();
                return hash;
            }
        }
    }

    public readonly struct SurfaceMaterial : IEquatable<SurfaceMaterial>
    {
        public readonly float Friction;
        public readonly float Restitution;
        public readonly float RollingResistance;
        public readonly float TangentSpeed;
        public readonly ulong UserMaterialId;
        public readonly uint CustomColor;

        public static readonly SurfaceMaterial Default = new SurfaceMaterial(0.6f, 0f, 0f, 0f, 0, 0);

        public SurfaceMaterial(float friction, float restitution, float rollingResistance, float tangentSpeed, ulong userMaterialId, uint customColor)
        {
            Friction = friction;
            Restitution = restitution;
            RollingResistance = rollingResistance;
            TangentSpeed = tangentSpeed;
            UserMaterialId = userMaterialId;
            CustomColor = customColor;
        }

        public bool Equals(SurfaceMaterial other)
        {
            return Friction == other.Friction && Restitution == other.Restitution &&
                   RollingResistance == other.RollingResistance && TangentSpeed == other.TangentSpeed &&
                   UserMaterialId == other.UserMaterialId && CustomColor == other.CustomColor;
        }

        public override bool Equals(object? obj) => obj is SurfaceMaterial other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + Friction.GetHashCode();
                hash = (hash * 31) + Restitution.GetHashCode();
                hash = (hash * 31) + RollingResistance.GetHashCode();
                hash = (hash * 31) + TangentSpeed.GetHashCode();
                hash = (hash * 31) + UserMaterialId.GetHashCode();
                hash = (hash * 31) + CustomColor.GetHashCode();
                return hash;
            }
        }
    }
}
