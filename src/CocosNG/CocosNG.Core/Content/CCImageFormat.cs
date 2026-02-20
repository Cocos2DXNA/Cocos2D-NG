using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core.Content
{
    public enum CCImageFormat
    {
        Jpg = 0,
        Png,
        Tiff,
        Webp,
        Gif,
        RawData,
        UnKnown
    }
}
