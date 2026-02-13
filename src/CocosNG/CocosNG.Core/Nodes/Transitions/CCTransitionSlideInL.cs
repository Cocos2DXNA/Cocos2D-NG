using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionSlideInL : CCTransitionScene, ICCTransitionEaseScene
    {

        public CCTransitionSlideInL() { }

        public CCTransitionSlideInL (float t, CCScene scene) : base (t, scene)
        { }
        
        #region ICCTransitionEaseScene Members

        public virtual CCFiniteTimeAction EaseAction(CCActionInterval action)
        {
            return new CCEaseOut(action, 2.0f);
        }

        #endregion

        /// <summary>
        /// initializes the scenes
        /// </summary>
        public virtual void InitScenes()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            m_pInScene.Position = new CCPoint(-(s.Width - 0.5f), 0);
        }

        /// <summary>
        /// returns the action that will be performed by the incomming and outgoing scene
        /// </summary>
        /// <returns></returns>
        public virtual CCActionInterval Action()
        {
            CCSize s = CCDirector.SharedDirector.WinSize;
            return new CCMoveBy (m_fDuration, new CCPoint(s.Width - 0.5f, 0));
        }

        public override void OnEnter()
        {
            base.OnEnter();
            InitScenes();

            CCActionInterval incAction = Action();
            CCActionInterval outcAction = Action();

            CCFiniteTimeAction inAction = EaseAction(incAction);
            CCActionInterval outAction = new CCSequence
                (
                    EaseAction(outcAction),
                    new CCCallFunc((Finish))
                );
            m_pInScene.RunAction(inAction);
            m_pOutScene.RunAction(outAction);
        }

        protected override void SceneOrder()
        {
            m_bIsInSceneOnTop = false;
        }
    }
}
