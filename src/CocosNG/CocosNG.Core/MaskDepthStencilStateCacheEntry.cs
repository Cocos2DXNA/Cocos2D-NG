using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core
{
        private struct MaskDepthStencilStateCacheEntry
        {
            public DepthStencilState Clear;
            public DepthStencilState ClearInvert;
            public DepthStencilState DrawMask;
            public DepthStencilState DrawMaskInvert;
            public DepthStencilState DrawContent;
            public DepthStencilState DrawContentDepth;

            public DepthStencilState GetClearState(int layer, bool inverted)
            {
                DepthStencilState result = inverted ? ClearInvert : Clear;

                if (result == null)
                {
                    int maskLayer = 1 << layer;

                    result = new DepthStencilState()
                    {
                        DepthBufferEnable = false,

                        StencilEnable = true,

                        StencilFunction = CompareFunction.Never,

                        StencilMask = maskLayer,
                        StencilWriteMask = maskLayer,
                        ReferenceStencil = maskLayer,

                        StencilFail = !inverted ? StencilOperation.Zero : StencilOperation.Replace
                    };

                    if (inverted)
                    {
                        ClearInvert = result;
                    }
                    else
                    {
                        Clear = result;
                    }
                }

                return result;
            }

            public DepthStencilState GetDrawMaskState(int layer, bool inverted)
            {
                DepthStencilState result = inverted ? DrawMaskInvert : DrawMask;

                if (result == null)
                {
                    int maskLayer = 1 << layer;

                    result = new DepthStencilState()
                    {
                        DepthBufferEnable = false,

                        StencilEnable = true,

                        StencilFunction = CompareFunction.Never,

                        StencilMask = maskLayer,
                        StencilWriteMask = maskLayer,
                        ReferenceStencil = maskLayer,

                        StencilFail = !inverted ? StencilOperation.Replace : StencilOperation.Zero,
                    };

                    if (inverted)
                    {
                        DrawMaskInvert = result;
                    }
                    else
                    {
                        DrawMask = result;
                    }
                }

                return result;
            }

            public DepthStencilState GetDrawContentState(int layer, bool depth)
            {
                DepthStencilState result = depth ? DrawContentDepth : DrawContent;

                if (result == null)
                {
                    int maskLayer = 1 << layer;
                    int maskLayerL = maskLayer - 1;
                    int maskLayerLe = maskLayer | maskLayerL;

                    result = new DepthStencilState()
                    {
                        DepthBufferEnable = _maskSavedStencilStates[_maskLayer].DepthBufferEnable,

                        StencilEnable = true,

                        StencilMask = maskLayerLe,
                        StencilWriteMask = 0,
                        ReferenceStencil = maskLayerLe,

                        StencilFunction = CompareFunction.Equal,

                        StencilPass = StencilOperation.Keep,
                        StencilFail = StencilOperation.Keep,
                    };

                    if (depth)
                    {
                        DrawContentDepth = result;
                    }
                    else
                    {
                        DrawContent = result;
                    }
                }

                return result;
            }
        }
}
