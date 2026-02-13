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
    public class CCTypes
    {
        //ccColor3B predefined colors
        //! White color (255,255,255)
        public static readonly CCColor3B CCWhite = new CCColor3B(255, 255, 255);
        //! Yellow color (255,255,0)
        public static readonly CCColor3B CCYellow = new CCColor3B(255, 255, 0);
        //! Blue color (0,0,255)
        public static readonly CCColor3B CCBlue = new CCColor3B(0, 0, 255);
        //! Green Color (0,255,0)
        public static readonly CCColor3B CCGreen = new CCColor3B(0, 255, 0);
        //! Red Color (255,0,0,)
        public static readonly CCColor3B CCRed = new CCColor3B(255, 0, 0);
        //! Magenta Color (255,0,255)
        public static readonly CCColor3B CCMagenta = new CCColor3B(255, 0, 255);
        //! Black Color (0,0,0)
        public static readonly CCColor3B CCBlack = new CCColor3B(0, 0, 0);
        //! Orange Color (255,127,0)
        public static readonly CCColor3B CCOrange = new CCColor3B(255, 127, 0);
        //! Gray Color (166,166,166)
        public static readonly CCColor3B CCGray = new CCColor3B(166, 166, 166);

        //! helper macro that creates an ccColor3B type
        static public CCColor3B CreateColor(byte r, byte g, byte b)
        {
            CCColor3B c = new CCColor3B(r, g, b);
            return c;
        }

        //! helper macro that creates an ccColor4B type
        public static CCColor4B CreateColor(byte r, byte g, byte b, byte o)
        {
            CCColor4B c = new CCColor4B(r, g, b, o);
            return c;
        }

        /** Returns a ccColor4F from a ccColor3B. Alpha will be 1.
         @since v0.99.1
         */
        public static CCColor4F CreateColor(CCColor3B c)
        {
            CCColor4F c4 = new CCColor4F(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, 1.0f);
            return c4;
        }

        /** Returns a ccColor4F from a ccColor4B.
         @since v0.99.1
         */
        public static CCColor4F CreateColor(CCColor4B c)
        {
            CCColor4F c4 = new CCColor4F(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
            return c4;
        }

        /** returns YES if both ccColor4F are equal. Otherwise it returns NO.
         @since v0.99.1
         */
        public static bool ColorsAreEqual(CCColor4F a, CCColor4F b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        public static CCVertex2F Vertex2(float x, float y)
        {
            CCVertex2F c = new CCVertex2F(x, y);
            return c;
        }

        public static CCVertex3F Vertex3(float x, float y, float z)
        {
            CCVertex3F c = new CCVertex3F(x, y, z);
            return c;
        }

        public static CCTex2F Tex2(float u, float v)
        {
            CCTex2F t = new CCTex2F(u, v);
            return t;
        }

        //! helper function to create a ccGridSize
        public static CCGridSize GridSize(int x, int y)
        {
            CCGridSize v = new CCGridSize(x, y);
            return v;
        }

        // Make a color from hue, saturation and value parameters. Hue should be
        // between 0 and 6, while saturation and value should be between 0 and 1.
        public static CCColor4F HSVToColor(float h, float s, float v)
        {
            if (h == 0 && s == 0)
                return new CCColor4F(v, v, v, 1f);

            float c = s * v;
            float x = c * (1 - Math.Abs(h % 2 - 1));
            float m = v - c;

            if (h < 1) return new CCColor4F(c + m, x + m, m, 1f);
            else if (h < 2) return new CCColor4F(x + m, c + m, m, 1f);
            else if (h < 3) return new CCColor4F(m, c + m, x + m, 1f);
            else if (h < 4) return new CCColor4F(m, x + m, c + m, 1f);
            else if (h < 5) return new CCColor4F(x + m, m, c + m, 1f);
            else return new CCColor4F(c + m, m, x + m,1f);
        }
    }
}
