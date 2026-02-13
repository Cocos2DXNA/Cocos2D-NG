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
    public struct CCColor4F
    {
        public CCColor4F(float inr, float ing, float inb, float ina)
        {
            R = inr;
            G = ing;
            B = inb;
            A = ina;
        }

        public float R;
        public float G;
        public float B;
        public float A;

        public override string ToString()
        {
            return (string.Format("{0},{1},{2},{3}", R, G, B, A));
        }

        public static CCColor4F Parse(string s)
        {
            string[] f = s.Split(',');
            if (f.Length == 4)
            {
                return (new CCColor4F(float.Parse(f[0]), float.Parse(f[1]), float.Parse(f[2]), float.Parse(f[3])));
            }
            return (new CCColor4F(float.Parse(f[0]), float.Parse(f[1]), float.Parse(f[2]), 1f));
        }

        public static implicit operator Color(CCColor4F point)
        {
            return new Color(point.R, point.G, point.B, point.A);
        }

        public static implicit operator CCColor3B(CCColor4F point)
        {
            return new CCColor3B((byte)(point.R * point.A * 255f), (byte)(point.G*point.A*255f), (byte)(point.B*point.A*255));
        }

        public static implicit operator CCColor4F(CCColor3B point)
        {
            return new CCColor4F((float)point.R/255f, (float)point.G/255f, (float)point.B/255f, 1f);
        }

        public static CCColor4F operator +(CCColor4F c, float amt)
        {
            CCColor4F nc = new CCColor4F(c.R + amt, c.G + amt, c.B + amt, c.A);
            return (nc);
        }

        public static CCColor4F operator *(CCColor4F c, float amt) 
        {
            CCColor4F nc = new CCColor4F(c.R * amt, c.G * amt, c.B * amt, c.A);
            return (nc);
        }

        public static CCColor4F operator /(CCColor4F c, float amt)
        {
            CCColor4F nc = new CCColor4F(c.R / amt, c.G / amt, c.B / amt, c.A);
            return (nc);
        }

        public static CCColor4F Lerp(CCColor4F value1, CCColor4F value2, float amount)
        {
            CCColor4F color;

            color.A = (value1.A + ((value2.A - value1.A) * amount));
            color.R = (value1.R + ((value2.R - value1.R) * amount));
            color.G = (value1.G + ((value2.G - value1.G) * amount));
            color.B = (value1.B + ((value2.B - value1.B) * amount));

            return color;
        }

        public static bool operator ==(CCColor4F p1, CCColor4F p2)
        {
            return p1.R == p2.R && p1.G == p2.G && p1.B == p2.B && p1.A == p2.A;
        }

        public static bool operator !=(CCColor4F p1, CCColor4F p2)
        {
            return p1.R != p2.R || p1.G != p2.G || p1.B != p2.B || p1.A != p2.A;
        }
    }
}
