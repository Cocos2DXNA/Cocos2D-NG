using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionFadeDown : CCTransitionFadeTR
    {
        public override CCActionInterval CreateAction(CCGridSize size)
        {
            return new CCFadeOutDownTiles(m_fDuration, size);
        }

        public CCTransitionFadeDown() { }
        public CCTransitionFadeDown(float t, CCScene scene) : base(t, scene)
        {
            InitWithDuration(t, scene);
        }
    }
}
