using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionCrossFade : CCTransitionScene
    {
        private const int kSceneFade = int.MaxValue;

        public CCTransitionCrossFade (float t, CCScene scene) : base (t, scene)
        {  }

        public override void Draw()
        {
            // override draw since both scenes (textures) are rendered in 1 scene
        }

        public override void OnEnter()
        {
            base.OnEnter();

            // create a transparent color layer
            // in which we are going to add our rendertextures
            var color = new CCColor4B(0, 0, 0, 0);
            CCSize size = CCDirector.SharedDirector.WinSize;
            CCLayerColor layer = new CCLayerColor(color);

            // create the first render texture for inScene
            CCRenderTexture inTexture = new CCRenderTexture((int) size.Width, (int) size.Height);

            if (null == inTexture)
            {
                return;
            }

            inTexture.Sprite.AnchorPoint = new CCPoint(0.5f, 0.5f);
            inTexture.Position = new CCPoint(size.Width / 2, size.Height / 2);
            inTexture.AnchorPoint = new CCPoint(0.5f, 0.5f);

            //  render inScene to its texturebuffer
            inTexture.Begin();
            m_pInScene.Visit();
            inTexture.End();

            // create the second render texture for outScene
            CCRenderTexture outTexture = new CCRenderTexture((int) size.Width, (int) size.Height);
            outTexture.Sprite.AnchorPoint = new CCPoint(0.5f, 0.5f);
            outTexture.Position = new CCPoint(size.Width / 2, size.Height / 2);
            outTexture.AnchorPoint = new CCPoint(0.5f, 0.5f);

            //  render outScene to its texturebuffer
            outTexture.Begin();
            m_pOutScene.Visit();
            outTexture.End();

            // create blend functions

            var blend1 = new CCBlendFunc(CCOGLES.GL_ONE, CCOGLES.GL_ONE); // inScene will lay on background and will not be used with alpha
            var blend2 = CCBlendFunc.NonPremultiplied; // we are going to blend outScene via alpha 

            // set blendfunctions
            inTexture.Sprite.BlendFunc = blend1;
            outTexture.Sprite.BlendFunc = blend2;

            // add render textures to the layer
            layer.AddChild(inTexture);
            layer.AddChild(outTexture);

            // initial opacity:
            inTexture.Sprite.Opacity = 255;
            outTexture.Sprite.Opacity = 255;

            // create the blend action
            CCAction layerAction = new CCSequence
                (
                    new CCFadeTo (m_fDuration, 0),
                    new CCCallFunc((HideOutShowIn)),
                    new CCCallFunc((Finish))
                );


            //// run the blend action
            outTexture.Sprite.RunAction(layerAction);

            // add the layer (which contains our two rendertextures) to the scene
            AddChild(layer, 2, kSceneFade);
        }

        public override void OnExit()
        {
            // remove our layer and release all containing objects 
            RemoveChildByTag(kSceneFade, false);
            base.OnExit();
        }

    }
}
