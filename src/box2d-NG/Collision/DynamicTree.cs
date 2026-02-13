using System;
using System.Collections.Generic;

namespace Box2DNG
{
    public sealed class DynamicTree<T>
    {
        private const float AabbExtension = 0.1f;
        private const float AabbMultiplier = 2f;

        private sealed class Node
        {
            public Aabb Aabb;
            public T? UserData;
            public int Parent = -1;
            public int Child1 = -1;
            public int Child2 = -1;
            public int Height = -1;

            public bool IsLeaf => Child1 == -1;
        }

        private readonly List<Node> _nodes = new List<Node>();
        private readonly IdPool _nodeIdPool = new IdPool();
        private int _root = -1;

        public int CreateProxy(Aabb aabb, T userData)
        {
            int id = AllocateNode();
            Node node = _nodes[id];
            node.Aabb = FattenAabb(aabb, Vec2.Zero);
            node.UserData = userData;
            node.Height = 0;
            _nodes[id] = node;

            InsertLeaf(id);
            return id;
        }

        public void DestroyProxy(int proxyId)
        {
            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        public void MoveProxy(int proxyId, Aabb aabb, Vec2 displacement)
        {
            Node node = _nodes[proxyId];
            Aabb fatAabb = FattenAabb(aabb, displacement);
            if (Contains(node.Aabb, fatAabb))
            {
                return;
            }

            RemoveLeaf(proxyId);
            node.Aabb = fatAabb;
            _nodes[proxyId] = node;
            InsertLeaf(proxyId);
        }

        public T? GetUserData(int proxyId) => _nodes[proxyId].UserData;

        public Aabb GetAabb(int proxyId) => _nodes[proxyId].Aabb;

        public int GetHeight() => _root == -1 ? 0 : _nodes[_root].Height;

        public void Query(Func<int, bool> callback, Aabb aabb)
        {
            if (_root == -1)
            {
                return;
            }

            Stack<int> stack = new Stack<int>();
            stack.Push(_root);
            while (stack.Count > 0)
            {
                int nodeId = stack.Pop();
                if (!TestOverlap(_nodes[nodeId].Aabb, aabb))
                {
                    continue;
                }

                if (_nodes[nodeId].IsLeaf)
                {
                    if (!callback(nodeId))
                    {
                        return;
                    }
                }
                else
                {
                    stack.Push(_nodes[nodeId].Child1);
                    stack.Push(_nodes[nodeId].Child2);
                }
            }
        }

        public void RayCast(Func<int, float> callback, RayCastInput input)
        {
            if (_root == -1)
            {
                return;
            }

            Vec2 p1 = input.Origin;
            Vec2 p2 = input.Origin + input.Translation;
            Vec2 r = p2 - p1;
            r = r.Normalize();

            Vec2 v = MathFng.RightPerp(r);
            Vec2 absV = new Vec2(MathF.Abs(v.X), MathF.Abs(v.Y));

            float maxFraction = input.MaxFraction;

            Stack<int> stack = new Stack<int>();
            stack.Push(_root);
            while (stack.Count > 0)
            {
                int nodeId = stack.Pop();
                Node node = _nodes[nodeId];

                if (!RayAabb(p1, p2, maxFraction, node.Aabb, v, absV))
                {
                    continue;
                }

                if (node.IsLeaf)
                {
                    float value = callback(nodeId);
                    if (value == 0f)
                    {
                        return;
                    }

                    if (value > 0f)
                    {
                        maxFraction = value;
                    }
                }
                else
                {
                    stack.Push(node.Child1);
                    stack.Push(node.Child2);
                }
            }
        }

        public void ShapeCast(Func<int, float> callback, ShapeCastInput input)
        {
            if (_root == -1 || input.Proxy.Count == 0)
            {
                return;
            }

            Aabb originAabb = ComputeProxyAabb(input.Proxy);
            Vec2 p1 = originAabb.Center;
            Vec2 extension = originAabb.Extents;

            Vec2 r = input.Translation;
            Vec2 v = MathFng.RightPerp(r);
            Vec2 absV = new Vec2(MathF.Abs(v.X), MathF.Abs(v.Y));

            float maxFraction = input.MaxFraction;
            Vec2 t = maxFraction * input.Translation;
            Aabb totalAabb = Combine(originAabb, OffsetAabb(originAabb, t));

            Stack<int> stack = new Stack<int>();
            stack.Push(_root);

            while (stack.Count > 0)
            {
                int nodeId = stack.Pop();
                Node node = _nodes[nodeId];

                if (!TestOverlap(node.Aabb, totalAabb))
                {
                    continue;
                }

                Vec2 c = node.Aabb.Center;
                Vec2 h = node.Aabb.Extents + extension;
                float term1 = MathF.Abs(Vec2.Dot(v, p1 - c));
                float term2 = Vec2.Dot(absV, h);
                if (term2 < term1)
                {
                    continue;
                }

                if (node.IsLeaf)
                {
                    float value = callback(nodeId);
                    if (value == 0f)
                    {
                        return;
                    }

                    if (value > 0f && value < maxFraction)
                    {
                        maxFraction = value;
                        t = maxFraction * input.Translation;
                        totalAabb = Combine(originAabb, OffsetAabb(originAabb, t));
                    }
                }
                else
                {
                    stack.Push(node.Child1);
                    stack.Push(node.Child2);
                }
            }
        }

        private int AllocateNode()
        {
            int id = _nodeIdPool.Alloc();
            if (id == _nodes.Count)
            {
                _nodes.Add(new Node());
            }

            Node node = _nodes[id];
            node.Parent = -1;
            node.Child1 = -1;
            node.Child2 = -1;
            node.Height = 0;
            node.UserData = default;
            _nodes[id] = node;
            return id;
        }

        private void FreeNode(int id)
        {
            Node node = _nodes[id];
            node.Height = -1;
            node.UserData = default;
            _nodes[id] = node;
            _nodeIdPool.Free(id);
        }

        private void InsertLeaf(int leaf)
        {
            if (_root == -1)
            {
                _root = leaf;
                _nodes[leaf].Parent = -1;
                return;
            }

            Aabb leafAabb = _nodes[leaf].Aabb;
            int index = _root;
            while (!_nodes[index].IsLeaf)
            {
                int child1 = _nodes[index].Child1;
                int child2 = _nodes[index].Child2;

                float area = _nodes[index].Aabb.Perimeter;
                Aabb combined = Combine(_nodes[index].Aabb, leafAabb);
                float combinedArea = combined.Perimeter;
                float cost = 2f * combinedArea;
                float inheritanceCost = 2f * (combinedArea - area);

                float cost1 = CostForChild(child1, leafAabb, inheritanceCost);
                float cost2 = CostForChild(child2, leafAabb, inheritanceCost);

                if (cost < cost1 && cost < cost2)
                {
                    break;
                }

                index = cost1 < cost2 ? child1 : child2;
            }

            int sibling = index;
            int oldParent = _nodes[sibling].Parent;
            int newParent = AllocateNode();

            Node parentNode = _nodes[newParent];
            parentNode.Parent = oldParent;
            parentNode.Aabb = Combine(leafAabb, _nodes[sibling].Aabb);
            parentNode.Height = _nodes[sibling].Height + 1;
            parentNode.Child1 = sibling;
            parentNode.Child2 = leaf;
            _nodes[newParent] = parentNode;

            _nodes[sibling].Parent = newParent;
            _nodes[leaf].Parent = newParent;

            if (oldParent == -1)
            {
                _root = newParent;
            }
            else
            {
                if (_nodes[oldParent].Child1 == sibling)
                {
                    _nodes[oldParent].Child1 = newParent;
                }
                else
                {
                    _nodes[oldParent].Child2 = newParent;
                }
            }

            index = _nodes[leaf].Parent;
            while (index != -1)
            {
                index = Balance(index);
                Node node = _nodes[index];

                int child1 = node.Child1;
                int child2 = node.Child2;
                node.Height = 1 + Math.Max(_nodes[child1].Height, _nodes[child2].Height);
                node.Aabb = Combine(_nodes[child1].Aabb, _nodes[child2].Aabb);
                _nodes[index] = node;

                index = node.Parent;
            }
        }

        private void RemoveLeaf(int leaf)
        {
            if (leaf == _root)
            {
                _root = -1;
                return;
            }

            int parent = _nodes[leaf].Parent;
            int grandParent = _nodes[parent].Parent;
            int sibling = _nodes[parent].Child1 == leaf ? _nodes[parent].Child2 : _nodes[parent].Child1;

            if (grandParent != -1)
            {
                if (_nodes[grandParent].Child1 == parent)
                {
                    _nodes[grandParent].Child1 = sibling;
                }
                else
                {
                    _nodes[grandParent].Child2 = sibling;
                }
                _nodes[sibling].Parent = grandParent;
                FreeNode(parent);

                int index = grandParent;
                while (index != -1)
                {
                    index = Balance(index);
                    Node node = _nodes[index];
                    int child1 = node.Child1;
                    int child2 = node.Child2;
                    node.Aabb = Combine(_nodes[child1].Aabb, _nodes[child2].Aabb);
                    node.Height = 1 + Math.Max(_nodes[child1].Height, _nodes[child2].Height);
                    _nodes[index] = node;
                    index = node.Parent;
                }
            }
            else
            {
                _root = sibling;
                _nodes[sibling].Parent = -1;
                FreeNode(parent);
            }
        }

        private int Balance(int iA)
        {
            Node a = _nodes[iA];
            if (a.IsLeaf || a.Height < 2)
            {
                return iA;
            }

            int iB = a.Child1;
            int iC = a.Child2;
            Node b = _nodes[iB];
            Node c = _nodes[iC];

            int balance = c.Height - b.Height;
            if (balance > 1)
            {
                int iF = c.Child1;
                int iG = c.Child2;
                Node f = _nodes[iF];
                Node g = _nodes[iG];

                c.Child1 = iA;
                c.Parent = a.Parent;
                a.Parent = iC;

                if (c.Parent != -1)
                {
                    if (_nodes[c.Parent].Child1 == iA)
                    {
                        _nodes[c.Parent].Child1 = iC;
                    }
                    else
                    {
                        _nodes[c.Parent].Child2 = iC;
                    }
                }
                else
                {
                    _root = iC;
                }

                if (f.Height > g.Height)
                {
                    c.Child2 = iF;
                    a.Child2 = iG;
                    _nodes[iG].Parent = iA;
                    a.Aabb = Combine(b.Aabb, _nodes[iG].Aabb);
                    c.Aabb = Combine(a.Aabb, f.Aabb);

                    a.Height = 1 + Math.Max(b.Height, _nodes[iG].Height);
                    c.Height = 1 + Math.Max(a.Height, f.Height);
                }
                else
                {
                    c.Child2 = iG;
                    a.Child2 = iF;
                    _nodes[iF].Parent = iA;
                    a.Aabb = Combine(b.Aabb, _nodes[iF].Aabb);
                    c.Aabb = Combine(a.Aabb, g.Aabb);

                    a.Height = 1 + Math.Max(b.Height, _nodes[iF].Height);
                    c.Height = 1 + Math.Max(a.Height, g.Height);
                }

                _nodes[iA] = a;
                _nodes[iC] = c;
                return iC;
            }

            if (balance < -1)
            {
                int iD = b.Child1;
                int iE = b.Child2;
                Node d = _nodes[iD];
                Node e = _nodes[iE];

                b.Child1 = iA;
                b.Parent = a.Parent;
                a.Parent = iB;

                if (b.Parent != -1)
                {
                    if (_nodes[b.Parent].Child1 == iA)
                    {
                        _nodes[b.Parent].Child1 = iB;
                    }
                    else
                    {
                        _nodes[b.Parent].Child2 = iB;
                    }
                }
                else
                {
                    _root = iB;
                }

                if (d.Height > e.Height)
                {
                    b.Child2 = iD;
                    a.Child1 = iE;
                    _nodes[iE].Parent = iA;
                    a.Aabb = Combine(c.Aabb, _nodes[iE].Aabb);
                    b.Aabb = Combine(a.Aabb, d.Aabb);

                    a.Height = 1 + Math.Max(c.Height, _nodes[iE].Height);
                    b.Height = 1 + Math.Max(a.Height, d.Height);
                }
                else
                {
                    b.Child2 = iE;
                    a.Child1 = iD;
                    _nodes[iD].Parent = iA;
                    a.Aabb = Combine(c.Aabb, _nodes[iD].Aabb);
                    b.Aabb = Combine(a.Aabb, e.Aabb);

                    a.Height = 1 + Math.Max(c.Height, _nodes[iD].Height);
                    b.Height = 1 + Math.Max(a.Height, e.Height);
                }

                _nodes[iA] = a;
                _nodes[iB] = b;
                return iB;
            }

            return iA;
        }

        private float CostForChild(int childId, Aabb leafAabb, float inheritanceCost)
        {
            Node child = _nodes[childId];
            if (child.IsLeaf)
            {
                Aabb combined = Combine(child.Aabb, leafAabb);
                return combined.Perimeter + inheritanceCost;
            }
            else
            {
                Aabb combined = Combine(child.Aabb, leafAabb);
                float oldArea = child.Aabb.Perimeter;
                float newArea = combined.Perimeter;
                return (newArea - oldArea) + inheritanceCost;
            }
        }

        private static bool Contains(Aabb a, Aabb b)
        {
            return a.LowerBound.X <= b.LowerBound.X &&
                   a.LowerBound.Y <= b.LowerBound.Y &&
                   a.UpperBound.X >= b.UpperBound.X &&
                   a.UpperBound.Y >= b.UpperBound.Y;
        }

        private static bool TestOverlap(Aabb a, Aabb b)
        {
            if (a.UpperBound.X < b.LowerBound.X || a.LowerBound.X > b.UpperBound.X)
            {
                return false;
            }
            if (a.UpperBound.Y < b.LowerBound.Y || a.LowerBound.Y > b.UpperBound.Y)
            {
                return false;
            }
            return true;
        }

        private static bool RayAabb(Vec2 p1, Vec2 p2, float maxFraction, Aabb aabb, Vec2 v, Vec2 absV)
        {
            Vec2 c = aabb.Center;
            Vec2 h = aabb.Extents;

            Vec2 p = 0.5f * (p1 + p2);
            Vec2 d = p2 - p1;

            float separation = MathF.Abs(Vec2.Dot(v, p1 - c)) - Vec2.Dot(absV, h);
            if (separation > 0f)
            {
                return false;
            }

            float t = maxFraction;
            if (MathF.Abs(d.X) > 1e-12f)
            {
                float inv = 1f / d.X;
                float t1 = (aabb.LowerBound.X - p1.X) * inv;
                float t2 = (aabb.UpperBound.X - p1.X) * inv;
                float tMin = MathF.Min(t1, t2);
                float tMax = MathF.Max(t1, t2);
                t = MathF.Min(t, tMax);
                if (tMin > t)
                {
                    return false;
                }
            }

            if (MathF.Abs(d.Y) > 1e-12f)
            {
                float inv = 1f / d.Y;
                float t1 = (aabb.LowerBound.Y - p1.Y) * inv;
                float t2 = (aabb.UpperBound.Y - p1.Y) * inv;
                float tMin = MathF.Min(t1, t2);
                float tMax = MathF.Max(t1, t2);
                t = MathF.Min(t, tMax);
                if (tMin > t)
                {
                    return false;
                }
            }

            return true;
        }

        private static Aabb Combine(Aabb a, Aabb b)
        {
            Vec2 lower = new Vec2(MathF.Min(a.LowerBound.X, b.LowerBound.X), MathF.Min(a.LowerBound.Y, b.LowerBound.Y));
            Vec2 upper = new Vec2(MathF.Max(a.UpperBound.X, b.UpperBound.X), MathF.Max(a.UpperBound.Y, b.UpperBound.Y));
            return new Aabb(lower, upper);
        }

        private static Aabb OffsetAabb(Aabb aabb, Vec2 translation)
        {
            return new Aabb(aabb.LowerBound + translation, aabb.UpperBound + translation);
        }

        private static Aabb ComputeProxyAabb(ShapeProxy proxy)
        {
            Vec2 lower = proxy.Points[0];
            Vec2 upper = proxy.Points[0];
            for (int i = 1; i < proxy.Count; ++i)
            {
                Vec2 p = proxy.Points[i];
                lower = new Vec2(MathF.Min(lower.X, p.X), MathF.Min(lower.Y, p.Y));
                upper = new Vec2(MathF.Max(upper.X, p.X), MathF.Max(upper.Y, p.Y));
            }

            Vec2 radius = new Vec2(proxy.Radius, proxy.Radius);
            return new Aabb(lower - radius, upper + radius);
        }
        private static Aabb FattenAabb(Aabb aabb, Vec2 displacement)
        {
            Vec2 extension = new Vec2(AabbExtension, AabbExtension);
            Vec2 lower = aabb.LowerBound - extension;
            Vec2 upper = aabb.UpperBound + extension;

            if (displacement.X < 0f)
            {
                lower = new Vec2(lower.X + displacement.X * AabbMultiplier, lower.Y);
            }
            else
            {
                upper = new Vec2(upper.X + displacement.X * AabbMultiplier, upper.Y);
            }

            if (displacement.Y < 0f)
            {
                lower = new Vec2(lower.X, lower.Y + displacement.Y * AabbMultiplier);
            }
            else
            {
                upper = new Vec2(upper.X, upper.Y + displacement.Y * AabbMultiplier);
            }

            return new Aabb(lower, upper);
        }
    }
}
