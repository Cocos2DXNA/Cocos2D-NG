using System;

namespace ThermoSim
{
    public sealed class Field2D
    {
        private readonly float[] _data;
        private readonly int _width;
        private readonly int _height;

        public Field2D(int width, int height, float initialValue = 0.0f)
        {
            _width = width;
            _height = height;
            _data = new float[width * height];
            if (initialValue != 0.0f)
            {
                for (int i = 0; i < _data.Length; ++i)
                {
                    _data[i] = initialValue;
                }
            }
        }

        public int Width => _width;
        public int Height => _height;

        public float this[int x, int y]
        {
            get => _data[y * _width + x];
            set => _data[y * _width + x] = value;
        }

        public float SampleBilinear(float x, float y)
        {
            if (x < 0.0f) x = 0.0f;
            if (y < 0.0f) y = 0.0f;
            if (x > _width - 1) x = _width - 1;
            if (y > _height - 1) y = _height - 1;

            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);
            int x1 = Math.Min(x0 + 1, _width - 1);
            int y1 = Math.Min(y0 + 1, _height - 1);

            float fx = x - x0;
            float fy = y - y0;

            float v00 = this[x0, y0];
            float v10 = this[x1, y0];
            float v01 = this[x0, y1];
            float v11 = this[x1, y1];

            float vx0 = v00 + (v10 - v00) * fx;
            float vx1 = v01 + (v11 - v01) * fx;
            return vx0 + (vx1 - vx0) * fy;
        }
    }
}
