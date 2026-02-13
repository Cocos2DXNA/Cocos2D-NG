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
    public class CCPointSprite
    {
        public CCPointSprite()
        {
            Position = new CCVertex2F();
            Color = new CCColor4B();
            Size = 0.0f;
        }

        public CCVertex2F Position;		// 8 bytes
        public CCColor4B Color;		// 4 bytes
        public float Size;		// 4 bytes
    }
}
