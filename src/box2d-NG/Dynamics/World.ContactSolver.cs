namespace Box2DNG
{
    public sealed partial class World
    {
        private sealed class ContactSolver
        {
            private readonly World _world;
            private readonly System.Collections.Generic.List<ContactConstraint> _constraints = new System.Collections.Generic.List<ContactConstraint>();

            public ContactSolver(World world)
            {
                _world = world;
            }

            public void Prepare(float timeStep, float dtRatio, System.Collections.Generic.IReadOnlyList<Contact> contacts)
            {
                _constraints.Clear();
                if (contacts.Count == 0)
                {
                    return;
                }

                for (int i = 0; i < contacts.Count; ++i)
                {
                    Contact contact = contacts[i];
                    if (contact.FixtureA == null || contact.FixtureB == null)
                    {
                        continue;
                    }

                    if (contact.Manifold.PointCount == 0)
                    {
                        continue;
                    }

                    Fixture fixtureA = contact.FixtureA;
                    Fixture fixtureB = contact.FixtureB;
                    Body bodyA = fixtureA.Body;
                    Body bodyB = fixtureB.Body;

                    float radiusA = GetShapeRadius(fixtureA.Shape);
                    float radiusB = GetShapeRadius(fixtureB.Shape);

                    WorldManifold worldManifold = new WorldManifold();
                    worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

                    Vec2 normal = worldManifold.Normal;
                    Vec2 tangent = new Vec2(-normal.Y, normal.X);

                    ContactConstraint constraint = new ContactConstraint(contact, bodyA, bodyB, normal, tangent, contact.Manifold.PointCount)
                    {
                        Friction = _world.MixFriction(fixtureA, fixtureB),
                        Restitution = _world.MixRestitution(fixtureA, fixtureB),
                        InvMassA = bodyA.InverseMass,
                        InvIA = bodyA.InverseInertia,
                        InvMassB = bodyB.InverseMass,
                        InvIB = bodyB.InverseInertia
                    };

                    Vec2 centerA = bodyA.GetWorldCenter();
                    Vec2 centerB = bodyB.GetWorldCenter();

                    for (int p = 0; p < constraint.PointCount; ++p)
                    {
                        ManifoldPoint mp = contact.Manifold.Points[p];
                        if (dtRatio != 1f)
                        {
                            mp = new ManifoldPoint(mp.LocalPoint, mp.NormalImpulse * dtRatio, mp.TangentImpulse * dtRatio, mp.Id);
                            contact.Manifold.Points[p] = mp;
                        }

                        Vec2 point = worldManifold.Points[p];
                        Vec2 rA = point - centerA;
                        Vec2 rB = point - centerB;

                        PositionManifold pm = ComputePositionManifold(contact, bodyA.Transform, bodyB.Transform, radiusA, radiusB, p);
                        float separation = pm.Separation;

                        float rnA = Vec2.Cross(rA, normal);
                        float rnB = Vec2.Cross(rB, normal);
                        float kNormal = bodyA.InverseMass + bodyB.InverseMass +
                                        bodyA.InverseInertia * rnA * rnA + bodyB.InverseInertia * rnB * rnB;
                        float normalMass = kNormal > 0f ? 1f / kNormal : 0f;

                        float rtA = Vec2.Cross(rA, tangent);
                        float rtB = Vec2.Cross(rB, tangent);
                        float kTangent = bodyA.InverseMass + bodyB.InverseMass +
                                         bodyA.InverseInertia * rtA * rtA + bodyB.InverseInertia * rtB * rtB;
                        float tangentMass = kTangent > 0f ? 1f / kTangent : 0f;

                        Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                        Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                        float relativeVelocity = Vec2.Dot(vB - vA, normal);

                        float bias = 0f;
                        float softness = 0f;
                        if (_world._def.EnableContactSoftening)
                        {
                            if (separation > 0f)
                            {
                                bias = separation / timeStep;
                            }
                            else if (_world._def.UseSoftConstraints)
                            {
                                _world.ComputeContactSoftness(kNormal, timeStep, separation, out bias, out softness);
                                if (softness > 0f)
                                {
                                    normalMass = 1f / (kNormal + softness);
                                }
                            }
                            else
                            {
                                float baumgarte = Constants.Baumgarte * (separation + Constants.LinearSlop) / timeStep;
                                float maxBias = MathF.Max(_world._def.ContactSpeed, 0f);
                                float legacyBias = MathFng.Clamp(baumgarte, -maxBias, 0f);
                                bias = MathFng.Clamp(legacyBias, -Constants.MaxLinearCorrection, 0f);
                            }
                        }

                        constraint.Points[p] = new ContactConstraintPoint
                        {
                            RA = rA,
                            RB = rB,
                            NormalImpulse = mp.NormalImpulse,
                            TangentImpulse = mp.TangentImpulse,
                            TotalNormalImpulse = 0f,
                            NormalMass = normalMass,
                            TangentMass = tangentMass,
                            BaseSeparation = separation - Vec2.Dot(rB - rA, normal),
                            RelativeVelocity = relativeVelocity,
                            Bias = bias,
                            Softness = softness
                        };
                    }

                    SolverPoint[] solverPoints = contact.EnsureSolverPoints(constraint.PointCount);
                    for (int p = 0; p < constraint.PointCount; ++p)
                    {
                        ContactConstraintPoint cp = constraint.Points[p];
                        solverPoints[p] = new SolverPoint
                        {
                            RA = cp.RA,
                            RB = cp.RB,
                            NormalMass = cp.NormalMass,
                            TangentMass = cp.TangentMass,
                            VelocityBias = 0f,
                            Softness = cp.Softness,
                            Bias = cp.Bias
                        };
                    }

                    _constraints.Add(constraint);
                }
            }

            public void WarmStart()
            {
                for (int i = 0; i < _constraints.Count; ++i)
                {
                    ContactConstraint constraint = _constraints[i];
                    Body bodyA = constraint.BodyA;
                    Body bodyB = constraint.BodyB;

                    Vec2 vA = bodyA.LinearVelocity;
                    float wA = bodyA.AngularVelocity;
                    Vec2 vB = bodyB.LinearVelocity;
                    float wB = bodyB.AngularVelocity;

                    float mA = constraint.InvMassA;
                    float iA = constraint.InvIA;
                    float mB = constraint.InvMassB;
                    float iB = constraint.InvIB;

                    for (int p = 0; p < constraint.PointCount; ++p)
                    {
                        ContactConstraintPoint cp = constraint.Points[p];
                        Vec2 P = cp.NormalImpulse * constraint.Normal + cp.TangentImpulse * constraint.Tangent;
                        cp.TotalNormalImpulse += cp.NormalImpulse;

                        wA -= iA * Vec2.Cross(cp.RA, P);
                        vA -= mA * P;
                        wB += iB * Vec2.Cross(cp.RB, P);
                        vB += mB * P;

                        constraint.Points[p] = cp;
                    }

                    bodyA.LinearVelocity = vA;
                    bodyA.AngularVelocity = wA;
                    bodyB.LinearVelocity = vB;
                    bodyB.AngularVelocity = wB;
                }
            }

            public void SolveVelocity(bool useBias)
            {
                for (int i = 0; i < _constraints.Count; ++i)
                {
                    ContactConstraint constraint = _constraints[i];
                    Body bodyA = constraint.BodyA;
                    Body bodyB = constraint.BodyB;

                    Vec2 vA = bodyA.LinearVelocity;
                    float wA = bodyA.AngularVelocity;
                    Vec2 vB = bodyB.LinearVelocity;
                    float wB = bodyB.AngularVelocity;

                    float mA = constraint.InvMassA;
                    float iA = constraint.InvIA;
                    float mB = constraint.InvMassB;
                    float iB = constraint.InvIB;

                    float totalNormalImpulse = 0f;

                    for (int p = 0; p < constraint.PointCount; ++p)
                    {
                        ContactConstraintPoint cp = constraint.Points[p];

                        Vec2 vrA = vA + Vec2.Cross(wA, cp.RA);
                        Vec2 vrB = vB + Vec2.Cross(wB, cp.RB);
                        float vn = Vec2.Dot(vrB - vrA, constraint.Normal);

                        float bias = useBias ? cp.Bias : 0f;
                        float impulse = -(vn + bias + cp.Softness * cp.NormalImpulse) * cp.NormalMass;
                        float newImpulse = MathF.Max(cp.NormalImpulse + impulse, 0f);
                        impulse = newImpulse - cp.NormalImpulse;
                        cp.NormalImpulse = newImpulse;
                        cp.TotalNormalImpulse += impulse;

                        totalNormalImpulse += newImpulse;

                        Vec2 P = impulse * constraint.Normal;
                        vA -= mA * P;
                        wA -= iA * Vec2.Cross(cp.RA, P);
                        vB += mB * P;
                        wB += iB * Vec2.Cross(cp.RB, P);

                        constraint.Points[p] = cp;
                    }

                    // Friction
                    for (int p = 0; p < constraint.PointCount; ++p)
                    {
                        ContactConstraintPoint cp = constraint.Points[p];

                        Vec2 vrA = vA + Vec2.Cross(wA, cp.RA);
                        Vec2 vrB = vB + Vec2.Cross(wB, cp.RB);
                        float vt = Vec2.Dot(vrB - vrA, constraint.Tangent) - constraint.TangentSpeed;

                        float impulse = -vt * cp.TangentMass;
                        float maxFriction = constraint.Friction * cp.NormalImpulse;
                        float newImpulse = MathFng.Clamp(cp.TangentImpulse + impulse, -maxFriction, maxFriction);
                        impulse = newImpulse - cp.TangentImpulse;
                        cp.TangentImpulse = newImpulse;

                        Vec2 P = impulse * constraint.Tangent;
                        vA -= mA * P;
                        wA -= iA * Vec2.Cross(cp.RA, P);
                        vB += mB * P;
                        wB += iB * Vec2.Cross(cp.RB, P);

                        constraint.Points[p] = cp;
                    }

                    bodyA.LinearVelocity = vA;
                    bodyA.AngularVelocity = wA;
                    bodyB.LinearVelocity = vB;
                    bodyB.AngularVelocity = wB;

                    _constraints[i] = constraint;
                }
            }

            public void ApplyRestitution(float threshold)
            {
                for (int i = 0; i < _constraints.Count; ++i)
                {
                    ContactConstraint constraint = _constraints[i];
                    if (constraint.Restitution == 0f)
                    {
                        continue;
                    }

                    Body bodyA = constraint.BodyA;
                    Body bodyB = constraint.BodyB;

                    Vec2 vA = bodyA.LinearVelocity;
                    float wA = bodyA.AngularVelocity;
                    Vec2 vB = bodyB.LinearVelocity;
                    float wB = bodyB.AngularVelocity;

                    float mA = constraint.InvMassA;
                    float iA = constraint.InvIA;
                    float mB = constraint.InvMassB;
                    float iB = constraint.InvIB;

                    for (int p = 0; p < constraint.PointCount; ++p)
                    {
                        ContactConstraintPoint cp = constraint.Points[p];
                        if (cp.RelativeVelocity > -threshold || cp.TotalNormalImpulse == 0f)
                        {
                            continue;
                        }

                        Vec2 vrA = vA + Vec2.Cross(wA, cp.RA);
                        Vec2 vrB = vB + Vec2.Cross(wB, cp.RB);
                        float vn = Vec2.Dot(vrB - vrA, constraint.Normal);

                        float impulse = -cp.NormalMass * (vn + constraint.Restitution * cp.RelativeVelocity);
                        float newImpulse = MathF.Max(cp.NormalImpulse + impulse, 0f);
                        impulse = newImpulse - cp.NormalImpulse;
                        cp.NormalImpulse = newImpulse;
                        cp.TotalNormalImpulse += impulse;

                        Vec2 P = impulse * constraint.Normal;
                        vA -= mA * P;
                        wA -= iA * Vec2.Cross(cp.RA, P);
                        vB += mB * P;
                        wB += iB * Vec2.Cross(cp.RB, P);

                        constraint.Points[p] = cp;
                    }

                    bodyA.LinearVelocity = vA;
                    bodyA.AngularVelocity = wA;
                    bodyB.LinearVelocity = vB;
                    bodyB.AngularVelocity = wB;

                    _constraints[i] = constraint;
                }
            }

            public void StoreImpulses()
            {
                for (int i = 0; i < _constraints.Count; ++i)
                {
                    ContactConstraint constraint = _constraints[i];
                    Contact contact = constraint.Contact;
                    for (int p = 0; p < constraint.PointCount; ++p)
                    {
                        ContactConstraintPoint cp = constraint.Points[p];
                        ManifoldPoint mp = contact.Manifold.Points[p];
                        contact.Manifold.Points[p] = new ManifoldPoint(mp.LocalPoint, cp.NormalImpulse, cp.TangentImpulse, mp.Id);
                    }
                }
            }

            private struct ContactConstraintPoint
            {
                public Vec2 RA;
                public Vec2 RB;
                public float NormalImpulse;
                public float TangentImpulse;
                public float TotalNormalImpulse;
                public float NormalMass;
                public float TangentMass;
                public float BaseSeparation;
                public float RelativeVelocity;
                public float Bias;
                public float Softness;
            }

            private struct ContactConstraint
            {
                public Contact Contact;
                public Body BodyA;
                public Body BodyB;
                public Vec2 Normal;
                public Vec2 Tangent;
                public int PointCount;
                public float Friction;
                public float Restitution;
                public float RollingResistance;
                public float RollingImpulse;
                public float TangentSpeed;
                public float InvMassA;
                public float InvIA;
                public float InvMassB;
                public float InvIB;
                public ContactConstraintPoint[] Points;

                public ContactConstraint(Contact contact, Body bodyA, Body bodyB, Vec2 normal, Vec2 tangent, int pointCount)
                {
                    Contact = contact;
                    BodyA = bodyA;
                    BodyB = bodyB;
                    Normal = normal;
                    Tangent = tangent;
                    PointCount = pointCount;
                    Friction = 0f;
                    Restitution = 0f;
                    RollingResistance = 0f;
                    RollingImpulse = 0f;
                    TangentSpeed = 0f;
                    InvMassA = 0f;
                    InvIA = 0f;
                    InvMassB = 0f;
                    InvIB = 0f;
                    Points = new ContactConstraintPoint[2];
                }
            }
        }
    }
}
