using Microsoft.Xna.Framework;
using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionFade : CCTransitionScene
    {
        private const int kSceneFade = 2147483647;
        protected CCColor4B m_tColor;

        /// <summary>
        /// creates the transition with a duration and with an RGB color
        /// Example: FadeTransition::create(2, scene, ccc3(255,0,0); // red color
        /// </summary>
        public CCTransitionFade (float duration, CCScene scene, CCColor3B color) : base (duration, scene)
        {
            InitWithDuration(duration, scene, color);
        }

        public CCTransitionFade (float t, CCScene scene) : this (t, scene, new CCColor3B())
        {
            //return Create(t, scene, new CCColor3B());
        }

        /// <summary>
        /// initializes the transition with a duration and with an RGB color 
        /// </summary>
        protected virtual bool InitWithDuration(float duration, CCScene scene, CCColor3B color)
        {
            if (base.InitWithDuration(duration, scene))
            {
                m_tColor = new CCColor4B {R = color.R, G = color.G, B = color.B, A = 0};
            }
            return true;
        }

        protected override bool InitWithDuration(float t, CCScene scene)
        {
            InitWithDuration(t, scene, new CCColor3B(Color.Black));
            return true;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            CCLayerColor l = new CCLayerColor(m_tColor);
            m_pInScene.Visible = false;

            AddChild(l, 2, kSceneFade);
            CCNode f = GetChildByTag(kSceneFade);

            var a = (CCActionInterval) new CCSequence
                                           (
                                               new CCFadeIn (m_fDuration / 2),
                                               new CCCallFunc((HideOutShowIn)),
                                               new CCFadeOut  (m_fDuration / 2),
                                               new CCCallFunc((Finish))
                                           );

            f.RunAction(a);
        }

        public override void OnExit()
        {
            base.OnExit();
            RemoveChildByTag(kSceneFade, false);
        }
    }
}
