using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionMoveInB : CCTransitionMoveInL
    {
        public CCTransitionMoveInB() { }

        public CCTransitionMoveInB (float t, CCScene scene) : base (t, scene)
        { }

        public override void InitScenes()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            m_pInScene.Position = new CCPoint(0, -s.Height);
        }

    }
}
