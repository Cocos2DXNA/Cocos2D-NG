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
    public class CCV2F_C4B_T2F
    {
        public CCV2F_C4B_T2F()
        {
            Vertices = new CCVertex2F();
            Colors = new CCColor4B();
            TexCoords = new CCTex2F();
        }

        /// <summary>
        /// vertices (2F)
        /// </summary>
        public CCVertex2F Vertices;

        /// <summary>
        /// colors (4B)
        /// </summary>
        public CCColor4B Colors;

        /// <summary>
        /// tex coords (2F)
        /// </summary>
        public CCTex2F TexCoords;
    }
}
