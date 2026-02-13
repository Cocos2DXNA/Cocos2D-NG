using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;
using CocosNG.Core.Primitives;

namespace CocosNG.Core.Nodes
{
    public enum CCEmitterMode
    {
        /** Gravity mode (A mode) */
        Gravity,

        /** Radius mode (B mode) */
        Radius,
    }
}
