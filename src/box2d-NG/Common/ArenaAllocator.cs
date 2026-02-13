using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Box2DNG
{
    public readonly struct ArenaBlock
    {
        public readonly byte[] Buffer;
        public readonly int Offset;
        public readonly int Size;
        public readonly bool UsedHeap;

        public ArenaBlock(byte[] buffer, int offset, int size, bool usedHeap)
        {
            Buffer = buffer;
            Offset = offset;
            Size = size;
            UsedHeap = usedHeap;
        }

        public Span<byte> Span => new Span<byte>(Buffer, Offset, Size);
    }

    public sealed class ArenaAllocator
    {
        private readonly List<ArenaEntry> _entries = new List<ArenaEntry>(32);
        private byte[] _data;
        private int _capacity;
        private int _index;
        private int _allocation;
        private int _maxAllocation;

        private readonly struct ArenaEntry
        {
            public readonly object Data;
            public readonly int Size;
            public readonly bool UsedHeap;
            public readonly bool IsBytes;
            public readonly int Offset;

            public ArenaEntry(object data, int size, bool usedHeap, bool isBytes, int offset)
            {
                Data = data;
                Size = size;
                UsedHeap = usedHeap;
                IsBytes = isBytes;
                Offset = offset;
            }
        }

        public ArenaAllocator(int capacity)
        {
            _capacity = Math.Max(0, capacity);
            _data = _capacity > 0 ? Box2DAllocator.Alloc<byte>(_capacity) : Array.Empty<byte>();
        }

        public int Capacity => _capacity;
        public int Allocation => _allocation;
        public int MaxAllocation => _maxAllocation;

        public void Destroy()
        {
            if (_entries.Count != 0)
            {
                Debug.Assert(false, "ArenaAllocator destroy called with outstanding allocations.");
            }

            Box2DAllocator.Free(_data);
            _data = Array.Empty<byte>();
            _capacity = 0;
            _index = 0;
            _allocation = 0;
            _maxAllocation = 0;
            _entries.Clear();
        }

        public ArenaBlock AllocateBytes(int size, string? name = null)
        {
            int size32 = Align32(size);
            bool usedHeap;
            int offset;
            byte[] buffer;

            if (_index + size32 > _capacity)
            {
                buffer = Box2DAllocator.Alloc<byte>(size32);
                offset = 0;
                usedHeap = true;
            }
            else
            {
                buffer = _data;
                offset = _index;
                _index += size32;
                usedHeap = false;
            }

            _allocation += size32;
            if (_allocation > _maxAllocation)
            {
                _maxAllocation = _allocation;
            }

            _entries.Add(new ArenaEntry(buffer, size32, usedHeap, true, offset));
            return new ArenaBlock(buffer, offset, size32, usedHeap);
        }

        public T[] AllocateArray<T>(int count, string? name = null)
        {
            if (count <= 0)
            {
                return Array.Empty<T>();
            }

            int size = System.Runtime.CompilerServices.Unsafe.SizeOf<T>() * count;
            int size32 = Align32(size);
            bool usedHeap = _index + size32 > _capacity;
            if (!usedHeap)
            {
                _index += size32;
            }

            _allocation += size32;
            if (_allocation > _maxAllocation)
            {
                _maxAllocation = _allocation;
            }

            T[] array = Box2DAllocator.Alloc<T>(count);
            _entries.Add(new ArenaEntry(array, size32, usedHeap, false, 0));
            return array;
        }

        public void FreeBytes(ArenaBlock block)
        {
            if (_entries.Count == 0)
            {
                Debug.Assert(false, "ArenaAllocator free with no entries.");
                return;
            }

            ArenaEntry entry = _entries[_entries.Count - 1];
            Debug.Assert(entry.IsBytes, "ArenaAllocator free order mismatch.");
            Debug.Assert(ReferenceEquals(entry.Data, block.Buffer), "ArenaAllocator free order mismatch.");
            Debug.Assert(entry.Offset == block.Offset && entry.Size == block.Size, "ArenaAllocator free order mismatch.");

            _entries.RemoveAt(_entries.Count - 1);
            if (entry.UsedHeap)
            {
                Box2DAllocator.Free((byte[])entry.Data);
            }
            else
            {
                _index -= entry.Size;
            }

            _allocation -= entry.Size;
        }

        public void FreeArray<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                return;
            }

            if (_entries.Count == 0)
            {
                Debug.Assert(false, "ArenaAllocator free with no entries.");
                return;
            }

            ArenaEntry entry = _entries[_entries.Count - 1];
            Debug.Assert(!entry.IsBytes, "ArenaAllocator free order mismatch.");
            Debug.Assert(ReferenceEquals(entry.Data, array), "ArenaAllocator free order mismatch.");

            _entries.RemoveAt(_entries.Count - 1);
            if (!entry.UsedHeap)
            {
                _index -= entry.Size;
            }

            _allocation -= entry.Size;
            Box2DAllocator.Free(array);
        }

        public void Grow()
        {
            Debug.Assert(_allocation == 0, "ArenaAllocator grow requires no outstanding allocations.");
            if (_maxAllocation > _capacity)
            {
                Box2DAllocator.Free(_data);
                _capacity = _maxAllocation + _maxAllocation / 2;
                _data = _capacity > 0 ? Box2DAllocator.Alloc<byte>(_capacity) : Array.Empty<byte>();
                _index = 0;
            }
        }

        private static int Align32(int size)
        {
            if (size <= 0)
            {
                return 0;
            }

            return ((size - 1) | 0x1F) + 1;
        }
    }
}
