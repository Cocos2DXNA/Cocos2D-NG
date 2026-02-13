using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionZoomFlipY : CCTransitionSceneOriented
    {
        public CCTransitionZoomFlipY() { }
        
        public CCTransitionZoomFlipY (float t, CCScene s, CCTransitionOrientation o) : base (t, s, o)
        { }

        public override void OnEnter()
        {
            base.OnEnter();

            CCActionInterval inA, outA;
            m_pInScene.Visible = false;

            float inDeltaZ, inAngleZ;
            float outDeltaZ, outAngleZ;

            if (m_eOrientation == CCTransitionOrientation.UpOver)
            {
                inDeltaZ = 90;
                inAngleZ = 270;
                outDeltaZ = 90;
                outAngleZ = 0;
            }
            else
            {
                inDeltaZ = -90;
                inAngleZ = 90;
                outDeltaZ = -90;
                outAngleZ = 0;
            }

            inA = new CCSequence
                (
                    new CCDelayTime (m_fDuration / 2),
                    new CCSpawn
                        (
                            new CCOrbitCamera(m_fDuration / 2, 1, 0, inAngleZ, inDeltaZ, 90, 0),
                            new CCScaleTo(m_fDuration / 2, 1),
                            new CCShow()
                        ),
                    new CCCallFunc(Finish)
                );

            outA = new CCSequence
                (
                    new CCSpawn
                        (
                            new CCOrbitCamera(m_fDuration / 2, 1, 0, outAngleZ, outDeltaZ, 90, 0),
                            new CCScaleTo(m_fDuration / 2, 0.5f)
                        ),
                    new CCHide(),
                    new CCDelayTime (m_fDuration / 2)
                );

            m_pInScene.Scale = 0.5f;
            m_pInScene.RunAction(inA);
            m_pOutScene.RunAction(outA);
        }
    }
}
