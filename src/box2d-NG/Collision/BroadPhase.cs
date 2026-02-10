using System;
using System.Collections.Generic;

namespace Box2DNG
{
    public sealed class BroadPhase<T>
    {
        private readonly DynamicTree<T> _tree = new DynamicTree<T>();
        private readonly List<int> _moveBuffer = new List<int>();
        private readonly List<Pair> _pairBuffer = new List<Pair>();
        private readonly HashSet64 _moveSet = new HashSet64(16);
        private readonly HashSet64 _pairSet = new HashSet64(32);
        private int _proxyCount;

        public int ProxyCount => _proxyCount;

        public int CreateProxy(Aabb aabb, T userData)
        {
            int proxyId = _tree.CreateProxy(aabb, userData);
            _proxyCount++;
            BufferMove(proxyId);
            return proxyId;
        }

        public void DestroyProxy(int proxyId)
        {
            UnbufferMove(proxyId);
            _proxyCount--;
            _tree.DestroyProxy(proxyId);
        }

        public void MoveProxy(int proxyId, Aabb aabb, Vec2 displacement)
        {
            _tree.MoveProxy(proxyId, aabb, displacement);
            BufferMove(proxyId);
        }

        public void TouchProxy(int proxyId) => BufferMove(proxyId);

        public T? GetUserData(int proxyId) => _tree.GetUserData(proxyId);

        public Aabb GetAabb(int proxyId) => _tree.GetAabb(proxyId);

        public void Query(Func<int, bool> callback, Aabb aabb) => _tree.Query(callback, aabb);

        public void RayCast(Func<int, float> callback, RayCastInput input) => _tree.RayCast(callback, input);

        public void ShapeCast(Func<int, float> callback, ShapeCastInput input) => _tree.ShapeCast(callback, input);

        public void UpdatePairs(Action<T?, T?> callback)
        {
            _pairBuffer.Clear();
            _pairSet.Clear();

            for (int i = 0; i < _moveBuffer.Count; i++)
            {
                int proxyId = _moveBuffer[i];
                Aabb fatAabb = _tree.GetAabb(proxyId);
                _tree.Query(otherId =>
                {
                    if (otherId == proxyId)
                    {
                        return true;
                    }

                    int a = Math.Min(proxyId, otherId);
                    int b = Math.Max(proxyId, otherId);
                    ulong key = MakePairKey(a, b);
                    if (!_pairSet.Add(key))
                    {
                        _pairBuffer.Add(new Pair(a, b));
                    }
                    return true;
                }, fatAabb);
            }

            _moveBuffer.Clear();
            _moveSet.Clear();

            if (_pairBuffer.Count == 0)
            {
                return;
            }

            _pairBuffer.Sort();

            int pairIndex = 0;
            while (pairIndex < _pairBuffer.Count)
            {
                Pair pair = _pairBuffer[pairIndex];
                T? userDataA = _tree.GetUserData(pair.ProxyIdA);
                T? userDataB = _tree.GetUserData(pair.ProxyIdB);
                callback(userDataA, userDataB);

                pairIndex++;
                while (pairIndex < _pairBuffer.Count && _pairBuffer[pairIndex].Equals(pair))
                {
                    pairIndex++;
                }
            }
        }

        private void BufferMove(int proxyId)
        {
            ulong key = MakeMoveKey(proxyId);
            if (_moveSet.Add(key))
            {
                return;
            }

            _moveBuffer.Add(proxyId);
        }

        private void UnbufferMove(int proxyId)
        {
            ulong key = MakeMoveKey(proxyId);
            if (!_moveSet.Remove(key))
            {
                return;
            }

            for (int i = 0; i < _moveBuffer.Count; i++)
            {
                if (_moveBuffer[i] == proxyId)
                {
                    int last = _moveBuffer.Count - 1;
                    _moveBuffer[i] = _moveBuffer[last];
                    _moveBuffer.RemoveAt(last);
                    return;
                }
            }
        }

        private static ulong MakeMoveKey(int proxyId)
        {
            return (ulong)(proxyId + 1);
        }

        private static ulong MakePairKey(int proxyIdA, int proxyIdB)
        {
            return proxyIdA < proxyIdB
                ? ((ulong)proxyIdA << 32) | (uint)proxyIdB
                : ((ulong)proxyIdB << 32) | (uint)proxyIdA;
        }

        private readonly struct Pair : IComparable<Pair>, IEquatable<Pair>
        {
            public readonly int ProxyIdA;
            public readonly int ProxyIdB;

            public Pair(int proxyIdA, int proxyIdB)
            {
                ProxyIdA = proxyIdA;
                ProxyIdB = proxyIdB;
            }

            public int CompareTo(Pair other)
            {
                if (ProxyIdA < other.ProxyIdA)
                {
                    return -1;
                }
                if (ProxyIdA > other.ProxyIdA)
                {
                    return 1;
                }
                if (ProxyIdB < other.ProxyIdB)
                {
                    return -1;
                }
                if (ProxyIdB > other.ProxyIdB)
                {
                    return 1;
                }
                return 0;
            }

            public bool Equals(Pair other) => ProxyIdA == other.ProxyIdA && ProxyIdB == other.ProxyIdB;

            public override bool Equals(object? obj) => obj is Pair other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = (hash * 31) + ProxyIdA;
                    hash = (hash * 31) + ProxyIdB;
                    return hash;
                }
            }
        }
    }
}
