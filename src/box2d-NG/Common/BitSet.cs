using System;
using System.Numerics;

namespace Box2DNG
{
    internal sealed class BitSet
    {
        private ulong[] _bits;
        private uint _blockCapacity;
        private uint _blockCount;

        public BitSet(uint bitCapacity)
        {
            _blockCapacity = (bitCapacity + 63) / 64;
            _blockCount = 0;
            _bits = _blockCapacity > 0 ? Box2DAllocator.Alloc<ulong>((int)_blockCapacity) : Array.Empty<ulong>();
            if (_bits.Length > 0)
            {
                Array.Clear(_bits, 0, _bits.Length);
            }
        }

        public uint BlockCount => _blockCount;
        public uint BlockCapacity => _blockCapacity;

        public void Destroy()
        {
            Box2DAllocator.Free(_bits);
            _bits = Array.Empty<ulong>();
            _blockCapacity = 0;
            _blockCount = 0;
        }

        public void SetBitCountAndClear(uint bitCount)
        {
            uint blockCount = (bitCount + 63) / 64;
            if (_blockCapacity < blockCount)
            {
                Destroy();
                uint newBitCapacity = bitCount + (bitCount >> 1);
                _blockCapacity = (newBitCapacity + 63) / 64;
                _bits = _blockCapacity > 0 ? Box2DAllocator.Alloc<ulong>((int)_blockCapacity) : Array.Empty<ulong>();
            }

            _blockCount = blockCount;
            if (_blockCount > 0)
            {
                Array.Clear(_bits, 0, (int)_blockCount);
            }
        }

        public void Grow(uint blockCount)
        {
            if (blockCount <= _blockCount)
            {
                return;
            }

            if (blockCount > _blockCapacity)
            {
                uint oldCapacity = _blockCapacity;
                _blockCapacity = blockCount + blockCount / 2;
                ulong[] newBits = _blockCapacity > 0 ? Box2DAllocator.Alloc<ulong>((int)_blockCapacity) : Array.Empty<ulong>();
                if (newBits.Length > 0)
                {
                    Array.Clear(newBits, 0, newBits.Length);
                }

                if (_bits.Length > 0)
                {
                    Array.Copy(_bits, newBits, (int)oldCapacity);
                    Box2DAllocator.Free(_bits);
                }

                _bits = newBits;
            }

            _blockCount = blockCount;
        }

        public void InPlaceUnion(BitSet other)
        {
            if (_blockCount != other._blockCount)
            {
                throw new InvalidOperationException("BitSet block counts must match for union.");
            }

            for (uint i = 0; i < _blockCount; ++i)
            {
                _bits[i] |= other._bits[i];
            }
        }

        public void SetBit(uint bitIndex)
        {
            uint blockIndex = bitIndex / 64;
            if (blockIndex >= _blockCount)
            {
                throw new ArgumentOutOfRangeException(nameof(bitIndex));
            }

            _bits[blockIndex] |= 1UL << (int)(bitIndex % 64);
        }

        public void SetBitGrow(uint bitIndex)
        {
            uint blockIndex = bitIndex / 64;
            if (blockIndex >= _blockCount)
            {
                Grow(blockIndex + 1);
            }

            _bits[blockIndex] |= 1UL << (int)(bitIndex % 64);
        }

        public void ClearBit(uint bitIndex)
        {
            uint blockIndex = bitIndex / 64;
            if (blockIndex >= _blockCount)
            {
                return;
            }

            _bits[blockIndex] &= ~(1UL << (int)(bitIndex % 64));
        }

        public bool GetBit(uint bitIndex)
        {
            uint blockIndex = bitIndex / 64;
            if (blockIndex >= _blockCount)
            {
                return false;
            }

            return (_bits[blockIndex] & (1UL << (int)(bitIndex % 64))) != 0;
        }

        public int CountSetBits()
        {
            int popCount = 0;
            for (uint i = 0; i < _blockCount; ++i)
            {
                popCount += BitOperations.PopCount(_bits[i]);
            }

            return popCount;
        }

        public int GetByteCount() => (int)_blockCapacity * sizeof(ulong);
    }
}
