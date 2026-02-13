using System;
using CocosNG.Core;

namespace CocosNG.Core.UI
{
    public class CCMenuItemAtlasFont : CCMenuItemLabel
    {
        /// <summary>
        /// creates a menu item from a string and atlas with a target/selector
        /// </summary>
        public static CCMenuItemAtlasFont ItemFromString(string value, string charMapFile, int itemWidth, int itemHeight, char startCharMap)
        {
            return ItemFromString(value, charMapFile, itemWidth, itemHeight, startCharMap, null, null);
        }

        /// <summary>
        ///  creates a menu item from a string and atlas. Use it with MenuItemToggle
        /// </summary>
        public static CCMenuItemAtlasFont ItemFromString(string value, string charMapFile, int itemWidth, int itemHeight, char startCharMap,
                                                         ISchedulerTarget target, Action<CCMenuItem> selector)
        {
            var pRet = new CCMenuItemAtlasFont();
            pRet.InitFromString(value, charMapFile, itemWidth, itemHeight, startCharMap, target, selector);
            return pRet;
        }

        /// <summary>
        /// initializes a menu item from a string and atlas with a target/selector
        /// </summary>
        public bool InitFromString(string value, string charMapFile, int itemWidth, int itemHeight, char startCharMap, ISchedulerTarget target,
                                   Action<CCMenuItem> selector)
        {
            // CCAssert( value != NULL && strlen(value) != 0, "value length must be greater than 0");
            var label = new CCLabelAtlas(value, charMapFile, itemWidth, itemHeight, startCharMap);
            base.InitWithLabel(label, selector);
            return true;
        }

        public CCMenuItemAtlasFont WithText(string text)
        {
            var label = m_pLabel as ICCLabelProtocol;
            if (label != null)
            {
                label.Text = text;
            }
            return this;
        }

        public CCMenuItemAtlasFont WithCharMap(string value, string charMapFile, int itemWidth, int itemHeight, char startCharMap)
        {
            InitFromString(value, charMapFile, itemWidth, itemHeight, startCharMap, null, null);
            return this;
        }
    }
}
