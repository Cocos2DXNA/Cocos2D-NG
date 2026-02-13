using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionSlideInB : CCTransitionSlideInL
    {
        public CCTransitionSlideInB() { }

        public CCTransitionSlideInB (float t, CCScene scene) : base (t, scene)
        { }

        /// <summary>
        ///  initializes the scenes 
        /// </summary>
        public override void InitScenes()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            m_pInScene.Position = new CCPoint(0, -(s.Height - 0.5f));
        }

        /// <summary>
        ///  returns the action that will be performed by the incomming and outgoing scene 
        /// </summary>
        /// <returns></returns>
        public override CCActionInterval Action()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            return new CCMoveBy (m_fDuration, new CCPoint(0, s.Height - 0.5f));
        }

        protected override void SceneOrder()
        {
            m_bIsInSceneOnTop = true;
        }

    }
}
