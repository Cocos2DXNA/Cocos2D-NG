using Microsoft.Xna.Framework;

namespace CocosNG.Core.Primitives
{
    public struct CCPoint
    {
        public float X;
        public float Y;

        public CCPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector2(CCPoint point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static implicit operator CCPoint(Vector2 vector)
        {
            return new CCPoint(vector.X, vector.Y);
        }
    }
}
