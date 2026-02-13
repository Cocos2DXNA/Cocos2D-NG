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
    public struct CCV3F_C4B_T2F_Quad : IVertexType
    {
        /// <summary>
        /// top left
        /// </summary>
        public CCV3F_C4B_T2F TopLeft;

        /// <summary>
        /// bottom left
        /// </summary>
        public CCV3F_C4B_T2F BottomLeft;

        /// <summary>
        /// top right
        /// </summary>
        public CCV3F_C4B_T2F TopRight;

        /// <summary>
        /// bottom right
        /// </summary>
        public CCV3F_C4B_T2F BottomRight;

        public static readonly VertexDeclaration VertexDeclaration;

        static CCV3F_C4B_T2F_Quad()
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
