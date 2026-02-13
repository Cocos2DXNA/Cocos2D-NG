using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionRotoZoom : CCTransitionScene
    {
        public CCTransitionRotoZoom()
        {
        }

        public CCTransitionRotoZoom(float t, CCScene scene) : base(t, scene)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_pInScene.Scale = 0.001f;
            m_pOutScene.Scale = 1.0f;

            m_pInScene.AnchorPoint = new CCPoint(0.5f, 0.5f);
            m_pOutScene.AnchorPoint = new CCPoint(0.5f, 0.5f);

            CCActionInterval rotozoom = new CCSequence(
                new CCSpawn(
                    new CCScaleBy(m_fDuration / 2, 0.001f),
                    new CCRotateBy(m_fDuration / 2, 360 * 2)
                    ),
                new CCDelayTime(m_fDuration / 2)
                );

            m_pOutScene.RunAction(rotozoom);
            m_pInScene.RunAction(
                new CCSequence(
                    rotozoom.Reverse(),
                    new CCCallFunc((Finish))
                    )
                );
        }
    }
}
