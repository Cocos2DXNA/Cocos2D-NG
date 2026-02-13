using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionSplitCols : CCTransitionScene, ICCTransitionEaseScene
    {
        #region ICCTransitionEaseScene Members

        public virtual CCFiniteTimeAction EaseAction(CCActionInterval action)
        {
            return new CCEaseInOut(action, 3.0f);
        }

        #endregion

        public virtual CCActionInterval Action()
        {
            return new CCSplitCols(m_fDuration / 2.0f, 3);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_pInScene.Visible = false;

            CCActionInterval split = Action();
            CCActionInterval seq = new CCSequence(
                split,
                new CCCallFunc((HideOutShowIn)),
                split.Reverse()
                );

            RunAction(
                new CCSequence(
                    EaseAction(seq),
                    new CCCallFunc(Finish),
                    new CCStopGrid()
                    )
                );
        }

        public CCTransitionSplitCols() { }

        public CCTransitionSplitCols(float t, CCScene scene) : base(t, scene)
        {
            InitWithDuration(t, scene);
        }
    }
}
