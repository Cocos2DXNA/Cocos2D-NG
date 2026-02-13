using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionPageTurn : CCTransitionScene
    {
        protected bool m_bBack;

        public CCTransitionPageTurn() { }
        
        /// <summary>
        /// Creates a base transition with duration and incoming scene.
        /// If back is true then the effect is reversed to appear as if the incoming 
        /// scene is being turned from left over the outgoing scene.
        /// </summary>
        public CCTransitionPageTurn (float t, CCScene scene, bool backwards)
        {
            // We can not call base here because m_bBack needs to be set first
            InitWithDuration(t, scene, backwards);
        }

        /// <summary>
        /// Creates a base transition with duration and incoming scene.
        /// If back is true then the effect is reversed to appear as if the incoming 
        /// scene is being turned from left over the outgoing scene.
        /// </summary>
        public virtual bool InitWithDuration(float t, CCScene scene, bool backwards)
        {
            // XXX: needed before [super init]
            m_bBack = backwards;

            if (base.InitWithDuration(t, scene))
            {
                // do something
            }

            return true;
        }

        public CCActionInterval ActionWithSize(CCGridSize vector)
        {
            if (m_bBack)
            {
                // Get hold of the PageTurn3DAction
                return new CCReverseTime
                    (
                        new CCPageTurn3D (m_fDuration, vector)
                    );
            }
            else
            {
                // Get hold of the PageTurn3DAction
                return new CCPageTurn3D (m_fDuration, vector);
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            CCSize s = CCDirector.SharedDirector.WinSize;
            int x, y;
            if (s.Width > s.Height)
            {
                x = 16;
                y = 12;
            }
            else
            {
                x = 12;
                y = 16;
            }

            CCActionInterval action = ActionWithSize(CCTypes.GridSize(x, y));

            if (!m_bBack)
            {
                m_pOutScene.RunAction(new CCSequence
                                          (
                                              action,
                                              new CCCallFunc(Finish),
                                              new CCStopGrid()));
            }
            else
            {
                // to prevent initial flicker
                m_pInScene.Visible = false;
                m_pInScene.RunAction(new CCSequence
                                         (
                                             new CCShow(),
                                             action,
                                             new CCCallFunc(Finish),
                                             new CCStopGrid()));
            }
        }

        protected override void SceneOrder()
        {
            m_bIsInSceneOnTop = m_bBack;
        }
    }
}
