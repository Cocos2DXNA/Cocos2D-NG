using System;

namespace ThermoSim
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            RoomConfig config = new RoomConfig();
            config.EquipmentHeatCapacityBtuPerF = 130.0f;
            Console.WriteLine("ThermoSim: 3-rack IT closet (6x5x5 ft), top-down 2D with Box2D geometry.");
            Console.WriteLine("Cell size = {0:0.##} ft, dt = {1:0.###} s, steps = {2}", config.CellSizeFt, config.TimeStepSeconds, config.Steps);
            float volumeFt3 = config.LengthFt * config.WidthFt * config.HeightFt;
            float roomHeatCapacity = config.AirHeatCapacityBtuPerFt3F * volumeFt3 + config.EquipmentHeatCapacityBtuPerF;
            Console.WriteLine("Room heat capacity (air+equipment): {0:0.0} BTU/F", roomHeatCapacity);

            Geometry geometry = SceneBuilder.BuildGeometry(config);
            Simulation sim = new Simulation(config, geometry);

            SceneBuilder.AddRacksAndHeat(config, sim);
            SceneBuilder.AddTecOutlets(config, sim);
            SceneBuilder.AddRackFlows(config, sim);

            HeatmapWriter heatmap = new HeatmapWriter(
                outputDir: "ThermoSim/output",
                minTempF: config.HeatmapMinTempF,
                maxTempF: config.HeatmapMaxTempF);
            Console.WriteLine("Heatmaps: ThermoSim/output/heatmap_*.ppm and heatmap_*.png");

            using TimeSeriesWriter series = new TimeSeriesWriter("ThermoSim/output/thermo_timeseries.csv");
            Console.WriteLine("Time series: ThermoSim/output/thermo_timeseries.csv");

            for (int step = 0; step <= config.Steps; ++step)
            {
                sim.Step();

                if (step % config.ReportEvery == 0)
                {
                    var stats = sim.GetTemperatureStats();
                    Console.WriteLine("Step {0,5}: avg {1,6:0.0} F | min {2,6:0.0} F | max {3,6:0.0} F",
                        step, stats.avgF, stats.minF, stats.maxF);
                    Console.WriteLine("Outside: {0,6:0.0} F | SetPoint: {1,6:0.0} F", sim.CurrentOutsideTempF, config.SetPointTempF);
                    heatmap.WritePpm(sim, step);
                    heatmap.WritePng(sim, step);
                    series.Write(step, step * config.TimeStepSeconds, sim.CurrentOutsideTempF, config.SetPointTempF,
                        stats.avgF, stats.minF, stats.maxF, sim.LastCoolingBtuPerSecond);
                }
            }

            Console.WriteLine("Done.");
        }
    }
}
