using System;

namespace Box2DNG
{
    public sealed class Contact
    {
        public Fixture? FixtureA { get; }
        public Fixture? FixtureB { get; }
        public Shape ShapeA { get; }
        public Shape ShapeB { get; }
        public Transform TransformA { get; private set; }
        public Transform TransformB { get; private set; }
        public Manifold Manifold { get; } = new Manifold();
        public Manifold OldManifold { get; } = new Manifold();
        public SolverPoint[] SolverPoints { get; private set; } = Array.Empty<SolverPoint>();
        public bool IsTouching { get; private set; }
        public bool WasTouching { get; private set; }
        public float Toi { get; private set; } = 1f;
        public int ToiCount { get; private set; }
        public bool HasToi { get; private set; }
        internal SolverSetType SolverSetType { get; set; } = SolverSetType.Awake;

        public Contact(Shape shapeA, Transform transformA, Shape shapeB, Transform transformB)
        {
            ShapeA = shapeA ?? throw new ArgumentNullException(nameof(shapeA));
            ShapeB = shapeB ?? throw new ArgumentNullException(nameof(shapeB));
            TransformA = transformA;
            TransformB = transformB;
        }

        public Contact(Fixture fixtureA, Fixture fixtureB)
            : this(fixtureA?.Shape ?? throw new ArgumentNullException(nameof(fixtureA)),
                   fixtureA.Body.Transform,
                   fixtureB?.Shape ?? throw new ArgumentNullException(nameof(fixtureB)),
                   fixtureB.Body.Transform)
        {
            FixtureA = fixtureA;
            FixtureB = fixtureB;
        }

        public void Update(Transform transformA, Transform transformB)
        {
            TransformA = transformA;
            TransformB = transformB;
            Evaluate();
        }

        public void Evaluate()
        {
            WasTouching = IsTouching;
            CopyToOldManifold();
            ContactManager.Evaluate(ShapeA, TransformA, ShapeB, TransformB, Manifold);
            WarmStartFromOld();
            EnsureSolverPoints();
            IsTouching = Manifold.PointCount > 0;
            if (!IsTouching)
            {
                Toi = 1f;
                ToiCount = 0;
                HasToi = false;
            }
            else
            {
                HasToi = false;
            }
        }

        public void CacheToi(float toi)
        {
            Toi = toi;
            HasToi = true;
        }

        public void IncrementToiCount() => ToiCount++;

        private void CopyToOldManifold()
        {
            OldManifold.Type = Manifold.Type;
            OldManifold.LocalNormal = Manifold.LocalNormal;
            OldManifold.LocalPoint = Manifold.LocalPoint;
            OldManifold.PointCount = Manifold.PointCount;
            for (int i = 0; i < Manifold.PointCount; ++i)
            {
                OldManifold.Points[i] = Manifold.Points[i];
            }
        }

        private void WarmStartFromOld()
        {
            for (int i = 0; i < Manifold.PointCount; ++i)
            {
                ManifoldPoint mp2 = Manifold.Points[i];
                float normalImpulse = 0f;
                float tangentImpulse = 0f;

                for (int j = 0; j < OldManifold.PointCount; ++j)
                {
                    ManifoldPoint mp1 = OldManifold.Points[j];
                    if (mp1.Id.Equals(mp2.Id))
                    {
                        normalImpulse = mp1.NormalImpulse;
                        tangentImpulse = mp1.TangentImpulse;
                        break;
                    }
                }

                Manifold.Points[i] = new ManifoldPoint(mp2.LocalPoint, normalImpulse, tangentImpulse, mp2.Id);
            }
        }

        private void EnsureSolverPoints()
        {
            if (SolverPoints.Length < Manifold.PointCount)
            {
                SolverPoints = new SolverPoint[Manifold.PointCount];
            }
        }

        internal SolverPoint[] EnsureSolverPoints(int count)
        {
            if (SolverPoints.Length < count)
            {
                SolverPoints = new SolverPoint[count];
            }
            return SolverPoints;
        }
    }

    public struct SolverPoint
    {
        public Vec2 RA;
        public Vec2 RB;
        public float NormalMass;
        public float TangentMass;
        public float VelocityBias;
        public float Softness;
        public float Bias;
    }
}
