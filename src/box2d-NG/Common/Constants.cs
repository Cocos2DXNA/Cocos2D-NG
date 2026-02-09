using System;

namespace Box2DNG
{
    public static class Constants
    {
        public const int MaxPolygonVertices = 8;
        public const float Epsilon = 1.1920928955078125e-07f;
        public const float LinearSlop = 0.005f;
        public const float PolygonRadius = 2f * LinearSlop;
        public const float Baumgarte = 0.2f;
        public const float MaxLinearCorrection = 0.2f;
        public const float AngularSlop = 2f * MathF.PI / 180f;
        public const float VelocityThreshold = 1.0f;
        public const float TimeToSleep = 0.5f;
        public const float LinearSleepTolerance = 0.01f;
        public const float AngularSleepTolerance = 2f * MathF.PI / 180f;
    }
}
