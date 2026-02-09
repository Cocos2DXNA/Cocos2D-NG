# Cocos2D-NG Box2D-NG Notes

## How-To: Enable SIMD Contact Solver (Vector<T>)

The SIMD path uses `System.Numerics.Vector<float>` and only batches **single-point contacts**
that do **not share bodies** (graph-colored per step). It automatically falls back to scalar
for multi-point contacts or when hardware acceleration is unavailable.

```csharp
WorldDef def = new WorldDef()
    .EnableContactSolverSimdPath(true);

World world = new World(def);
```

### Reading SIMD stats

When SIMD is enabled, you can inspect per-step stats:

```csharp
World.ContactSolverStats stats = world.LastContactSolverStats;
// stats.SimdBatches, stats.SimdLanes, stats.Colors, etc.
```

## How-To: Toggle Soft Constraints vs Legacy Bias

Soft constraints are enabled by default. You can switch back to the legacy Baumgarte bias
without changing the rest of the solver.

```csharp
WorldDef def = new WorldDef()
    .EnableSoftening(true)
    .UseSoftConstraintsSolver(false); // legacy bias path
```

## How-To: Adjust Contact Softness Parameters

Soft constraints use `ContactHertz`, `ContactDampingRatio`, and `ContactSpeed`:

```csharp
WorldDef def = new WorldDef()
    .WithContactHertz(120f)
    .WithContactDamping(1f)
    .WithContactSpeed(15f);
```
