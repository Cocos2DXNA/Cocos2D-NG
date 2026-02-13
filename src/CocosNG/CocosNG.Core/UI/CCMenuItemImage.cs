using System;
using CocosNG.Core;

namespace CocosNG.Core.UI
{
    public class CCMenuItemImage : CCMenuItemSprite
    {
        public CCMenuItemImage() : this(null, null, null, null)
        {
        }

        public CCMenuItemImage(string normalImage, string selectedImage)
            :this(normalImage, selectedImage, null, null)
        {
        }

        public CCMenuItemImage(string normalImage, string selectedImage, Action<CCMenuItem> selector)
            :this(normalImage, selectedImage, null, selector)
        {
        }

        public CCMenuItemImage(string normalImage, string selectedImage, string disabledImage, Action<CCMenuItem> selector)
            : base(selector)
        {

            if (!string.IsNullOrEmpty(normalImage))
            {
                NormalImage = new CCSprite(normalImage);
            }

            if (!string.IsNullOrEmpty(selectedImage))
            {
                SelectedImage = new CCSprite(selectedImage);
            }

            if (!string.IsNullOrEmpty(disabledImage))
            {
                DisabledImage = new CCSprite(disabledImage);
            }
        }

        public CCMenuItemImage(string normalImage, string selectedImage, string disabledImage)
            : this(normalImage, selectedImage, disabledImage, null)
        {
        }

        public void SetNormalSpriteFrame(CCSpriteFrame frame)
        {
            NormalImage = new CCSprite(frame);
        }

        public void SetSelectedSpriteFrame(CCSpriteFrame frame)
        {
            SelectedImage = new CCSprite(frame);
        }

        public void SetDisabledSpriteFrame(CCSpriteFrame frame)
        {
            DisabledImage = new CCSprite(frame);
        }

        public CCMenuItemImage WithNormalImage(string normalImage)
        {
            NormalImage = string.IsNullOrEmpty(normalImage) ? null : new CCSprite(normalImage);
            return this;
        }

        public CCMenuItemImage WithSelectedImage(string selectedImage)
        {
            SelectedImage = string.IsNullOrEmpty(selectedImage) ? null : new CCSprite(selectedImage);
            return this;
        }

        public CCMenuItemImage WithDisabledImage(string disabledImage)
        {
            DisabledImage = string.IsNullOrEmpty(disabledImage) ? null : new CCSprite(disabledImage);
            return this;
        }

        public CCMenuItemImage WithNormalSpriteFrame(CCSpriteFrame frame)
        {
            SetNormalSpriteFrame(frame);
            return this;
        }

        public CCMenuItemImage WithSelectedSpriteFrame(CCSpriteFrame frame)
        {
            SetSelectedSpriteFrame(frame);
            return this;
        }

        public CCMenuItemImage WithDisabledSpriteFrame(CCSpriteFrame frame)
        {
            SetDisabledSpriteFrame(frame);
            return this;
        }
    }
}
