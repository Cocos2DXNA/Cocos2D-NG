using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core
{
    public enum CCResolutionPolicy
    {
        UnKnown,

        // The entire application is visible in the specified area without trying to preserve the original aspect ratio. 
        // Distortion can occur, and the application may appear stretched or compressed.
        ExactFit,
        // The entire application fills the specified area, without distortion but possibly with some cropping, 
        // while maintaining the original aspect ratio of the application.
        NoBorder,
        // The entire application is visible in the specified area without distortion while maintaining the original 
        // aspect ratio of the application. Borders can appear on two sides of the application.
        ShowAll,
        // The application takes the height of the design resolution size and modifies the width of the internal
        // canvas so that it fits the aspect ratio of the device
        // no distortion will occur however you must make sure your application works on different
        // aspect ratios
        FixedHeight,
        // The application takes the width of the design resolution size and modifies the height of the internal
        // canvas so that it fits the aspect ratio of the device
        // no distortion will occur however you must make sure your application works on different
        // aspect ratios
        FixedWidth
    }
}
