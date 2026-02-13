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
    public struct CCVertex2F
    {
        /*
        public ccVertex2F()
        {
            x = 0.0f;
            y = 0.0f;
        }
        */

        public CCVertex2F(float inx, float iny)
        {
            X = inx;
            Y = iny;
        }

        public float X;
        public float Y;
    }
}
