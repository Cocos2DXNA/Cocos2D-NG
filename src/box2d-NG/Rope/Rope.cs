using System;

namespace Box2DNG
{
    public sealed class Rope
    {
        private int _count;
        private Vec2[] _ps = Array.Empty<Vec2>();
        private Vec2[] _p0s = Array.Empty<Vec2>();
        private Vec2[] _vs = Array.Empty<Vec2>();
        private float[] _ims = Array.Empty<float>();
        private float[] _lengths = Array.Empty<float>();
        private float[] _angles = Array.Empty<float>();
        private Vec2 _gravity;
        private float _damping;
        private float _k2 = 1f;
        private float _k3 = 0.1f;

        public int Count => _count;
        public Vec2[] Positions => _ps;

        public void Initialize(RopeDef def)
        {
            if (def.Vertices == null || def.Masses == null)
            {
                throw new ArgumentNullException(nameof(def.Vertices));
            }
            if (def.Count < 2 || def.Vertices.Length < def.Count || def.Masses.Length < def.Count)
            {
                throw new ArgumentException("Invalid rope definition.");
            }

            _count = def.Count;
            _ps = new Vec2[_count];
            _p0s = new Vec2[_count];
            _vs = new Vec2[_count];
            _ims = new float[_count];

            for (int i = 0; i < _count; ++i)
            {
                _ps[i] = def.Vertices[i];
                _p0s[i] = def.Vertices[i];
                _vs[i] = Vec2.Zero;
                float m = def.Masses[i];
                _ims[i] = m > 0f ? 1f / m : 0f;
            }

            int count2 = _count - 1;
            int count3 = _count - 2;
            _lengths = new float[count2];
            _angles = new float[count3];

            for (int i = 0; i < count2; ++i)
            {
                _lengths[i] = (_ps[i + 1] - _ps[i]).Length;
            }

            for (int i = 0; i < count3; ++i)
            {
                Vec2 d1 = _ps[i + 1] - _ps[i];
                Vec2 d2 = _ps[i + 2] - _ps[i + 1];
                float a = Vec2.Cross(d1, d2);
                float b = Vec2.Dot(d1, d2);
                _angles[i] = MathFng.Atan2(a, b);
            }

            _gravity = def.Gravity;
            _damping = def.Damping;
            _k2 = def.K2;
            _k3 = def.K3;
        }

        public void Step(float h, int iterations)
        {
            if (h <= 0f || _count == 0)
            {
                return;
            }

            float d = MathF.Exp(-h * _damping);
            for (int i = 0; i < _count; ++i)
            {
                _p0s[i] = _ps[i];
                if (_ims[i] > 0f)
                {
                    _vs[i] += h * _gravity;
                }
                _vs[i] *= d;
                _ps[i] += h * _vs[i];
            }

            for (int i = 0; i < iterations; ++i)
            {
                SolveC2();
                SolveC3();
                SolveC2();
            }

            float invH = 1f / h;
            for (int i = 0; i < _count; ++i)
            {
                _vs[i] = invH * (_ps[i] - _p0s[i]);
            }
        }

        public void SetAngle(float angle)
        {
            for (int i = 0; i < _angles.Length; ++i)
            {
                _angles[i] = angle;
            }
        }

        private void SolveC2()
        {
            int count2 = _count - 1;
            for (int i = 0; i < count2; ++i)
            {
                Vec2 p1 = _ps[i];
                Vec2 p2 = _ps[i + 1];
                Vec2 d = p2 - p1;
                float length = d.Length;
                if (length > Constants.Epsilon)
                {
                    d = d / length;
                }

                float im1 = _ims[i];
                float im2 = _ims[i + 1];
                if (im1 + im2 == 0f)
                {
                    continue;
                }

                float s1 = im1 / (im1 + im2);
                float s2 = im2 / (im1 + im2);

                float C = _lengths[i] - length;
                p1 -= _k2 * s1 * C * d;
                p2 += _k2 * s2 * C * d;

                _ps[i] = p1;
                _ps[i + 1] = p2;
            }
        }

        private void SolveC3()
        {
            int count3 = _count - 2;
            for (int i = 0; i < count3; ++i)
            {
                Vec2 p1 = _ps[i];
                Vec2 p2 = _ps[i + 1];
                Vec2 p3 = _ps[i + 2];

                float m1 = _ims[i];
                float m2 = _ims[i + 1];
                float m3 = _ims[i + 2];

                Vec2 d1 = p2 - p1;
                Vec2 d2 = p3 - p2;
                float l1sqr = d1.LengthSquared;
                float l2sqr = d2.LengthSquared;
                if (l1sqr * l2sqr == 0f)
                {
                    continue;
                }

                float a = Vec2.Cross(d1, d2);
                float b = Vec2.Dot(d1, d2);
                float angle = MathFng.Atan2(a, b);

                Vec2 Jd1 = (-1f / l1sqr) * MathFng.LeftPerp(d1);
                Vec2 Jd2 = (1f / l2sqr) * MathFng.LeftPerp(d2);

                Vec2 J1 = -Jd1;
                Vec2 J2 = Jd1 - Jd2;
                Vec2 J3 = Jd2;

                float mass = m1 * Vec2.Dot(J1, J1) + m2 * Vec2.Dot(J2, J2) + m3 * Vec2.Dot(J3, J3);
                if (mass == 0f)
                {
                    continue;
                }

                mass = 1f / mass;
                float C = angle - _angles[i];
                while (C > MathF.PI)
                {
                    angle -= 2f * MathF.PI;
                    C = angle - _angles[i];
                }
                while (C < -MathF.PI)
                {
                    angle += 2f * MathF.PI;
                    C = angle - _angles[i];
                }

                float impulse = -_k3 * mass * C;
                p1 += (m1 * impulse) * J1;
                p2 += (m2 * impulse) * J2;
                p3 += (m3 * impulse) * J3;

                _ps[i] = p1;
                _ps[i + 1] = p2;
                _ps[i + 2] = p3;
            }
        }
    }
}
