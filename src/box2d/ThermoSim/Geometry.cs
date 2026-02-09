using System.Collections.Generic;
using System.Reflection.Emit;
using Box2D.Collision.Shapes;
using Box2D.Common;

namespace ThermoSim
{
    public sealed class Geometry
    {
        private readonly List<(b2PolygonShape Shape, b2Transform Xf, string Label)> _solids = new();

        public IEnumerable<(b2PolygonShape Shape, b2Transform Xf, string Label)> Solids => _solids;

        public void AddBox(float centerX, float centerY, float halfWidth, float halfHeight, string label = "box")
        {
            b2PolygonShape poly = new b2PolygonShape();
            poly.SetAsBox(halfWidth, halfHeight);

            b2Transform xf = b2Transform.Identity;
            xf.p = new b2Vec2(centerX, centerY);

            _solids.Add((poly, xf, label));
        }

        public bool IsSolid(b2Vec2 point)
        {
            foreach (var solid in _solids)
            {
                var xf = solid.Xf;
                if (solid.Shape.TestPoint(ref xf, point))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
