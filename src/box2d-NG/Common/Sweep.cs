using System;

namespace Box2DNG
{
    public readonly struct Sweep
    {
        public readonly Vec2 LocalCenter;
        public readonly Vec2 C0;
        public readonly Vec2 C;
        public readonly float A0;
        public readonly float A;
        public readonly float Alpha0;

        public Sweep(Vec2 localCenter, Vec2 c0, Vec2 c, float a0, float a, float alpha0)
        {
            LocalCenter = localCenter;
            C0 = c0;
            C = c;
            A0 = a0;
            A = a;
            Alpha0 = alpha0;
        }

        public Transform GetTransform(float beta)
        {
            float x = (1f - beta) * C0.X + beta * C.X;
            float y = (1f - beta) * C0.Y + beta * C.Y;
            float angle = (1f - beta) * A0 + beta * A;

            Rot q = new Rot(angle);
            Vec2 p = new Vec2(x - (q.C * LocalCenter.X - q.S * LocalCenter.Y),
                              y - (q.S * LocalCenter.X + q.C * LocalCenter.Y));
            return new Transform(p, q);
        }

        public Sweep Advance(float alpha)
        {
            float beta = (alpha - Alpha0) / (1f - Alpha0);
            Vec2 c0 = (1f - beta) * C0 + beta * C;
            float a0 = (1f - beta) * A0 + beta * A;
            return new Sweep(LocalCenter, c0, C, a0, A, alpha);
        }

        public Sweep Normalize()
        {
            float twoPi = 2f * MathFng.Pi;
            float d = twoPi * MathF.Floor(A0 / twoPi);
            return new Sweep(LocalCenter, C0, C, A0 - d, A - d, Alpha0);
        }
    }
}
