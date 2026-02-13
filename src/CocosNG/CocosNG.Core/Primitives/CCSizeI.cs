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
    public struct CCSizeI
    {
        public int Width;
        public int Height;

        public CCSizeI(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static implicit operator CCSize(CCSizeI p)
        {
            return new CCSize(p.Width, p.Height);
        }
    }
}
