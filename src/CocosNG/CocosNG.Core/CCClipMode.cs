using System;
using System.Collections.Generic;
using System.Diagnostics;
using CocosNG.Core;

namespace CocosNG.Core
{
    public enum CCClipMode
    {
        /// <summary>
        /// No clipping of children
        /// </summary>
        None,
        /// <summary>
        /// Clipping with a ScissorRect
        /// </summary>
        Bounds,
        /// <summary>
        /// Clipping with the ScissorRect and in a RenderTarget
        /// </summary>
        BoundsWithRenderTarget
    }
}
