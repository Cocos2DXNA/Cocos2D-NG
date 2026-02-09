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
        private readonly List<FlowRegion> _loadFlows = new();
        private float _timeSeconds;
        private float _stepSeconds;
        private float _outsideTempF;
        private float _outsideRh;
        private float _internalRh;
        private double _waterMoles;
        private float _lastCoolingBtuPerSecond;
        private float _lastHeatLoadBtuPerSecond;
        private float _currentLoadFraction;
        private bool _loadCycleEnabled = true;

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
            InitializeHumidityState();
        }

        public int Nx => _nx;
        public int Ny => _ny;
        public float CellSizeFt => _dx;
        public float CurrentOutsideTempF => _outsideTempF;
        public float CurrentOutsideRh => _outsideRh;
        public float CurrentInternalRh => _internalRh;
        public float LastCoolingBtuPerSecond => _lastCoolingBtuPerSecond;
        public float CurrentHeatLoadBtuPerSecond => _lastHeatLoadBtuPerSecond;
        public float CurrentLoadFraction => _currentLoadFraction;
        public float TimeSeconds => _timeSeconds;
        public IReadOnlyList<FlowRegion> Flows => _flows;
        public IReadOnlyList<FlowRegion> LoadFlows => _loadFlows;
        public IReadOnlyList<OutletRegion> Outlets => _outlets;
        public bool LoadCycleEnabled
        {
            get => _loadCycleEnabled;
            set => _loadCycleEnabled = value;
        }
        public float HeatmapMinTempF => _config.HeatmapMinTempF;
        public float HeatmapMaxTempF => _config.HeatmapMaxTempF;
        public float SetPointTempF
        {
            get => _config.SetPointTempF;
            set
            {
                _config.SetPointTempF = value;
                for (int i = 0; i < _outlets.Count; ++i)
                {
                    OutletRegion outlet = _outlets[i];
                    outlet.SetPointF = value;
                    _outlets[i] = outlet;
                }
            }
        }

        public void AddOutlet(OutletRegion outlet)
        {
            _outlets.Add(outlet);
        }

        public void AddFlow(FlowRegion flow)
        {
            _flows.Add(flow);
        }

        public void AddLoadFlow(FlowRegion flow)
        {
            _loadFlows.Add(flow);
        }

        public void AddHeatSource(HeatSource source)
        {
            _heatSources.Add(source);
        }

        public void Step()
        {
            float totalDt = _config.TimeStepSeconds;
            float maxSub = Math.Max(0.01f, _config.MaxSubstepSeconds);
            int substeps = (int)Math.Max(1, Math.Ceiling(totalDt / maxSub));
            float subDt = totalDt / substeps;

            for (int i = 0; i < substeps; ++i)
            {
                _stepSeconds = subDt;
                _outsideTempF = ComputeOutsideTempF(_timeSeconds);
                _outsideRh = ComputeOutsideRh(_timeSeconds);
                _currentLoadFraction = _loadCycleEnabled ? GetLoadMultiplier(_timeSeconds) : 0.0f;
                _lastCoolingBtuPerSecond = 0.0f;
                UpdateVelocityField();
                AdvectTemperature();
                DiffuseTemperature();
                ApplyHeatSources();
                ApplyOutlets();
                ApplyWallHeatExchange();
                SwapBuffers();
                UpdateHumidityState();
                _timeSeconds += _stepSeconds;
            }
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
                    if (float.IsNaN(t) || float.IsInfinity(t))
                    {
                        continue;
                    }
                    if (count == 0)
                    {
                        min = t;
                        max = t;
                    }
                    else
                    {
                        if (t < min) min = t;
                        if (t > max) max = t;
                    }
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

            if (_loadFlows.Count > 0 && _currentLoadFraction > 0.0f)
            {
                foreach (var flow in _loadFlows)
                {
                    for (int y = flow.MinY; y <= flow.MaxY; ++y)
                    {
                        for (int x = flow.MinX; x <= flow.MaxX; ++x)
                        {
                            if (IsSolid(x, y))
                            {
                                continue;
                            }
                            _velX[x, y] += flow.VelX * _currentLoadFraction;
                            _velY[x, y] += flow.VelY * _currentLoadFraction;
                        }
                    }
                }
            }
        }

        private void AdvectTemperature()
        {
            float dt = _stepSeconds;
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

            float dt = _stepSeconds;
            float coeff = k * dt / (_dx * _dx);
            // Explicit diffusion is only stable for small coefficients.
            if (coeff > 0.25f)
            {
                coeff = 0.25f;
            }

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
                _lastHeatLoadBtuPerSecond = 0.0f;
                return;
            }

            float dt = _stepSeconds;
            float cellVolume = _dx * _dx * _config.HeightFt;
            float capacity = GetCellHeatCapacity(cellVolume);
            float loadMultiplier = _currentLoadFraction;
            float baseTotal = 0.0f;
            foreach (var source in _heatSources)
            {
                baseTotal += source.BtuPerSecond;
            }
            _lastHeatLoadBtuPerSecond = baseTotal * loadMultiplier;

            foreach (var source in _heatSources)
            {
                float btuPerSecond = source.BtuPerSecond * loadMultiplier;
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

            float dt = _stepSeconds;
            float totalCoolingBtu = 0.0f;

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
                            coolingBtu = maxCoolingBtu;
                        }
                        else if (coolingBtu < 0.0f)
                        {
                            coolingBtu = 0.0f;
                        }

                        _nextTemperature[x, y] = t + targetDelta;
                        totalCoolingBtu += coolingBtu;
                    }
                }
            }

            _lastCoolingBtuPerSecond = totalCoolingBtu / dt;
        }

        private void ApplyWallHeatExchange()
        {
            float dt = _stepSeconds;
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
            float mission = Math.Max(1.0f, _config.TourLengthSeconds);
            float t = timeSeconds;
            if (t > mission)
            {
                t = mission;
            }

            float rampAlpha = t / mission;
            float baseTemp = _config.AmbientStartTempF +
                             (_config.AmbientEndTempF - _config.AmbientStartTempF) * rampAlpha;

            float period = Math.Max(1.0f, _config.AmbientDiurnalPeriodSeconds);
            float phase = (float)(2.0 * Math.PI * (timeSeconds / period));
            float diurnal = _config.AmbientDiurnalAmplitudeF * (float)Math.Sin(phase);

            return baseTemp + diurnal;
        }

        private float ComputeOutsideRh(float timeSeconds)
        {
            float mission = Math.Max(1.0f, _config.TourLengthSeconds);
            float t = timeSeconds;
            if (t > mission)
            {
                t = mission;
            }

            float rampAlpha = t / mission;
            float baseRh = _config.AmbientStartRh +
                           (_config.AmbientEndRh - _config.AmbientStartRh) * rampAlpha;
            return Clamp(baseRh, 0.0f, 100.0f);
        }

        private void InitializeHumidityState()
        {
            float tempC = FToC(_config.InitialTempF);
            float tempK = tempC + 273.15f;
            float es0Pa = HpaToPa(SaturationVaporPressureHpa(tempC));
            float rh = Clamp(_config.InitialRh, 0.0f, 100.0f);
            float pw0Pa = (rh / 100.0f) * es0Pa;

            const double R = 8.314462618;
            const double V = 1.0;
            _waterMoles = pw0Pa * V / (R * tempK);
            _internalRh = rh;
        }

        private void UpdateHumidityState()
        {
            float tempF = ComputeAverageTempF();
            float tempC = FToC(tempF);
            float tempK = tempC + 273.15f;
            float dtHours = _stepSeconds / Units.SecondsPerHour;

            const double R = 8.314462618;
            const double V = 1.0;

            float esAmbPa = HpaToPa(SaturationVaporPressureHpa(FToC(_outsideTempF)));
            float pwAmbPa = (Clamp(_outsideRh, 0.0f, 100.0f) / 100.0f) * esAmbPa;

            double pwPa = _waterMoles * R * tempK / V;

            if (_config.OutgasPaPerHour != 0.0f && dtHours > 0.0f)
            {
                pwPa += _config.OutgasPaPerHour * dtHours;
            }

            float tauLeak = _config.LeakTimeConstantHours;
            if (tauLeak > 0.0f && dtHours > 0.0f)
            {
                double alpha = dtHours / tauLeak;
                if (alpha < 0.0) alpha = 0.0;
                if (alpha > 1.0) alpha = 1.0;
                pwPa = (1.0 - alpha) * pwPa + alpha * pwAmbPa;
            }

            float esIntPa = HpaToPa(SaturationVaporPressureHpa(tempC));
            if (pwPa > esIntPa)
            {
                pwPa = esIntPa;
            }

            _waterMoles = pwPa * V / (R * tempK);
            _internalRh = esIntPa > 0.0f ? (float)(100.0 * pwPa / esIntPa) : 0.0f;
        }

        private float ComputeAverageTempF()
        {
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
                    if (float.IsNaN(t) || float.IsInfinity(t))
                    {
                        continue;
                    }
                    sum += t;
                    count++;
                }
            }

            if (count == 0)
            {
                return _config.InitialTempF;
            }

            return sum / count;
        }

        private static float SaturationVaporPressureHpa(float tempC)
        {
            return 6.112f * (float)Math.Exp((17.67f * tempC) / (tempC + 243.5f));
        }

        private static float HpaToPa(float hPa)
        {
            return hPa * 100.0f;
        }

        private static float FToC(float tempF)
        {
            return (tempF - 32.0f) * (5.0f / 9.0f);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private float GetLoadMultiplier(float timeSeconds)
        {
            float on = Math.Max(0.0f, _config.LoadPulseOnSeconds);
            float off = Math.Max(0.0f, _config.LoadPulseOffSeconds);
            float period = Math.Max(1.0f, on + off);
            float tour = Math.Max(1.0f, _config.TourLengthSeconds);
            float t = timeSeconds % tour;
            float phase = t % period;
            bool isOn = phase < on;
            return isOn ? _config.LoadOnFraction : _config.LoadOffFraction;
        }

        public float GetTemperature(int x, int y)
        {
            return _temperature[x, y];
        }

        public b2Vec2 GetVelocityAt(b2Vec2 worldPoint)
        {
            int xi = (int)Math.Floor(worldPoint.x / _dx);
            int yi = (int)Math.Floor(worldPoint.y / _dx);
            xi = Math.Max(0, Math.Min(_nx - 1, xi));
            yi = Math.Max(0, Math.Min(_ny - 1, yi));
            return new b2Vec2(_velX[xi, yi], _velY[xi, yi]);
        }

        public float GetTemperatureAt(b2Vec2 worldPoint)
        {
            float fx = worldPoint.x / _dx;
            float fy = worldPoint.y / _dx;
            return _temperature.SampleBilinear(fx, fy);
        }

        public float GetHeatmapNormalizedTempAt(b2Vec2 worldPoint)
        {
            float t = GetTemperatureAt(worldPoint);
            float denom = _config.HeatmapMaxTempF - _config.HeatmapMinTempF;
            if (denom <= 0.01f)
            {
                return 0.0f;
            }
            float normalized = (t - _config.HeatmapMinTempF) / denom;
            if (normalized < 0.0f) return 0.0f;
            if (normalized > 1.0f) return 1.0f;
            return normalized;
        }

        public bool IsSolidCell(int x, int y)
        {
            return IsSolid(x, y);
        }
    }
}
