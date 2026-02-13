using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionFadeTR : CCTransitionScene, ICCTransitionEaseScene
    {
        #region ICCTransitionEaseScene Members

        public virtual CCFiniteTimeAction EaseAction(CCActionInterval action)
        {
            return action;
        }

        #endregion

        public virtual CCActionInterval CreateAction(CCGridSize size)
        {
            return new CCFadeOutTRTiles(m_fDuration, size);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            CCSize s = CCDirector.SharedDirector.WinSize;
            float aspect = s.Width / s.Height;
            var x = (int) (12 * aspect);
            int y = 12;

            CCActionInterval action = CreateAction(new CCGridSize(x, y));

            m_pOutScene.RunAction
                (
                    new CCSequence
                        (
                            EaseAction(action),
                            new CCCallFunc(Finish),
                            new CCStopGrid()
                        )
                );
        }

        public CCTransitionFadeTR() { }
        public CCTransitionFadeTR(float t, CCScene scene) : base(t, scene)
        {
            InitWithDuration(t, scene);
        }

        protected override void SceneOrder()
        {
            m_bIsInSceneOnTop = false;
        }
    }
}
