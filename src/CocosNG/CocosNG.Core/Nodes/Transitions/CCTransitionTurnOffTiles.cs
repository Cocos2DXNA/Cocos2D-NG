using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionTurnOffTiles : CCTransitionScene, ICCTransitionEaseScene
    {
        public CCTransitionTurnOffTiles() { }

        public CCTransitionTurnOffTiles (float t, CCScene scene) : base (t, scene)
        { }
        

        #region ICCTransitionEaseScene Members

        public virtual CCFiniteTimeAction EaseAction(CCActionInterval action)
        {
            return action;
        }

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();
            CCSize s = CCDirector.SharedDirector.WinSize;
            float aspect = s.Width / s.Height;
            var x = (int) (12 * aspect);
            int y = 12;

            CCTurnOffTiles toff = new CCTurnOffTiles(m_fDuration, new CCGridSize(x, y));
            CCFiniteTimeAction action = EaseAction(toff);
            m_pOutScene.RunAction
                (
                    new CCSequence
                        (
                            action,
                            new CCCallFunc((Finish)),
                            new CCStopGrid()
                        )
                );
        }

        protected override void SceneOrder()
        {
            m_bIsInSceneOnTop = false;
        }
    }
}
