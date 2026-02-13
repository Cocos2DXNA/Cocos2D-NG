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
    public struct CCQuad3
    {
        /*
        public ccQuad3()
        {
            tl = new ccVertex3F();
            tr = new ccVertex3F();
            bl = new ccVertex3F();
            br = new ccVertex3F();
        }
        */
        public CCVertex3F BottomLeft;
        public CCVertex3F BottomRight;
        public CCVertex3F TopLeft;
        public CCVertex3F TopRight;
    }
}
