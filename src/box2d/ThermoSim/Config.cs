namespace ThermoSim
{
    public sealed class RoomConfig
    {
        public float LengthFt { get; set; } = 6.0f;
        public float WidthFt { get; set; } = 5.0f;
        public float HeightFt { get; set; } = 5.0f;

        public float CellSizeFt { get; set; } = 0.25f;
        public float TimeStepSeconds { get; set; } = 0.1f;
        public int Steps { get; set; } = 2000;
        public int ReportEvery { get; set; } = 100;

        public float InitialTempF { get; set; } = 80.0f;
        public float SetPointTempF { get; set; } = 72.0f;
        public float SupplyTempF { get; set; } = 55.0f;

        // Outside temperature duty-cycle profile (square wave).
        public float OutsideTempLowF { get; set; } = 70.0f;
        public float OutsideTempHighF { get; set; } = 95.0f;
        public float OutsideTempPeriodSeconds { get; set; } = 3600.0f;
        public float OutsideTempDutyCycle { get; set; } = 0.5f;

        // Heat exchange with outside through walls (BTU/hr/ft^2/F).
        public float WallUABtuPerHourFt2F { get; set; } = 1.2f;

        // Approximate volumetric heat capacity of air at room conditions.
        public float AirHeatCapacityBtuPerFt3F { get; set; } = 0.018f;
        public float EquipmentHeatCapacityBtuPerF { get; set; } = 130.0f;

        // Effective thermal diffusion (ft^2/s). Tuned for stability with this grid.
        public float Diffusion { get; set; } = 0.02f;

        // Rack heat load per rack (BTU/hr).
        public float RackHeatLoadBtuPerHour { get; set; } = 6638.0f;

        // TEC fan info.
        public int FansPerTec { get; set; } = 6;
        public float FanCfm { get; set; } = 120.0f;
        public float TecCapacityBtuPerHour { get; set; } = 4545.0f;

        // TEC geometry in inches.
        public float TecDepthIn { get; set; } = 20.0f;
        public float TecHeightIn { get; set; } = 37.0f;
        public float TecWidthIn { get; set; } = 45.08f;

        // Rack exhaust guidance (ft/s).
        public float RackExhaustVelocityFtPerSec { get; set; } = 1.5f;
    }
}
