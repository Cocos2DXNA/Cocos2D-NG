using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Box2DNG
{
    internal sealed class IdPool
    {
        private readonly List<int> _free = new List<int>(32);
        private int _nextIndex;

        public int Alloc()
        {
            int count = _free.Count;
            if (count > 0)
            {
                int id = _free[count - 1];
                _free.RemoveAt(count - 1);
                return id;
            }

            int next = _nextIndex;
            _nextIndex++;
            return next;
        }

        public void Free(int id)
        {
            Debug.Assert(_nextIndex > 0);
            Debug.Assert(id >= 0 && id < _nextIndex);
            _free.Add(id);
        }

        public void ValidateFreeId(int id)
        {
            for (int i = 0; i < _free.Count; i++)
            {
                if (_free[i] == id)
                {
                    return;
                }
            }

            Debug.Assert(false);
        }

        public void ValidateUsedId(int id)
        {
            for (int i = 0; i < _free.Count; i++)
            {
                if (_free[i] == id)
                {
                    Debug.Assert(false);
                }
            }
        }

        public int Count => _nextIndex - _free.Count;
        public int Capacity => _nextIndex;
        public int ByteCount => _free.Capacity * sizeof(int);

        public void Reset()
        {
            _free.Clear();
            _nextIndex = 0;
        }
    }
}
