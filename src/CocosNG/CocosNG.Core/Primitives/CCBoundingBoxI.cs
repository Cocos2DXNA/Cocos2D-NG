using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CocosNG.Core;

namespace CocosNG.Core.Primitives
{
    public struct CCBoundingBoxI
    {
        public static readonly CCBoundingBoxI Zero = new CCBoundingBoxI(0, 0, 0, 0);
        public static readonly CCBoundingBoxI Null = new CCBoundingBoxI(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;

        public CCBoundingBoxI(int minx, int miny, int maxx, int maxy)
        {
            MinX = minx;
            MinY = miny;
            MaxX = maxx;
            MaxY = maxy;
        }

        public CCSizeI Size
        {
            get { return new CCSizeI(MaxX - MinX, MaxY - MinY); }
        }

        public void ExpandToCircle(int x, int y, int radius)
        {
            Debug.Assert(radius >= 0);

            MinX = Math.Min(MinX, x - radius);
            MinY = Math.Min(MinY, y - radius);
            MaxX = Math.Max(MaxX, x + radius);
            MaxY = Math.Max(MaxY, y + radius);
        }

        public void ExpandToCircle(ref CCPointI point, int radius)
        {
            ExpandToCircle(point.X, point.Y, radius);
        }

        public void ExpandToPoint(int x, int y)
        {
            MinX = Math.Min(MinX, x);
            MinY = Math.Min(MinY, y);
            MaxX = Math.Max(MaxX, x);
            MaxY = Math.Max(MaxY, y);
        }

        public void ExpandToPoint(ref CCPointI point)
        {
            ExpandToPoint(point.X, point.Y);
        }

        public void ExpandToRect(ref CCBoundingBoxI r)
        {
            MinX = Math.Min(MinX, r.MinX);
            MinY = Math.Min(MinY, r.MinY);
            MaxX = Math.Max(MaxX, r.MaxX);
            MaxY = Math.Max(MaxY, r.MaxY);
        }

        public bool ContainsPoint(int x, int y)
        {
            return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
        }

        public bool Intersects(ref CCBoundingBoxI rect)
        {
            return !(MaxX < rect.MinX || rect.MaxX < MinX || MaxY < rect.MinY || rect.MaxY < MinY);
        }

        public void SetLerp(CCBoundingBoxI a, CCBoundingBoxI b, float ratio)
        {
            MinX = CCMathHelper.Lerp(a.MinX, b.MinX, ratio);
            MinY = CCMathHelper.Lerp(a.MinY, b.MinY, ratio);
            MaxX = CCMathHelper.Lerp(a.MaxX, b.MaxX, ratio);
            MaxY = CCMathHelper.Lerp(a.MaxY, b.MaxY, ratio);
        }

        public CCBoundingBoxI Transform(ref CCAffineTransform matrix)
        {
            var top = MinY;
            var left = MinX;
            var right = MaxX;
            var bottom = MaxY;

            var topLeft = new CCPointI(left, top);
            var topRight = new CCPointI(right, top);
            var bottomLeft = new CCPointI(left, bottom);
            var bottomRight = new CCPointI(right, bottom);

            matrix.Transform(ref topLeft.X, ref topLeft.Y);
            matrix.Transform(ref topRight.X, ref topRight.Y);
            matrix.Transform(ref bottomLeft.X, ref bottomLeft.Y);
            matrix.Transform(ref bottomRight.X, ref bottomRight.Y);

            int minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
            int maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
            int minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
            int maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

            return new CCBoundingBoxI(minX, minY, maxX, maxY);
        }

        public static implicit operator CCRect(CCBoundingBoxI box)
        {
            return new CCRect(box.MinX, box.MinY, box.MaxX - box.MinX, box.MaxY - box.MinY);
        }

        public static bool operator ==(CCBoundingBoxI b1, CCBoundingBoxI b2)
        {
            return b1.MinX == b2.MinX && b1.MaxX == b2.MaxX && b1.MinY == b2.MinY && b1.MaxY == b2.MaxY;
        }

        public static bool operator !=(CCBoundingBoxI b1, CCBoundingBoxI b2)
        {
            return b1.MinX != b2.MinX || b1.MaxX != b2.MaxX || b1.MinY != b2.MinY || b1.MaxY != b2.MaxY;
        }
    }
}
