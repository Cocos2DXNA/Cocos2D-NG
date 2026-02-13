using System.Numerics;

namespace Box2DNG
{
    internal static class BitUtils
    {
        public static bool IsPowerOf2(int value)
        {
            return (value & (value - 1)) == 0;
        }

        public static int BoundingPowerOf2(int value)
        {
            if (value <= 1)
            {
                return 1;
            }

            return 32 - BitOperations.LeadingZeroCount((uint)value - 1);
        }

        public static int RoundUpPowerOf2(int value)
        {
            if (value <= 1)
            {
                return 1;
            }

            return 1 << (32 - BitOperations.LeadingZeroCount((uint)value - 1));
        }
    }
}
