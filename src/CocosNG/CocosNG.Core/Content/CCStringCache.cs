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
    internal struct CCStringCache
    {
        public String Text;
        public CCSize Dimensions;
        public CCTextAlignment HAlignment;
        public CCVerticalTextAlignment VAlignment;
        public String FontName;
        public float FontSize;
    }
}
