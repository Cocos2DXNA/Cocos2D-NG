using System;
using System.IO;
using CocosNG.Core;

namespace CocosNG.Core.Nodes
{
    public class CCScene : CCNode
    {
        /// <summary>
        /// Sets the anchor point to the middle of the scene but ignores the anchor for positioning.
        /// </summary>
        public CCScene()
        {
            m_bIgnoreAnchorPointForPosition = true;
            AnchorPoint = new CCPoint(0.5f, 0.5f);
            ContentSize = CCDirector.SharedDirector.WinSize;
        }

        /// <summary>
        /// Returns false always unless this is a transition scene.
        /// </summary>
        public virtual bool IsTransition
        {
            get
            {
                return (false);
            }
        }
        /// <summary>
        /// Initialize this scene
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            return (true);
        }

        public CCScene WithContentSize(CCSize size)
        {
            ContentSize = size;
            return this;
        }
    }
}
