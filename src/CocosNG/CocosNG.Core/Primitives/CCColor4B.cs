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
    public struct CCColor4B
    {
        //ccColor4B predefined colors
        //! White color (255,255,255)
        public static readonly CCColor4B White = new CCColor4B(255, 255, 255, 255);
        //! Yellow color (255,255,0)
        public static readonly CCColor4B Yellow = new CCColor4B(255, 255, 0, 255);
        //! Blue color (0,0,255)
        public static readonly CCColor4B Blue = new CCColor4B(0, 0, 255, 255);
        //! Green Color (0,255,0)
        public static readonly CCColor4B Green = new CCColor4B(0, 255, 0, 255);
        //! Red Color (255,0,0,)
        public static readonly CCColor4B Red = new CCColor4B(255, 0, 0, 255);
        //! Magenta Color (255,0,255)
        public static readonly CCColor4B Magenta = new CCColor4B(255, 0, 255, 255);
        //! Black Color (0,0,0)
        public static readonly CCColor4B Black = new CCColor4B(0, 0, 0, 255);
        //! Orange Color (255,127,0)
        public static readonly CCColor4B Orange = new CCColor4B(255, 127, 0, 255);
        //! Gray Color (166,166,166)
        public static readonly CCColor4B Gray = new CCColor4B(166, 166, 166, 255);

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public CCColor4B(byte inr, byte ing, byte inb)
        {
            R = inr;
            G = ing;
            B = inb;
            A = 255;
        }

        public CCColor4B(byte inr, byte ing, byte inb, byte ina)
        {
            R = inr;
            G = ing;
            B = inb;
            A = ina;
        }

        public CCColor4B(float inr, float ing, float inb, float ina)
        {
            R = (byte)inr;
            G = (byte)ing;
            B = (byte)inb;
            A = (byte)ina;
        }

        /// <summary>
        /// Convert Color value of XNA Framework to CCColor4B type
        /// </summary>
        public CCColor4B(Microsoft.Xna.Framework.Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public override string ToString()
        {
            return (string.Format("{0},{1},{2},{3}", R, G, B, A));
        }

        public static CCColor4B Parse(string s)
        {
            string[] f = s.Split(',');
            return (new CCColor4B(byte.Parse(f[0]), byte.Parse(f[1]), byte.Parse(f[2]), byte.Parse(f[3])));
        }

        public static implicit operator Color(CCColor4B point)
        {
            return new Color(point.R, point.G, point.B, point.A);
        }

        public static implicit operator CCColor3B(CCColor4B point)
        {
            return new CCColor3B(point.R, point.G, point.B);
        }

        public static implicit operator CCColor4B(CCColor3B point)
        {
            return new CCColor4B(point.R, point.G, point.B, 255);
        }

        public static CCColor4B Lerp(CCColor4B value1, CCColor4B value2, float amount)
        {
            CCColor4B color;

            color.A = (byte)(value1.A + ((value2.A - value1.A) * amount));
            color.R = (byte)(value1.R + ((value2.R - value1.R) * amount));
            color.G = (byte)(value1.G + ((value2.G - value1.G) * amount));
            color.B = (byte)(value1.B + ((value2.B - value1.B) * amount));

            return color;
        }

        public static bool operator ==(CCColor4B p1, CCColor4B p2)
        {
            return p1.R == p2.R && p1.G == p2.G && p1.B == p2.B && p1.A == p2.A;
        }

        public static bool operator !=(CCColor4B p1, CCColor4B p2)
        {
            return p1.R != p2.R || p1.G != p2.G || p1.B != p2.B || p1.A != p2.A;
        }
    }
}
