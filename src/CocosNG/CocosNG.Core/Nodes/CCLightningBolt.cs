using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocosNG.Core;
using CocosNG.Core.Primitives;

namespace CocosNG.Core.Nodes
{
    public struct CCLightningBolt 
    {
        public CCPoint Start;
        public CCPoint End;
        public float Width;
        public CCColor4F BoltColor;
        public CCColor4F BoltGlowColor;
        public float StrikeTime;
        public float FadeTime;

        public CCLightningBolt WithStart(CCPoint start)
        {
            Start = start;
            return this;
        }

        public CCLightningBolt WithEnd(CCPoint end)
        {
            End = end;
            return this;
        }

        public CCLightningBolt WithWidth(float width)
        {
            Width = width;
            return this;
        }

        public CCLightningBolt WithBoltColor(CCColor4F color)
        {
            BoltColor = color;
            return this;
        }

        public CCLightningBolt WithBoltGlowColor(CCColor4F color)
        {
            BoltGlowColor = color;
            return this;
        }

        public CCLightningBolt WithStrikeTime(float strikeTime)
        {
            StrikeTime = strikeTime;
            return this;
        }

        public CCLightningBolt WithFadeTime(float fadeTime)
        {
            FadeTime = fadeTime;
            return this;
        }
    }
}
