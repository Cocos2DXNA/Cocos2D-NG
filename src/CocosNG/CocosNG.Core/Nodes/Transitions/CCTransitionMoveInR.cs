using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionMoveInR : CCTransitionMoveInL
    {

        public CCTransitionMoveInR() { }

        public CCTransitionMoveInR (float t, CCScene scene) : base (t, scene)
        { }

        public override void InitScenes()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            m_pInScene.Position = new CCPoint(s.Width, 0);
        }

    }
}
