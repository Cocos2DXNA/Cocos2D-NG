using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using CocosNG.Core;

namespace CocosNG.Core.Primitives
{
    public struct CCSize
    {
        public static readonly CCSize Zero = new CCSize(0, 0);

        public float Width;
        public float Height;

        public CCSize(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public CCSize Clamp(CCSize max)
        {
            float w = (Width > max.Width) ? max.Width : Width;
            float h = (Height > max.Height) ? max.Height : Height;
            return (new CCSize(w, h));
        }

        /// <summary>
        /// Computes the diagonal length of this size. This method will always compute
        /// the length using Sqrt()
        /// </summary>
        public float Diagonal
        {
            get
            {
                return ((float)Math.Sqrt(Width * Width + Height * Height));
            }
        }

        /// <summary>
        ///     Returns the inversion of this size, which is the height and width swapped.
        /// </summary>
        public CCSize Inverted
        {
            get { return new CCSize(Height, Width); }
        }

        public static bool Equal(ref CCSize size1, ref CCSize size2)
        {
            return ((size1.Width == size2.Width) && (size1.Height == size2.Height));
        }

        public override int GetHashCode()
        {
            return (Width.GetHashCode() + Height.GetHashCode());
        }

        public bool Equals(CCSize s)
        {
            return Width == s.Width && Height == s.Height;
        }

        public override bool Equals(object obj)
        {
            return (Equals((CCSize) obj));
        }

        public CCPoint Center
        {
            get { return new CCPoint(Width / 2f, Height / 2f); }
        }

        public override string ToString()
        {
            return String.Format("{0} x {1}", Width, Height);
        }

        public static bool operator ==(CCSize p1, CCSize p2)
        {
            return (p1.Equals(p2));
        }

        public static bool operator !=(CCSize p1, CCSize p2)
        {
            return (!p1.Equals(p2));
        }

        public static CCSize operator *(CCSize p, float f)
        {
            return (new CCSize(p.Width * f, p.Height * f));
        }

        public static CCSize operator /(CCSize p, float f)
        {
            return (new CCSize(p.Width / f, p.Height / f));
        }

        public static CCSize operator +(CCSize p, float f)
        {
            return (new CCSize(p.Width + f, p.Height + f));
        }

        public static CCSize operator +(CCSize p, CCSize q)
        {
            return (new CCSize(p.Width + q.Width, p.Height + q.Height));
        }

        public static CCSize operator -(CCSize p, float f)
        {
            return (new CCSize(p.Width - f, p.Height - f));
        }

        public static CCSize Parse(string s)
        {
#if !WINDOWS_PHONE && !XBOX && !NETFX_CORE
            return (CCSize) TypeDescriptor.GetConverter(typeof (CCSize)).ConvertFromString(s);
#else
            return (CCSizeConverter.CCSizeFromString(s));
#endif
        }

        /**
         * Allow Cast CCPoint to CCSize
         */

        public static explicit operator CCSize(CCPoint point)
        {
            CCSize size;
            size.Width = point.X;
            size.Height = point.Y;
            return size;
        }

        public CCRect AsRect
        {
            get
            {
                return (new CCRect(0, 0, Width, Height));
            }
        }
    }
}
