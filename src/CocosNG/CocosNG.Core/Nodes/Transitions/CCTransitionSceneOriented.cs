using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionSceneOriented : CCTransitionScene
    {
        protected CCTransitionOrientation m_eOrientation;

        public CCTransitionSceneOriented() { }

        /// <summary>
        /// creates a base transition with duration and incoming scene
        /// </summary>
        public CCTransitionSceneOriented (float t, CCScene scene, CCTransitionOrientation orientation) : base (t, scene)
        {
            m_eOrientation = orientation;
        }

        /// <summary>
        /// initializes a transition with duration and incoming scene
        /// </summary>
        public virtual bool InitWithDuration(float t, CCScene scene, CCTransitionOrientation orientation)
        {
            if (base.InitWithDuration(t, scene))
            {
                m_eOrientation = orientation;
            }

            return true;
        }
    }
}
