using System;
using System.Collections.Generic;
using Box2D.Common;

namespace ThermoSim
{
    public sealed class Simulation
    {
        private readonly RoomConfig _config;
        private readonly Geometry _geometry;
        private readonly int _nx;
        private readonly int _ny;
        private readonly float _dx;
        private readonly Field2D _temperature;
        private readonly Field2D _nextTemperature;
        private readonly Field2D _velX;
        private readonly Field2D _velY;
        private readonly bool[] _solidMask;
        private readonly List<OutletRegion> _outlets = new();
        private readonly List<HeatSource> _heatSources = new();
        private readonly List<FlowRegion> _flows = new();
        private float _timeSeconds;
        private float _outsideTempF;

        public Simulation(RoomConfig config, Geometry geometry)
        {
            _config = config;
            _geometry = geometry;

            _dx = config.CellSizeFt;
            _nx = (int)Math.Ceiling(config.LengthFt / _dx);
            _ny = (int)Math.Ceiling(config.WidthFt / _dx);

            _temperature = new Field2D(_nx, _ny, config.InitialTempF);
            _nextTemperature = new Field2D(_nx, _ny, config.InitialTempF);
            _velX = new Field2D(_nx, _ny, 0.0f);
            _velY = new Field2D(_nx, _ny, 0.0f);
            _solidMask = new bool[_nx * _ny];

            BuildSolidMask();
        }

        public int Nx => _nx;
        public int Ny => _ny;
        public float CellSizeFt => _dx;
        public float CurrentOutsideTempF => _outsideTempF;

        public void AddOutlet(OutletRegion outlet)
        {
            _outlets.Add(outlet);
        }

        public void AddFlow(FlowRegion flow)
        {
            _flows.Add(flow);
        }

        public void AddHeatSource(HeatSource source)
        {
            _heatSources.Add(source);
        }

        public void Step()
        {
            _outsideTempF = ComputeOutsideTempF(_timeSeconds);
            UpdateVelocityField();
            AdvectTemperature();
            DiffuseTemperature();
            ApplyHeatSources();
            ApplyOutlets();
            ApplyWallHeatExchange();
            SwapBuffers();
            _timeSeconds += _config.TimeStepSeconds;
        }

        public (float minF, float maxF, float avgF) GetTemperatureStats()
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            float sum = 0.0f;
            int count = 0;

            for (int y = 0; y < _ny; ++y)
            {
                for (int x = 0; x < _nx; ++x)
                {
                    if (IsSolid(x, y))
                    {
                        continue;
                    }
                    float t = _temperature[x, y];
                    if (t < min) min = t;
                    if (t > max) max = t;
                    sum += t;
                    count++;
                }
            }

            if (count == 0)
            {
                return (0.0f, 0.0f, 0.0f);
            }

            return (min, max, sum / count);
        }

        private void BuildSolidMask()
        {
            for (int y = 0; y < _ny; ++y)
            {
                for (int x = 0; x < _nx; ++x)
                {
                    b2Vec2 p = CellCenter(x, y);
                    _solidMask[y * _nx + x] = _geometry.IsSolid(p);
                }
            }
        }

        private bool IsSolid(int x, int y)
        {
            return _solidMask[y * _nx + x];
        }

        private b2Vec2 CellCenter(int x, int y)
        {
            return new b2Vec2((x + 0.5f) * _dx, (y + 0.5f) * _dx);
        }

        private void UpdateVelocityField()
        {
            for (int y = 0; y < _ny; ++y)
            {
                for (int x = 0; x < _nx; ++x)
                {
                    _velX[x, y] = 0.0f;
                    _velY[x, y] = 0.0f;
                }
            }

            foreach (var outlet in _outlets)
            {
                for (int y = outlet.MinY; y <= outlet.MaxY; ++y)
                {
                    for (int x = outlet.MinX; x <= outlet.MaxX; ++x)
                    {
                        if (IsSolid(x, y))
                        {
                            continue;
                        }
                        _velX[x, y] += outlet.VelX;
                        _velY[x, y] += outlet.VelY;
                    }
                }
            }

            foreach (var flow in _flows)
            {
                for (int y = flow.MinY; y <= flow.MaxY; ++y)
                {
                    for (int x = flow.MinX; x <= flow.MaxX; ++x)
                    {
                        if (IsSolid(x, y))
                        {
                            continue;
                        }
                        _velX[x, y] += flow.VelX;
                        _velY[x, y] += flow.VelY;
                    }
                }
            }
        }

        private void AdvectTemperature()
        {
            float dt = _config.TimeStepSeconds;
            float invDx = 1.0f / _dx;

            for (int y = 0; y < _ny; ++y)
            {
                for (int x = 0; x < _nx; ++x)
                {
                    if (IsSolid(x, y))
                    {
                        _nextTemperature[x, y] = _temperature[x, y];
                        continue;
                    }

                    float vx = _velX[x, y];
                    float vy = _velY[x, y];

                    float backX = x - vx * dt * invDx;
                    float backY = y - vy * dt * invDx;

                    _nextTemperature[x, y] = _temperature.SampleBilinear(backX, backY);
                }
            }
        }

        private void DiffuseTemperature()
        {
            float k = _config.Diffusion;
            if (k <= 0.0f)
            {
                return;
            }

            float dt = _config.TimeStepSeconds;
            float coeff = k * dt / (_dx * _dx);

            for (int y = 0; y < _ny; ++y)
            {
                for (int x = 0; x < _nx; ++x)
                {
                    if (IsSolid(x, y))
                    {
                        continue;
                    }

                    float t = _nextTemperature[x, y];
                    float tL = _nextTemperature[Math.Max(x - 1, 0), y];
                    float tR = _nextTemperature[Math.Min(x + 1, _nx - 1), y];
                    float tD = _nextTemperature[x, Math.Max(y - 1, 0)];
                    float tU = _nextTemperature[x, Math.Min(y + 1, _ny - 1)];

                    float lap = tL + tR + tD + tU - 4.0f * t;
                    _nextTemperature[x, y] = t + coeff * lap;
                }
            }
        }

        private void ApplyHeatSources()
        {
            if (_heatSources.Count == 0)
            {
                return;
            }

            float dt = _config.TimeStepSeconds;
            float cellVolume = _dx * _dx * _config.HeightFt;
            float capacity = GetCellHeatCapacity(cellVolume);

            foreach (var source in _heatSources)
            {
                float btuPerSecond = source.BtuPerSecond;
                float deltaF = (btuPerSecond * dt) / capacity;

                for (int y = source.MinY; y <= source.MaxY; ++y)
                {
                    for (int x = source.MinX; x <= source.MaxX; ++x)
                    {
                        if (IsSolid(x, y))
                        {
                            continue;
                        }
                        _nextTemperature[x, y] += deltaF;
                    }
                }
            }
        }

        private void ApplyOutlets()
        {
            if (_outlets.Count == 0)
            {
                return;
            }

            float dt = _config.TimeStepSeconds;

            foreach (var outlet in _outlets)
            {
                float outletVolume = outlet.DepthFt * outlet.WidthFt * outlet.HeightFt;
                float mix = outlet.FlowCfs * dt / outletVolume;
                if (mix > 1.0f)
                {
                    mix = 1.0f;
                }

                for (int y = outlet.MinY; y <= outlet.MaxY; ++y)
                {
                    for (int x = outlet.MinX; x <= outlet.MaxX; ++x)
                    {
                        if (IsSolid(x, y))
                        {
                            continue;
                        }
                        float t = _nextTemperature[x, y];
                        if (t <= outlet.SetPointF)
                        {
                            continue;
                        }

                        float desiredDelta = outlet.SupplyTempF - t;
                        float targetDelta = mix * desiredDelta;

                        float cellVolume = _dx * _dx * _config.HeightFt;
                        float capacity = GetCellHeatCapacity(cellVolume);
                        float coolingBtu = -targetDelta * capacity;
                        float maxCoolingBtu = outlet.CapacityBtuPerSecond * dt;
                        if (coolingBtu > maxCoolingBtu && coolingBtu > 0.0f)
                        {
                            targetDelta = -(maxCoolingBtu / capacity);
                        }

                        _nextTemperature[x, y] = t + targetDelta;
                    }
                }
            }
        }

        private void ApplyWallHeatExchange()
        {
            float dt = _config.TimeStepSeconds;
            float ua = _config.WallUABtuPerHourFt2F / Units.SecondsPerHour;

            for (int y = 0; y < _ny; ++y)
            {
                for (int x = 0; x < _nx; ++x)
                {
                    if (IsSolid(x, y))
                    {
                        continue;
                    }

                    bool boundary = x == 0 || y == 0 || x == _nx - 1 || y == _ny - 1;
                    if (!boundary)
                    {
                        continue;
                    }

                    float t = _nextTemperature[x, y];
                    float delta = _outsideTempF - t;
                    float area = _dx * _config.HeightFt;
                    float heatBtu = ua * area * delta * dt;

                    float cellVolume = _dx * _dx * _config.HeightFt;
                    float capacity = GetCellHeatCapacity(cellVolume);
                    float deltaF = heatBtu / capacity;
                    _nextTemperature[x, y] = t + deltaF;
                }
            }
        }

        private float GetCellHeatCapacity(float cellVolume)
        {
            float airCapacity = _config.AirHeatCapacityBtuPerFt3F * cellVolume;
            float equipCapacity = _config.EquipmentHeatCapacityBtuPerF / (_config.LengthFt * _config.WidthFt * _config.HeightFt) * cellVolume;
            return airCapacity + equipCapacity;
        }

        private void SwapBuffers()
        {
            for (int y = 0; y < _ny; ++y)
            {
                for (int x = 0; x < _nx; ++x)
                {
                    _temperature[x, y] = _nextTemperature[x, y];
                }
            }
        }

        public struct OutletRegion
        {
            public int MinX;
            public int MaxX;
            public int MinY;
            public int MaxY;
            public float WidthFt;
            public float DepthFt;
            public float HeightFt;
            public float FlowCfs;
            public float SupplyTempF;
            public float SetPointF;
            public float CapacityBtuPerSecond;
            public float VelX;
            public float VelY;
        }

        public struct FlowRegion
        {
            public int MinX;
            public int MaxX;
            public int MinY;
            public int MaxY;
            public float VelX;
            public float VelY;
        }

        public struct HeatSource
        {
            public int MinX;
            public int MaxX;
            public int MinY;
            public int MaxY;
            public float BtuPerSecond;
        }

        private float ComputeOutsideTempF(float timeSeconds)
        {
            float period = Math.Max(1.0f, _config.OutsideTempPeriodSeconds);
            float duty = Math.Max(0.0f, Math.Min(1.0f, _config.OutsideTempDutyCycle));
            float phase = timeSeconds % period;
            bool high = phase < duty * period;
            return high ? _config.OutsideTempHighF : _config.OutsideTempLowF;
        }

        public float GetTemperature(int x, int y)
        {
            return _temperature[x, y];
        }

        public bool IsSolidCell(int x, int y)
        {
            return IsSolid(x, y);
        }
    }
}
