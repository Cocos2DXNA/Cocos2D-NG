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

            Geometry geometry = BuildGeometry(config);
            Simulation sim = new Simulation(config, geometry);

            AddRacksAndHeat(config, sim);
            AddTecOutlets(config, sim);
            AddRackExhaustFlow(config, sim);

            HeatmapWriter heatmap = new HeatmapWriter(
                outputDir: "ThermoSim/output",
                minTempF: config.SetPointTempF - 10.0f,
                maxTempF: config.SetPointTempF + 30.0f);
            Console.WriteLine("Heatmaps: ThermoSim/output/heatmap_*.ppm");

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
                }
            }

            Console.WriteLine("Done.");
        }

        private static Geometry BuildGeometry(RoomConfig config)
        {
            Geometry geometry = new Geometry();

            // Racks are solids.
            const float rackWidthFt = 2.0f;
            const float rackDepthFt = 3.5f; // 42 in

            float rackCenterY = config.WidthFt * 0.5f;
            float rackHalfDepth = rackDepthFt * 0.5f;
            float rackHalfWidth = rackWidthFt * 0.5f;

            float[] rackCentersX = { 1.0f, 3.0f, 5.0f };
            foreach (float cx in rackCentersX)
            {
                geometry.AddBox(cx, rackCenterY, rackHalfWidth, rackHalfDepth);
            }

            return geometry;
        }

        private static void AddRacksAndHeat(RoomConfig config, Simulation sim)
        {
            const float rackWidthFt = 2.0f;
            const float rackDepthFt = 3.5f;

            float rackCenterY = config.WidthFt * 0.5f;
            float rackHalfDepth = rackDepthFt * 0.5f;
            float rackHalfWidth = rackWidthFt * 0.5f;

            float[] rackCentersX = { 1.0f, 3.0f, 5.0f };
            foreach (float cx in rackCentersX)
            {
                AddHeatBox(config, sim, cx, rackCenterY, rackHalfWidth, rackHalfDepth, config.RackHeatLoadBtuPerHour);
            }
        }

        private static void AddTecOutlets(RoomConfig config, Simulation sim)
        {
            float tecWidthFt = Units.InchesToFeet(config.TecWidthIn);
            float tecDepthFt = Units.InchesToFeet(config.TecDepthIn);
            float tecHeightFt = Units.InchesToFeet(config.TecHeightIn);

            float flowPerTecCfs = Units.CfmToCfs(config.FanCfm * config.FansPerTec);
            float outletVelocity = flowPerTecCfs / (tecWidthFt * tecHeightFt);
            float tecCapacityBtuPerSecond = Units.BtuPerHourToBtuPerSecond(config.TecCapacityBtuPerHour);

            // Wall at y = 0: one TEC centered.
            AddTecWithSideBlow(config, sim,
                centerX: config.LengthFt * 0.5f,
                minY: 0.0f,
                widthFt: tecWidthFt,
                depthFt: tecDepthFt,
                heightFt: tecHeightFt,
                flowCfs: flowPerTecCfs,
                supplyTempF: config.SupplyTempF,
                setPointF: config.SetPointTempF,
                capacityBtuPerSecond: tecCapacityBtuPerSecond,
                velMagnitude: outletVelocity);

            // Wall at y = Width: two TECs, left/right.
            float offset = tecWidthFt * 0.6f;
            float leftX = config.LengthFt * 0.5f - offset;
            float rightX = config.LengthFt * 0.5f + offset;

            AddTecWithSideBlow(config, sim,
                centerX: leftX,
                minY: config.WidthFt - tecDepthFt,
                widthFt: tecWidthFt,
                depthFt: tecDepthFt,
                heightFt: tecHeightFt,
                flowCfs: flowPerTecCfs,
                supplyTempF: config.SupplyTempF,
                setPointF: config.SetPointTempF,
                capacityBtuPerSecond: tecCapacityBtuPerSecond,
                velMagnitude: outletVelocity);

            AddTecWithSideBlow(config, sim,
                centerX: rightX,
                minY: config.WidthFt - tecDepthFt,
                widthFt: tecWidthFt,
                depthFt: tecDepthFt,
                heightFt: tecHeightFt,
                flowCfs: flowPerTecCfs,
                supplyTempF: config.SupplyTempF,
                setPointF: config.SetPointTempF,
                capacityBtuPerSecond: tecCapacityBtuPerSecond,
                velMagnitude: outletVelocity);
        }

        private static void AddOutlet(
            RoomConfig config,
            Simulation sim,
            float centerX,
            float minY,
            float widthFt,
            float depthFt,
            float heightFt,
            float flowCfs,
            float supplyTempF,
            float setPointF,
            float capacityBtuPerSecond,
            float velX,
            float velY)
        {
            float halfWidth = widthFt * 0.5f;
            float minX = centerX - halfWidth;
            float maxX = centerX + halfWidth;
            float maxY = minY + depthFt;

            int minXi = (int)Math.Floor(minX / config.CellSizeFt);
            int maxXi = (int)Math.Floor(maxX / config.CellSizeFt);
            int minYi = (int)Math.Floor(minY / config.CellSizeFt);
            int maxYi = (int)Math.Floor(maxY / config.CellSizeFt);

            Simulation.OutletRegion outlet = new Simulation.OutletRegion
            {
                MinX = Clamp(minXi, 0, sim.Nx - 1),
                MaxX = Clamp(maxXi, 0, sim.Nx - 1),
                MinY = Clamp(minYi, 0, sim.Ny - 1),
                MaxY = Clamp(maxYi, 0, sim.Ny - 1),
                WidthFt = widthFt,
                DepthFt = depthFt,
                HeightFt = heightFt,
                FlowCfs = flowCfs,
                SupplyTempF = supplyTempF,
                SetPointF = setPointF,
                CapacityBtuPerSecond = capacityBtuPerSecond,
                VelX = velX,
                VelY = velY
            };

            sim.AddOutlet(outlet);
        }

        private static void AddTecWithSideBlow(
            RoomConfig config,
            Simulation sim,
            float centerX,
            float minY,
            float widthFt,
            float depthFt,
            float heightFt,
            float flowCfs,
            float supplyTempF,
            float setPointF,
            float capacityBtuPerSecond,
            float velMagnitude)
        {
            // Split width into left outlet / intake / right outlet.
            float third = widthFt / 3.0f;
            float leftCenter = centerX - third;
            float rightCenter = centerX + third;
            float velY = 0.0f;

            AddOutlet(config, sim,
                centerX: leftCenter,
                minY: minY,
                widthFt: third,
                depthFt: depthFt,
                heightFt: heightFt,
                flowCfs: flowCfs * 0.5f,
                supplyTempF: supplyTempF,
                setPointF: setPointF,
                capacityBtuPerSecond: capacityBtuPerSecond * 0.5f,
                velX: -velMagnitude,
                velY: velY);

            AddOutlet(config, sim,
                centerX: rightCenter,
                minY: minY,
                widthFt: third,
                depthFt: depthFt,
                heightFt: heightFt,
                flowCfs: flowCfs * 0.5f,
                supplyTempF: supplyTempF,
                setPointF: setPointF,
                capacityBtuPerSecond: capacityBtuPerSecond * 0.5f,
                velX: velMagnitude,
                velY: velY);
        }

        private static void AddHeatBox(
            RoomConfig config,
            Simulation sim,
            float centerX,
            float centerY,
            float halfWidth,
            float halfDepth,
            float heatLoadBtuPerHour)
        {
            float minX = centerX - halfWidth;
            float maxX = centerX + halfWidth;
            float minY = centerY - halfDepth;
            float maxY = centerY + halfDepth;

            int minXi = (int)Math.Floor(minX / config.CellSizeFt);
            int maxXi = (int)Math.Floor(maxX / config.CellSizeFt);
            int minYi = (int)Math.Floor(minY / config.CellSizeFt);
            int maxYi = (int)Math.Floor(maxY / config.CellSizeFt);

            Simulation.HeatSource source = new Simulation.HeatSource
            {
                MinX = Clamp(minXi, 0, sim.Nx - 1),
                MaxX = Clamp(maxXi, 0, sim.Nx - 1),
                MinY = Clamp(minYi, 0, sim.Ny - 1),
                MaxY = Clamp(maxYi, 0, sim.Ny - 1),
                BtuPerSecond = Units.BtuPerHourToBtuPerSecond(heatLoadBtuPerHour)
            };

            sim.AddHeatSource(source);
        }

        private static void AddRackExhaustFlow(RoomConfig config, Simulation sim)
        {
            const float rackWidthFt = 2.0f;
            const float rackDepthFt = 3.5f;

            float rackCenterY = config.WidthFt * 0.5f;
            float rackHalfDepth = rackDepthFt * 0.5f;
            float rackHalfWidth = rackWidthFt * 0.5f;

            float[] rackCentersX = { 1.0f, 3.0f, 5.0f };
            foreach (float cx in rackCentersX)
            {
                float exhaustY = rackCenterY + rackHalfDepth;
                float minY = exhaustY - config.CellSizeFt * 0.5f;
                float maxY = exhaustY + config.CellSizeFt * 0.5f;
                float minX = cx - rackHalfWidth;
                float maxX = cx + rackHalfWidth;

                int minXi = (int)Math.Floor(minX / config.CellSizeFt);
                int maxXi = (int)Math.Floor(maxX / config.CellSizeFt);
                int minYi = (int)Math.Floor(minY / config.CellSizeFt);
                int maxYi = (int)Math.Floor(maxY / config.CellSizeFt);

                Simulation.FlowRegion flow = new Simulation.FlowRegion
                {
                    MinX = Clamp(minXi, 0, sim.Nx - 1),
                    MaxX = Clamp(maxXi, 0, sim.Nx - 1),
                    MinY = Clamp(minYi, 0, sim.Ny - 1),
                    MaxY = Clamp(maxYi, 0, sim.Ny - 1),
                    VelX = 0.0f,
                    VelY = config.RackExhaustVelocityFtPerSec
                };

                sim.AddFlow(flow);
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
