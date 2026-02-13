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
    public struct CCGridSize
    {
        public CCGridSize(int inx, int iny)
        {
            X = inx;
            Y = iny;
        }

        public int X;
        public int Y;
    }
}
