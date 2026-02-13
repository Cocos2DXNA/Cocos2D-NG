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
    public struct CCVertex3F
    {
        public static readonly CCVertex3F Zero = new CCVertex3F();

        public CCVertex3F(float inx, float iny, float inz)
        {
            X = inx;
            Y = iny;
            Z = inz;
        }

        public float X;
        public float Y;
        public float Z;

        public override string ToString()
        {
            return String.Format("ccVertex3F x:{0}, y:{1}, z:{2}", X, Y, Z);
        }
    }
}
