using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using CocosNG.Core;

namespace CocosNG.Core.Primitives
{
    public struct CCRect
    {
        public static readonly CCRect Zero = new CCRect(0, 0, 0, 0);

        public CCPoint Origin;
        public CCSize Size;

        public CCRect(CCSize sz)
        {
            Origin = CCPoint.Zero;
            Size = sz;
        }

        /// <summary>
        ///     Creates the rectangle at (x,y) -> (width,height)
        /// </summary>
        /// <param name="x">Lower Left corner X</param>
        /// <param name="y">Lower left corner Y</param>
        /// <param name="width">width of the rectangle</param>
        /// <param name="height">height of the rectangle</param>
        public CCRect(float x, float y, float width, float height)
        {
            // Only support that, the width and height > 0
            Debug.Assert(width >= 0 && height >= 0);

            Origin.X = x;
            Origin.Y = y;

            Size.Width = width;
            Size.Height = height;
        }

        /// <summary>
        ///     Returns the inversion of this rect's size, which is the height and width swapped, while the origin stays unchanged.
        /// </summary>
        public CCRect InvertedSize
        {
            get { return new CCRect(Origin.X, Origin.Y, Size.Height, Size.Width); }
        }

        // return the rightmost x-value of 'rect'
        public float MaxX
        {
            get { return Origin.X + Size.Width; }
        }

        // return the midpoint x-value of 'rect'
        public float MidX
        {
            get { return Origin.X + Size.Width / 2.0f; }
        }

        // return the leftmost x-value of 'rect'
        public float MinX
        {
            get { return Origin.X; }
        }

        // Return the topmost y-value of 'rect'
        public float MaxY
        {
            get { return Origin.Y + Size.Height; }
        }

        // Return the midpoint y-value of 'rect'
        public float MidY
        {
            get { return Origin.Y + Size.Height / 2.0f; }
        }

        // Return the bottommost y-value of 'rect'
        public float MinY
        {
            get { return Origin.Y; }
        }

        public CCPoint Center
        {
            get
            {
                CCPoint pt;
                pt.X = MidX;
                pt.Y = MidY;
                return pt;
            }
        }

        public CCPoint UpperRight
        {
            get
            {
                CCPoint pt;
                pt.X = MaxX;
                pt.Y = MaxY;
                return (pt);
            }
        }

        public CCPoint LowerLeft
        {
            get
            {
                CCPoint pt;
                pt.X = MinX;
                pt.Y = MinY;
                return (pt);
            }
        }

        public CCRect Union(CCRect rect)
        {
            float minx = Math.Min(MinX, rect.MinX);
            float miny = Math.Min(MinY, rect.MinY);
            float maxx = Math.Max(MaxX, rect.MaxX);
            float maxy = Math.Max(MaxY, rect.MaxY);
            return (new CCRect(minx, miny, maxx - minx, maxy - miny));
        }

        public CCRect Intersection(CCRect rect)
        {
            if (!IntersectsRect(rect))
            {
                return (Zero);
            }

            /*       +-------------+
             *       |             |
             *       |         +---+-----+
             * +-----+---+     |   |     |
             * |     |   |     |   |     |
             * |     |   |     +---+-----+
             * |     |   |         |
             * |     |   |         |
             * +-----+---+         |
             *       |             |
             *       +-------------+
             */
            float minx = 0, miny = 0, maxx = 0, maxy = 0;
            // X
            if (rect.MinX < MinX)
            {
                minx = MinX;
            }
            else if (rect.MinX < MaxX)
            {
                minx = rect.MinX;
            }
            if (rect.MaxX < MaxX)
            {
                maxx = rect.MaxX;
            }
            else if (rect.MaxX > MaxX)
            {
                maxx = MaxX;
            }
            //  Y
            if (rect.MinY < MinY)
            {
                miny = MinY;
            }
            else if (rect.MinY < MaxY)
            {
                miny = rect.MinY;
            }
            if (rect.MaxY < MaxY)
            {
                maxy = rect.MaxY;
            }
            else if (rect.MaxY > MaxY)
            {
                maxy = MaxY;
            }
            return new CCRect(minx, miny, maxx - minx, maxy - miny);
        }

        public bool IntersectsRect(CCRect rect)
        {
            return !(MaxX < rect.MinX || rect.MaxX < MinX || MaxY < rect.MinY || rect.MaxY < MinY);
        }

        public bool IntersectsRect(ref CCRect rect)
        {
            return !(MaxX < rect.MinX || rect.MaxX < MinX || MaxY < rect.MinY || rect.MaxY < MinY);
        }

        public bool ContainsPoint(CCPoint point)
        {
            return point.X >= MinX && point.X <= MaxX && point.Y >= MinY && point.Y <= MaxY;
        }

        public bool ContainsPoint(float x, float y)
        {
            return x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
        }

        public static bool Equal(ref CCRect rect1, ref CCRect rect2)
        {
            return rect1.Origin.Equals(rect2.Origin) && rect1.Size.Equals(rect2.Size);
        }

        public static bool ContainsPoint(ref CCRect rect, ref CCPoint point)
        {
            bool bRet = false;

            if (float.IsNaN(point.X))
            {
                point.X = 0;
            }

            if (float.IsNaN(point.Y))
            {
                point.Y = 0;
            }

            if (point.X >= rect.MinX && point.X <= rect.MaxX && point.Y >= rect.MinY &&
                point.Y <= rect.MaxY)
            {
                bRet = true;
            }

            return bRet;
        }

        public static bool IntersetsRect(ref CCRect rectA, ref CCRect rectB)
        {
            return
                !(rectA.MaxX < rectB.MinX || rectB.MaxX < rectA.MinX || rectA.MaxY < rectB.MinY ||
                  rectB.MaxY < rectA.MinY);
        }

        public static bool operator ==(CCRect p1, CCRect p2)
        {
            return (p1.Equals(p2));
        }

        public static bool operator !=(CCRect p1, CCRect p2)
        {
            return (!p1.Equals(p2));
        }

        public override int GetHashCode()
        {
            return Origin.GetHashCode() + Size.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (Equals((CCRect) obj));
        }

        public bool Equals(CCRect rect)
        {
            return Origin.Equals(rect.Origin) && Size.Equals(rect.Size);
        }

        public override string ToString()
        {
            return String.Format("CCRect : (x={0}, y={1}, width={2}, height={3})", Origin.X, Origin.Y, Size.Width,
                                 Size.Height);
        }

        public static CCRect Parse(string s)
        {
#if !WINDOWS_PHONE && !XBOX && !NETFX_CORE
            return (CCRect) TypeDescriptor.GetConverter(typeof (CCRect)).ConvertFromString(s);
#else
            return (CCRectConverter.CCRectFromString(s));
#endif
        }
    }
}
