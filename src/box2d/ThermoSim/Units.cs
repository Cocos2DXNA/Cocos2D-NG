using System;

namespace ThermoSim
{
    public static class Units
    {
        public const float InchesPerFoot = 12.0f;
        public const float SecondsPerHour = 3600.0f;
        public const float CubicFeetPerMinuteToCubicFeetPerSecond = 1.0f / 60.0f;

        public static float InchesToFeet(float inches) => inches / InchesPerFoot;
        public static float CfmToCfs(float cfm) => cfm * CubicFeetPerMinuteToCubicFeetPerSecond;
        public static float BtuPerHourToBtuPerSecond(float btuPerHour) => btuPerHour / SecondsPerHour;
    }
}
