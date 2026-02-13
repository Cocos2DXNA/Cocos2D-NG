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
    public struct CCV3F_C4B_T2F : IVertexType
    {
        /// <summary>
        /// vertices (3F)
        /// </summary>
        public CCVertex3F Vertices;			// 12 bytes

        /// <summary>
        /// colors (4B)
        /// </summary>
        public CCColor4B Colors;				// 4 bytes

        /// <summary>
        /// tex coords (2F)
        /// </summary>
        public CCTex2F TexCoords;			// 8 byts

        public static readonly VertexDeclaration VertexDeclaration;

        static CCV3F_C4B_T2F()
        {
            var elements = new VertexElement[]
                {
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                    new VertexElement(0x10, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                };
            VertexDeclaration = new VertexDeclaration(elements);
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }
}
