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
    public struct CCBlendFunc
    {
        public static readonly CCBlendFunc AlphaBlend = new CCBlendFunc(CCOGLES.GL_ONE, CCOGLES.GL_ONE_MINUS_SRC_ALPHA);
        public static readonly CCBlendFunc Additive = new CCBlendFunc(CCOGLES.GL_SRC_ALPHA, CCOGLES.GL_ONE);
        public static readonly CCBlendFunc NonPremultiplied = new CCBlendFunc(CCOGLES.GL_SRC_ALPHA, CCOGLES.GL_ONE_MINUS_SRC_ALPHA);
        public static readonly CCBlendFunc Opaque = new CCBlendFunc(CCOGLES.GL_ONE, CCOGLES.GL_ZERO);

        public CCBlendFunc(int src, int dst)
        {
            this.Source = src;
            this.Destination = dst;
        }

        /// <summary>
        /// source blend function
        /// </summary>
        public int Source;

        /// <summary>
        /// destination blend function
        /// </summary>
        public int Destination;

        public static bool operator ==(CCBlendFunc b1, CCBlendFunc b2)
        {
            return b1.Source == b2.Source && b1.Destination == b2.Destination;
        }

        public static bool operator !=(CCBlendFunc b1, CCBlendFunc b2)
        {
            return b1.Source != b2.Source || b1.Destination != b2.Destination;
        }

        public override bool Equals(object obj)
        {
            if (obj is CCBlendFunc)
            {
                return this == (CCBlendFunc) obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
