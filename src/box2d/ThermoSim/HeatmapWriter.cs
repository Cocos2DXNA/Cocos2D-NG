using System;
using System.IO;

namespace ThermoSim
{
    public sealed class HeatmapWriter
    {
        private readonly string _outputDir;
        private readonly float _minTempF;
        private readonly float _maxTempF;

        public HeatmapWriter(string outputDir, float minTempF, float maxTempF)
        {
            _outputDir = outputDir;
            _minTempF = minTempF;
            _maxTempF = maxTempF;
            Directory.CreateDirectory(_outputDir);
        }

        public void WritePpm(Simulation sim, int step)
        {
            string path = Path.Combine(_outputDir, $"heatmap_{step:D5}.ppm");
            int width = sim.Nx;
            int height = sim.Ny;

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.WriteLine("P3");
                writer.WriteLine($"{width} {height}");
                writer.WriteLine("255");

                for (int y = height - 1; y >= 0; --y)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        if (sim.IsSolidCell(x, y))
                        {
                            writer.Write("0 0 0 ");
                            continue;
                        }

                        float t = sim.GetTemperature(x, y);
                        float normalized = (_maxTempF - _minTempF) > 0.01f
                            ? (t - _minTempF) / (_maxTempF - _minTempF)
                            : 0.0f;
                        normalized = Math.Max(0.0f, Math.Min(1.0f, normalized));

                        (int r, int g, int b) = ColorMap(normalized);
                        writer.Write($"{r} {g} {b} ");
                    }
                    writer.WriteLine();
                }
            }
        }

        private static (int r, int g, int b) ColorMap(float t)
        {
            // Simple blue->cyan->yellow->red gradient.
            float r, g, b;
            if (t < 0.33f)
            {
                float k = t / 0.33f;
                r = 0.0f;
                g = k;
                b = 1.0f;
            }
            else if (t < 0.66f)
            {
                float k = (t - 0.33f) / 0.33f;
                r = k;
                g = 1.0f;
                b = 1.0f - k;
            }
            else
            {
                float k = (t - 0.66f) / 0.34f;
                r = 1.0f;
                g = 1.0f - k;
                b = 0.0f;
            }

            return ((int)(r * 255.0f), (int)(g * 255.0f), (int)(b * 255.0f));
        }
    }
}
