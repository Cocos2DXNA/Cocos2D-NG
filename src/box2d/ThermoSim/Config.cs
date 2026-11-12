namespace ThermoSim
{
    public sealed class RoomConfig
    {
        public float LengthFt { get; set; } = 6.0f;
        public float WidthFt { get; set; } = 5.0f;
        public float HeightFt { get; set; } = 5.0f;

        public float CellSizeFt { get; set; } = 0.5f/12.0f; // half inch
        public float TimeStepSeconds { get; set; } = 60.0f;
        public float MaxSubstepSeconds { get; set; } = 0.5f;
        public int Steps { get; set; } = 2000;
        public int ReportEvery { get; set; } = 100;

        public float InitialTempF { get; set; } = 69.0f;
        public float SetPointTempF { get; set; } = 93.2f;
        public float SupplyTempF { get; set; } = 55.0f;

        // Outside temperature profile (ocean mission ramp + diurnal).
        public float AmbientStartTempF { get; set; } = 69.0f;
        public float AmbientEndTempF { get; set; } = 84.0f;
        public float AmbientDiurnalAmplitudeF { get; set; } = 1.3f;
        public float AmbientDiurnalPeriodSeconds { get; set; } = 12.0f * 3600.0f;
        public float AmbientStartRh { get; set; } = 65.0f;
        public float AmbientEndRh { get; set; } = 86.0f;

        // Internal humidity model.
        public float InitialRh { get; set; } = 65.0f;
        public float LeakTimeConstantHours { get; set; } = 0.0f;
        public float OutgasPaPerHour { get; set; } = 0.0f;

        // Heat exchange with outside through walls (BTU/hr/ft^2/F).
        public float WallUABtuPerHourFt2F { get; set; } = 1.2f;

        // Approximate volumetric heat capacity of air at room conditions.
        public float AirHeatCapacityBtuPerFt3F { get; set; } = 0.018f;
        public float EquipmentHeatCapacityBtuPerF { get; set; } = 130.0f;

        // Effective thermal diffusion (ft^2/s). Tuned for stability with this grid.
        public float Diffusion { get; set; } = 0.02f;

        // Rack heat load per rack (BTU/hr).
        public float RackHeatLoadBtuPerHour { get; set; } = 13378.0f/3.0f;

        // Duty-cycle load profile for racks (square wave over a "tour").
        public float TourLengthSeconds { get; set; } = 24.0f * 3600.0f * 30.0f; // 30 days in seconds
        public float LoadPulseOnSeconds { get; set; } = 24.0f * 3600.0f * 7.0f; // 7 days in seconds
        public float LoadPulseOffSeconds { get; set; } = 24.0f * 3600.0f * 5.0f; // 5 days in seconds
        public float LoadOnFraction { get; set; } = 0.75f;
        public float LoadOffFraction { get; set; } = 0.20f;

        // TEC fan info.
        public int FansPerTec { get; set; } = 6;
        public float FanCfm { get; set; } = 120.0f;
        public float TecCapacityBtuPerHour { get; set; } = 4545.0f;

        // TEC geometry in inches.
        public float TecDepthIn { get; set; } = 4.94f; // inside depth only
        public float TecHeightIn { get; set; } = 35.08f;
        public float TecWidthIn { get; set; } = 20.52f;
        public float TecSeparation { get; set; } = 0.4f; // gap between TECs

        // Rack exhaust guidance (ft/s).
        public float RackExhaustVelocityFtPerSec { get; set; } = 1.5f;

        // Electronics-driven airflow at full load (ft/s).
        public float RackLoadFlowFtPerSec { get; set; } = 0.8f;

        // Viewer heatmap range.
        public float HeatmapMinTempF { get; set; } = 62.0f;
        public float HeatmapMaxTempF { get; set; } = 102.0f;

        // Viewer particle field.
        public float AirParticleSpacingFt { get; set; } = 0.3f;
        public float AirParticleForceScale { get; set; } = 6.0f;
    }
}
