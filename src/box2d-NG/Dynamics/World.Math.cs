using System;

namespace Box2DNG
{
    public sealed partial class World
    {
        private static Vec2 Solve22(Mat22 A, Vec2 b)
        {
            float det = A.Ex.X * A.Ey.Y - A.Ey.X * A.Ex.Y;
            if (det != 0.0f)
            {
                det = 1.0f / det;
            }
            return new Vec2(det * (A.Ey.Y * b.X - A.Ey.X * b.Y), det * (A.Ex.X * b.Y - A.Ex.Y * b.X));
        }

        private static Vec3 Solve33(Mat33 A, Vec3 b)
        {
            float det = Vec3.Dot(A.Ex, Vec3.Cross(A.Ey, A.Ez));
            if (det != 0.0f)
            {
                det = 1.0f / det;
            }
            return new Vec3(
                det * Vec3.Dot(b, Vec3.Cross(A.Ey, A.Ez)),
                det * Vec3.Dot(A.Ex, Vec3.Cross(b, A.Ez)),
                det * Vec3.Dot(A.Ex, Vec3.Cross(A.Ey, b)));
        }
    }
}
