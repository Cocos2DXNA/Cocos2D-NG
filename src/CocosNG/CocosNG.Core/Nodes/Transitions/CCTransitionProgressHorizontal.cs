using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionProgressHorizontal : CCTransitionProgress
    {
        public CCTransitionProgressHorizontal(float t, CCScene scene) : base(t, scene)
        {
        }

        protected override CCProgressTimer ProgressTimerNodeWithRenderTexture(CCRenderTexture texture)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;

            CCProgressTimer node = new CCProgressTimer(texture.Sprite);

            // but it is flipped upside down so we flip the sprite
            //node.Sprite.IsFlipY = true;
            node.Type = CCProgressTimerType.Bar;

            node.Midpoint = new CCPoint(1, 0);
            node.BarChangeRate = new CCPoint(1, 0);

            node.Percentage = 100;
            node.Position = new CCPoint(size.Width / 2, size.Height / 2);
            node.AnchorPoint = new CCPoint(0.5f, 0.5f);

            return node;
        }
    }
}
