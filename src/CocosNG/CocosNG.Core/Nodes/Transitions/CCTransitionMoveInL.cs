using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionMoveInL : CCTransitionScene, ICCTransitionEaseScene
    {

        public CCTransitionMoveInL() { }

        public CCTransitionMoveInL (float t, CCScene scene) : base (t, scene)
        { }

        #region ICCTransitionEaseScene Members

        public CCFiniteTimeAction EaseAction(CCActionInterval action)
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
            m_pInScene.Position = new CCPoint(-s.Width, 0);
        }

        /// <summary>
        /// returns the action that will be performed
        /// </summary>
        /// <returns></returns>
        public virtual CCActionInterval Action()
        {
            return new CCMoveTo (m_fDuration, new CCPoint(0, 0));
        }

        public override void OnEnter()
        {
            base.OnEnter();
            InitScenes();

            CCActionInterval a = Action();

            m_pInScene.RunAction
                (
                    new CCSequence
                        (
                            EaseAction(a),
                            new CCCallFunc(Finish)
                        )
                );
        }

    }
}
