using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionProgressInOut : CCTransitionProgress
    {
        public CCTransitionProgressInOut(float t, CCScene scene) : base(t, scene)
        {
        }

        protected override CCProgressTimer ProgressTimerNodeWithRenderTexture(CCRenderTexture texture)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;

            CCProgressTimer node = new CCProgressTimer(texture.Sprite);

            // but it is flipped upside down so we flip the sprite
            //node.Sprite.IsFlipY = true;
            node.Type = CCProgressTimerType.Bar;

            node.Midpoint = new CCPoint(0.5f, 0.5f);
            node.BarChangeRate = new CCPoint(1, 1);

            node.Percentage = 0;
            node.Position = new CCPoint(size.Width / 2, size.Height / 2);
            node.AnchorPoint = new CCPoint(0.5f, 0.5f);

            return node;
        }

        protected override void SceneOrder()
        {
            m_bIsInSceneOnTop = false;
        }

        protected override void SetupTransition()
        {
            m_pSceneToBeModified = m_pInScene;
            m_fFrom = 0;
            m_fTo = 100;
        }
    }
}
