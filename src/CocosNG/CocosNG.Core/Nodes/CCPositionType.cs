using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core.Nodes
{
    public enum CCPositionType
    {
        /** Living particles are attached to the world and are unaffected by emitter repositioning. */
        Free,

        /** Living particles are attached to the world but will follow the emitter repositioning.
            Use case: Attach an emitter to an sprite, and you want that the emitter follows the sprite.
        */
        Relative,

        /** Living particles are attached to the emitter and are translated along with it. */
        Grouped,
    }
}
