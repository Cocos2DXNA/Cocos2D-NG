using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionProgressRadialCCW : CCTransitionProgress
    {
        public CCTransitionProgressRadialCCW(float t, CCScene scene) : base(t, scene)
        {
        }

        //OLD_TRANSITION_CREATE_FUNC(CCTransitionProgressRadialCCW)
        //TRANSITION_CREATE_FUNC(CCTransitionProgressRadialCCW)

        protected override CCProgressTimer ProgressTimerNodeWithRenderTexture(CCRenderTexture texture)
        {
            CCSize size = CCDirector.SharedDirector.WinSize;

            CCProgressTimer node = new CCProgressTimer(texture.Sprite);

            // but it is flipped upside down so we flip the sprite
            //node.Sprite.IsFlipY = true;
            node.Type = CCProgressTimerType.Radial;

            //    Return the radial type that we want to use
            node.ReverseDirection = false;
            node.Percentage = 100;
            node.Position = new CCPoint(size.Width / 2, size.Height / 2);
            node.AnchorPoint = new CCPoint(0.5f, 0.5f);

            return node;
        }
    }
}
