using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionFadeUp : CCTransitionFadeTR
    {
        public override CCActionInterval CreateAction(CCGridSize size)
        {
            return new CCFadeOutUpTiles(m_fDuration, size);
        }
        public CCTransitionFadeUp() { }

        public CCTransitionFadeUp(float t, CCScene scene) : base(t, scene)
        {
            InitWithDuration(t, scene);
        }
    }
}
