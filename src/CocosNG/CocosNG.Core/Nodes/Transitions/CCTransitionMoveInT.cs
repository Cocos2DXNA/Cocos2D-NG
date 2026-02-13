using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionMoveInT : CCTransitionMoveInL
    {

        public CCTransitionMoveInT()
        {
        }

        public CCTransitionMoveInT (float t, CCScene scene) : base (t, scene)
        { }

        public override void InitScenes()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            m_pInScene.Position = new CCPoint(0, s.Height);
        }


    }
}
