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
    public struct CCColor3B
    {
        //ccColor3B predefined colors
        //! White color (255,255,255)
        public static readonly CCColor3B White = new CCColor3B(255, 255, 255);
        //! Yellow color (255,255,0)
        public static readonly CCColor3B Yellow = new CCColor3B(255, 255, 0);
        //! Blue color (0,0,255)
        public static readonly CCColor3B Blue = new CCColor3B(0, 0, 255);
        //! Green Color (0,255,0)
        public static readonly CCColor3B Green = new CCColor3B(0, 255, 0);
        //! Red Color (255,0,0,)
        public static readonly CCColor3B Red = new CCColor3B(255, 0, 0);
        //! Magenta Color (255,0,255)
        public static readonly CCColor3B Magenta = new CCColor3B(255, 0, 255);
        //! Black Color (0,0,0)
        public static readonly CCColor3B Black = new CCColor3B(0, 0, 0);
        //! Orange Color (255,127,0)
        public static readonly CCColor3B Orange = new CCColor3B(255, 127, 0);
        //! Gray Color (166,166,166)
        public static readonly CCColor3B Gray = new CCColor3B(166, 166, 166);

        /*
        public CCColor3B()
        {
            r = 0;
            g = 0;
            b = 0;
        }
        */
        public CCColor3B(byte inr, byte ing, byte inb)
        {
            R = inr;
            G = ing;
            B = inb;
        }

        /// <summary>
        /// Convert Color value of XNA Framework to CCColor3B type
        /// </summary>
        public CCColor3B(Microsoft.Xna.Framework.Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public byte R;
        public byte G;
        public byte B;

        public static implicit operator Color(CCColor3B point)
        {
            return new Color(point.R, point.G, point.B);
        }

        public CCColor4B AsColor4B()
        {
            return (new CCColor4B(R, G, B, 255));
        }

        public CCColor4B AsColor4B(byte alpha)
        {
            return (new CCColor4B(R, G, B, alpha));
        }

        public CCColor4F AsColor4F()
        {
            return (new CCColor4F(R/255f, G/255f, B/255f, 1f));
        }
        public CCColor4F AsColor4F(float alpha)
        {
            return (new CCColor4F(R/255f, G/255f, B/255f, alpha));
        }
    }
}
