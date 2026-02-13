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
    public struct CCTex2F
    {
        /*
        public ccTex2F()
        {
            u = 0.0f;
            v = 0.0f;
        }
        */
        public CCTex2F(float inu, float inv)
        {
            U = inu;
            V = inv;
        }

        public float U;
        public float V;

        public override string ToString()
        {
            return String.Format("ccTex2F u:{0}, v:{1}", U, V);
        }
    }
}
