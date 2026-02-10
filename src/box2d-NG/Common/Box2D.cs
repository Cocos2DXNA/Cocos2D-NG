using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

namespace Box2DNG
{
    public readonly struct Box2DVersion
    {
        public readonly int Major;
        public readonly int Minor;
        public readonly int Revision;

        public Box2DVersion(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public override string ToString() => $"{Major}.{Minor}.{Revision}";
    }

    public static class Box2D
    {
        private static float _lengthUnitsPerMeter = 1f;
        private static Func<string, string, int, bool>? _assertHandler;
        private static Action<string>? _logHandler;

        public static float LengthUnitsPerMeter => _lengthUnitsPerMeter;

        public static void SetLengthUnitsPerMeter(float lengthUnits)
        {
            if (!MathFng.IsValidFloat(lengthUnits) || lengthUnits <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthUnits), "Length units must be positive and finite.");
            }

            _lengthUnitsPerMeter = lengthUnits;
        }

        public static Box2DVersion GetVersion() => new Box2DVersion(3, 2, 0);

        public static void SetAssertHandler(Func<string, string, int, bool> handler)
        {
            _assertHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public static void SetLogHandler(Action<string> handler)
        {
            _logHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public static void SetAllocator(IArrayAllocator allocator)
        {
            Box2DAllocator.SetAllocator(allocator);
        }

        public static int GetByteCount() => Box2DAllocator.GetByteCount();

        public static ulong GetTicks() => (ulong)Stopwatch.GetTimestamp();

        public static float GetMilliseconds(ulong ticks)
        {
            long now = Stopwatch.GetTimestamp();
            return (float)((now - (long)ticks) * 1000.0 / Stopwatch.Frequency);
        }

        public static float GetMillisecondsAndReset(ref ulong ticks)
        {
            long now = Stopwatch.GetTimestamp();
            float ms = (float)((now - (long)ticks) * 1000.0 / Stopwatch.Frequency);
            ticks = (ulong)now;
            return ms;
        }

        public static void Yield() => Thread.Yield();

        public static uint Hash(uint hash, ReadOnlySpan<byte> data)
        {
            uint result = hash;
            for (int i = 0; i < data.Length; i++)
            {
                result = (result << 5) + result + data[i];
            }

            return result;
        }

        public static uint Hash(uint hash, byte[] data, int count)
        {
            if (count <= 0)
            {
                return hash;
            }

            return Hash(hash, new ReadOnlySpan<byte>(data, 0, Math.Min(count, data.Length)));
        }

        internal static void Log(string message)
        {
            if (_logHandler != null)
            {
                _logHandler(message);
                return;
            }

            Debug.WriteLine($"Box2D: {message}");
        }

        internal static void Assert(bool condition, string conditionText, string fileName, int lineNumber)
        {
            if (condition)
            {
                return;
            }

            if (_assertHandler != null && _assertHandler(conditionText, fileName, lineNumber))
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    Debug.Fail($"BOX2D ASSERTION: {conditionText}, {fileName}, line {lineNumber}");
                }
            }
        }
    }

    internal static class Box2DAllocator
    {
        private static long _byteCount;
        private static IArrayAllocator? _allocator;

        public static void SetAllocator(IArrayAllocator allocator)
        {
            _allocator = allocator ?? throw new ArgumentNullException(nameof(allocator));
        }

        public static int GetByteCount() => (int)Math.Min(int.MaxValue, Interlocked.Read(ref _byteCount));

        public static T[] Alloc<T>(int count)
        {
            if (count <= 0)
            {
                return Array.Empty<T>();
            }

            int size = UnsafeSizeOf<T>() * count;
            Interlocked.Add(ref _byteCount, size);

            if (_allocator != null)
            {
                return _allocator.Allocate<T>(count);
            }

            return new T[count];
        }

        public static void Free<T>(T[]? array)
        {
            if (array == null || array.Length == 0)
            {
                return;
            }

            int size = UnsafeSizeOf<T>() * array.Length;
            Interlocked.Add(ref _byteCount, -size);

            _allocator?.Free(array);
        }

        public static T[] Grow<T>(T[]? oldArray, int newCount)
        {
            if (newCount <= 0)
            {
                return Array.Empty<T>();
            }

            T[] next = Alloc<T>(newCount);
            if (oldArray != null && oldArray.Length > 0)
            {
                Array.Copy(oldArray, next, Math.Min(oldArray.Length, next.Length));
                Free(oldArray);
            }

            return next;
        }

        private static int UnsafeSizeOf<T>() => System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
    }

    public interface IArrayAllocator
    {
        T[] Allocate<T>(int count);
        void Free<T>(T[] array);
    }
}
