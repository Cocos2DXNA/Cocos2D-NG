using System;

namespace Box2DNG
{
    internal sealed class HashSet64
    {
        private ulong[] _keys = Array.Empty<ulong>();
        private int _count;
        private int _capacity;

        public HashSet64(int capacity)
        {
            int cap = capacity > 16 ? BitUtils.RoundUpPowerOf2(capacity) : 16;
            _capacity = cap;
            _keys = Box2DAllocator.Alloc<ulong>(_capacity);
            Array.Clear(_keys, 0, _keys.Length);
        }

        public int Count => _count;
        public int Capacity => _capacity;

        public void Clear()
        {
            _count = 0;
            Array.Clear(_keys, 0, _keys.Length);
        }

        public bool Contains(ulong key)
        {
            if (key == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            ulong hash = KeyHash(key);
            int index = FindSlot(key, hash);
            return _keys[index] == key;
        }

        public bool Add(ulong key)
        {
            if (key == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            ulong hash = KeyHash(key);
            int index = FindSlot(key, hash);
            if (_keys[index] != 0)
            {
                return true;
            }

            if (2 * _count >= _capacity)
            {
                Grow();
            }

            AddKeyHaveCapacity(key, hash);
            return false;
        }

        public bool Remove(ulong key)
        {
            ulong hash = KeyHash(key);
            int i = FindSlot(key, hash);
            if (_keys[i] == 0)
            {
                return false;
            }

            _keys[i] = 0;
            _count--;

            int j = i;
            int capacity = _capacity;
            while (true)
            {
                j = (j + 1) & (capacity - 1);
                if (_keys[j] == 0)
                {
                    break;
                }

                ulong hashJ = KeyHash(_keys[j]);
                int k = (int)(hashJ & (uint)(capacity - 1));

                if (i <= j)
                {
                    if (i < k && k <= j)
                    {
                        continue;
                    }
                }
                else
                {
                    if (i < k || k <= j)
                    {
                        continue;
                    }
                }

                _keys[i] = _keys[j];
                _keys[j] = 0;
                i = j;
            }

            return true;
        }

        public int GetByteCount() => _capacity * sizeof(ulong);

        public void CopyKeys(System.Collections.Generic.List<ulong> keys)
        {
            keys.Clear();
            for (int i = 0; i < _keys.Length; ++i)
            {
                ulong key = _keys[i];
                if (key != 0)
                {
                    keys.Add(key);
                }
            }
        }

        private int FindSlot(ulong key, ulong hash)
        {
            int capacity = _capacity;
            int index = (int)(hash & (uint)(capacity - 1));
            while (_keys[index] != 0 && _keys[index] != key)
            {
                index = (index + 1) & (capacity - 1);
            }

            return index;
        }

        private void AddKeyHaveCapacity(ulong key, ulong hash)
        {
            int index = FindSlot(key, hash);
            if (_keys[index] != 0)
            {
                throw new InvalidOperationException("HashSet64 slot already occupied.");
            }

            _keys[index] = key;
            _count++;
        }

        private void Grow()
        {
            int oldCount = _count;
            int oldCapacity = _capacity;
            ulong[] oldKeys = _keys;

            _count = 0;
            _capacity = 2 * oldCapacity;
            _keys = Box2DAllocator.Alloc<ulong>(_capacity);
            Array.Clear(_keys, 0, _keys.Length);

            for (int i = 0; i < oldCapacity; ++i)
            {
                ulong key = oldKeys[i];
                if (key == 0)
                {
                    continue;
                }

                ulong hash = KeyHash(key);
                AddKeyHaveCapacity(key, hash);
            }

            if (_count != oldCount)
            {
                throw new InvalidOperationException("HashSet64 count mismatch after grow.");
            }

            Box2DAllocator.Free(oldKeys);
        }

        private static ulong KeyHash(ulong key)
        {
            ulong h = key;
            h ^= h >> 33;
            h *= 0xff51afd7ed558ccdUL;
            h ^= h >> 33;
            h *= 0xc4ceb9fe1a85ec53UL;
            h ^= h >> 33;
            return h;
        }
    }
}
