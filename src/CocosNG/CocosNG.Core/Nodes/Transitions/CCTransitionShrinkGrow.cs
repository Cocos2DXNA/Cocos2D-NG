using System;
using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionShrinkGrow : CCTransitionScene, ICCTransitionEaseScene
    {

        public CCTransitionShrinkGrow() { }

        public CCTransitionShrinkGrow (float t, CCScene scene) : base (t, scene)
        { }

        #region ICCTransitionEaseScene Members

        public CCFiniteTimeAction EaseAction(CCActionInterval action)
        {
            return new CCEaseOut(action, 2.0f);
        }

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();

            m_pInScene.Scale = 0.001f;
            m_pOutScene.Scale = (1.0f);

            m_pInScene.AnchorPoint = new CCPoint(2 / 3.0f, 0.5f);
            m_pOutScene.AnchorPoint = new CCPoint(1 / 3.0f, 0.5f);

            CCActionInterval scaleOut = new CCScaleTo(m_fDuration, 0.01f);
            CCActionInterval scaleIn = new CCScaleTo(m_fDuration, 1.0f);

            m_pInScene.RunAction(EaseAction(scaleIn));
            m_pOutScene.RunAction
                (
                    new CCSequence
                        (
                            EaseAction(scaleOut),
                            new CCCallFunc((Finish))
                        )
                );
        }

    }
}
