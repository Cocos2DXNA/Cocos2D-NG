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
    public class CCQuad2
    {
        public CCQuad2()
        {
            TopLeft = new CCVertex2F();
            TopRight = new CCVertex2F();
            BottomLeft = new CCVertex2F();
            BottomRight = new CCVertex2F();
        }

        public CCVertex2F TopLeft;
        public CCVertex2F TopRight;
        public CCVertex2F BottomLeft;
        public CCVertex2F BottomRight;
    }
}
