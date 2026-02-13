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
    public class CCV2F_C4F_T2F
    {
        public CCV2F_C4F_T2F()
        {
            Vertices = new CCVertex2F();
            Colors = new CCColor4F();
            TexCoords = new CCTex2F();
        }

        /// <summary>
        /// vertices (2F)
        /// </summary>
        public CCVertex2F Vertices;

        /// <summary>
        /// colors (4F)
        /// </summary>
        public CCColor4F Colors;

        /// <summary>
        /// tex coords (2F)
        /// </summary>
        public CCTex2F TexCoords;
    }
}
