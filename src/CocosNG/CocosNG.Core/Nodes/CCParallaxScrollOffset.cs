using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocosNG.Core;

namespace CocosNG.Core.Nodes
{
    public class CCParallaxScrollOffset
    {
        public CCPoint Offset { get; set; }
        public CCPoint OriginalPosition { get; set; }
        public CCPoint RelativeVelocity { get; set; }
        public CCPoint Ratio { get; set; }
        public CCPoint Position { get; set; }
        public CCPoint CurrentPosition { get; set; }
        public CCNode Child { get; set; }

        public CCParallaxScrollOffset WithOffset(CCPoint offset)
        {
            Offset = offset;
            return this;
        }

        public CCParallaxScrollOffset WithOriginalPosition(CCPoint position)
        {
            OriginalPosition = position;
            return this;
        }

        public CCParallaxScrollOffset WithRelativeVelocity(CCPoint velocity)
        {
            RelativeVelocity = velocity;
            return this;
        }

        public CCParallaxScrollOffset WithRatio(CCPoint ratio)
        {
            Ratio = ratio;
            return this;
        }

        public CCParallaxScrollOffset WithPosition(CCPoint position)
        {
            Position = position;
            return this;
        }

        public CCParallaxScrollOffset WithCurrentPosition(CCPoint position)
        {
            CurrentPosition = position;
            return this;
        }

        public CCParallaxScrollOffset WithChild(CCNode child)
        {
            Child = child;
            return this;
        }

        public CCParallaxScrollOffset(CCNode node, CCPoint r, CCPoint p, CCPoint s)
            : this(node, r, p, s, CCPoint.Zero)
        {
        }

        public CCParallaxScrollOffset(CCNode node, CCPoint r, CCPoint p, CCPoint s, CCPoint v)
        {
            Child = node;
            Ratio = r;
            Offset = s;
            RelativeVelocity = v;
            Child.Position = p;
            OriginalPosition = p;
            //currPosition = p;
            Child.AnchorPoint = CCPoint.AnchorLowerLeft;
        }
    }
}
