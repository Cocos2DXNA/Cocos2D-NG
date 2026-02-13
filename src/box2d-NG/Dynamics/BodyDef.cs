using System;

namespace Box2DNG
{
    public enum BodyType
    {
        Static = 0,
        Kinematic = 1,
        Dynamic = 2
    }

    [Flags]
    public enum MotionLocks
    {
        None = 0,
        LinearX = 1 << 0,
        LinearY = 1 << 1,
        AngularZ = 1 << 2
    }

    public sealed class BodyDef
    {
        public BodyType Type { get; private set; } = BodyType.Static;
        public Vec2 Position { get; private set; } = Vec2.Zero;
        public Rot Rotation { get; private set; } = Rot.Identity;
        public Vec2 LinearVelocity { get; private set; } = Vec2.Zero;
        public float AngularVelocity { get; private set; }
        public float LinearDamping { get; private set; }
        public float AngularDamping { get; private set; }
        public float GravityScale { get; private set; } = 1f;
        public bool AllowSleep { get; private set; } = true;
        public bool Awake { get; private set; } = true;
        public bool FixedRotation { get; private set; }
        public bool Bullet { get; private set; }
        public bool Active { get; private set; } = true;
        public MotionLocks MotionLocks { get; private set; } = MotionLocks.None;
        public object? UserData { get; private set; }
        public string? Name { get; private set; }

        public BodyDef AsDynamic() { Type = BodyType.Dynamic; return this; }
        public BodyDef AsKinematic() { Type = BodyType.Kinematic; return this; }
        public BodyDef AsStatic() { Type = BodyType.Static; return this; }

        public BodyDef At(Vec2 position) { Position = position; return this; }
        public BodyDef At(float x, float y) { Position = new Vec2(x, y); return this; }
        public BodyDef WithRotation(Rot rotation) { Rotation = rotation; return this; }
        public BodyDef WithAngle(float radians) { Rotation = new Rot(radians); return this; }
        public BodyDef WithLinearVelocity(Vec2 velocity) { LinearVelocity = velocity; return this; }
        public BodyDef WithAngularVelocity(float velocity) { AngularVelocity = velocity; return this; }
        public BodyDef WithLinearDamping(float damping) { LinearDamping = damping; return this; }
        public BodyDef WithAngularDamping(float damping) { AngularDamping = damping; return this; }
        public BodyDef WithGravityScale(float scale) { GravityScale = scale; return this; }
        public BodyDef AllowSleeping(bool allow) { AllowSleep = allow; return this; }
        public BodyDef IsAwake(bool awake) { Awake = awake; return this; }
        public BodyDef LockRotation(bool fixedRotation) { FixedRotation = fixedRotation; return this; }
        public BodyDef IsBullet(bool bullet) { Bullet = bullet; return this; }
        public BodyDef IsActive(bool active) { Active = active; return this; }
        public BodyDef LockMotion(MotionLocks locks) { MotionLocks = locks; return this; }
        public BodyDef WithUserData(object? data) { UserData = data; return this; }
        public BodyDef WithName(string? name) { Name = name; return this; }
    }
}
