using System;

namespace Box2DNG
{
    internal readonly struct Softness
    {
        public readonly float BiasRate;
        public readonly float MassScale;
        public readonly float ImpulseScale;

        public Softness(float biasRate, float massScale, float impulseScale)
        {
            BiasRate = biasRate;
            MassScale = massScale;
            ImpulseScale = impulseScale;
        }

        public static Softness Make(float hertz, float dampingRatio, float dt)
        {
            if (hertz <= 0f)
            {
                return new Softness(0f, 0f, 0f);
            }

            float omega = 2f * MathF.PI * hertz;
            float a1 = 2f * dampingRatio + dt * omega;
            float a2 = dt * omega * a1;
            float a3 = 1f / (1f + a2);

            return new Softness(
                biasRate: omega / a1,
                massScale: a2 * a3,
                impulseScale: a3);
        }
    }
}
