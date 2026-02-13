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
    public class CCV2F_C4F_T2F_Quad
    {
        public CCV2F_C4F_T2F_Quad()
        {
            TopLeft = new CCV2F_C4F_T2F();
            BottomLeft = new CCV2F_C4F_T2F();
            TopRight = new CCV2F_C4F_T2F();
            BottomRight = new CCV2F_C4F_T2F();
        }

        /// <summary>
        /// bottom left
        /// </summary>
        public CCV2F_C4F_T2F BottomLeft;

        /// <summary>
        /// bottom right
        /// </summary>
        public CCV2F_C4F_T2F BottomRight;

        /// <summary>
        /// top left
        /// </summary>
        public CCV2F_C4F_T2F TopLeft;

        /// <summary>
        /// top right
        /// </summary>
        public CCV2F_C4F_T2F TopRight;
    }
}
