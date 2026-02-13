using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionSlideInR : CCTransitionSlideInL
    {
        public CCTransitionSlideInR() { }

        public CCTransitionSlideInR (float t, CCScene scene) : base (t, scene)
        { }

        /// <summary>
        /// initializes the scenes 
        /// </summary>
        public override void InitScenes()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            m_pInScene.Position = new CCPoint(s.Width - 0.5f, 0);
        }

        /// <summary>
        /// returns the action that will be performed by the incomming and outgoing scene
        /// </summary>
        /// <returns></returns>
        public override CCActionInterval Action()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            return new CCMoveBy (m_fDuration, new CCPoint(-(s.Width - 0.5f), 0));
        }

        protected override void SceneOrder()
        {
            m_bIsInSceneOnTop = true;
        }
    }
}
