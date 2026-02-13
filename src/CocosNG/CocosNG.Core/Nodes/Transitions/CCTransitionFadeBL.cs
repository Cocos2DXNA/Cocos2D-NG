using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionFadeBL : CCTransitionFadeTR
    {
        public override CCActionInterval CreateAction(CCGridSize size)
        {
            return new CCFadeOutBLTiles(m_fDuration, size);
        }

        public CCTransitionFadeBL() { }
        public CCTransitionFadeBL(float t, CCScene scene) : base(t, scene)
        {
            InitWithDuration(t, scene);
        }
    }
}
