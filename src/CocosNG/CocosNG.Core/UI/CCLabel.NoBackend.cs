using System;

namespace CocosNG.Core.UI
{
    // Placeholder font rasterizer for CCLabel. cocos2d-xna shipped one backend per platform
    // (GDI+, CoreGraphics, Android, WinRT, PSM). None has been chosen for Cocos2D-NG yet, so
    // runtime-rasterized labels fail loudly. CCLabelTTF and CCLabelBMFont do not use this path.
    public partial class CCLabel
    {
        private const string NoBackendMessage =
            "CCLabel has no font rasterizer backend on this platform yet. Use CCLabelTTF or CCLabelBMFont.";

        private string CreateFont(string fontName, float fontSize, CCRawList<char> charset)
        {
            throw new NotSupportedException(NoBackendMessage);
        }

        private float GetFontHeight()
        {
            throw new NotSupportedException(NoBackendMessage);
        }

        private CCSize GetMeasureString(string text)
        {
            throw new NotSupportedException(NoBackendMessage);
        }

        private KerningInfo GetKerningInfo(char ch)
        {
            throw new NotSupportedException(NoBackendMessage);
        }

        private unsafe byte* GetBitmapData(string s, out int stride)
        {
            throw new NotSupportedException(NoBackendMessage);
        }
    }
}
