using System.Collections.Generic;
using System.Diagnostics;
using CocosNG.Core;

namespace CocosNG.Core.Nodes
{
    public class CCAnimation : ICCCopyable
    {
        protected bool m_bRestoreOriginalFrame;
        protected float m_fDelayPerUnit;
        protected float m_fTotalDelayUnits;
        protected List<CCAnimationFrame> m_pFrames;
        protected uint m_uLoops;

        public CCAnimation() : this (new List<CCSpriteFrame>(), 0)
        { }

        public CCAnimation(CCSpriteSheet cs, string[] frames, float delay)
        {
            List<CCSpriteFrame> l = new List<CCSpriteFrame>();
            foreach(string f in frames) 
            {
                CCSpriteFrame cf = cs[f];
                if (cf != null)
                {
                    l.Add(cs[f]);
                }
            }
            InitWithSpriteFrames(l, delay);
        }

        public CCAnimation(CCSpriteSheet cs, float delay)
        {
            InitWithSpriteFrames(cs.Frames, delay);
        }

        public CCAnimation (List<CCSpriteFrame> frames, float delay)
        {
            InitWithSpriteFrames(frames, delay);
        }
        
        public CCAnimation (List<CCAnimationFrame> arrayOfAnimationFrameNames, float delayPerUnit, uint loops)
        {
            InitWithAnimationFrames(arrayOfAnimationFrameNames, delayPerUnit, loops);
        }

        public float Duration
        {
            get { return m_fTotalDelayUnits * m_fDelayPerUnit; }
        }

        public float DelayPerUnit
        {
            get { return m_fDelayPerUnit; }
            set { m_fDelayPerUnit = value; }
        }

        public CCAnimation WithDelayPerUnit(float delayPerUnit)
        {
            DelayPerUnit = delayPerUnit;
            return this;
        }

        public List<CCAnimationFrame> Frames
        {
            get { return m_pFrames; }
        }

        public bool RestoreOriginalFrame
        {
            get { return m_bRestoreOriginalFrame; }
            set { m_bRestoreOriginalFrame = value; }
        }

        public CCAnimation WithRestoreOriginalFrame(bool restore)
        {
            RestoreOriginalFrame = restore;
            return this;
        }

        public uint Loops
        {
            get { return m_uLoops; }
            set { m_uLoops = value; }
        }

        public CCAnimation WithLoops(uint loops)
        {
            Loops = loops;
            return this;
        }

        public float TotalDelayUnits
        {
            get { return m_fTotalDelayUnits; }
        }

        protected virtual bool InitWithSpriteFrames(List<CCSpriteFrame> pFrames, float delay)
        {
            if (pFrames != null)
            {/*
                foreach (object frame in pFrames)
                {
                    Debug.Assert(frame is CCSpriteFrame, "element type is wrong!");
                }
              */
            }

            m_uLoops = 1;
            m_fDelayPerUnit = delay;
            m_pFrames = new List<CCAnimationFrame>();

            if (pFrames != null)
            {
                foreach (CCSpriteFrame pObj in pFrames)
                {
                    var frame = (CCSpriteFrame) pObj;
                    var animFrame = new CCAnimationFrame();
                    animFrame.InitWithSpriteFrame(frame, 1, null);
                    m_pFrames.Add(animFrame);

                    m_fTotalDelayUnits++;
                }
            }

            return true;
        }

        public CCAnimation WithSpriteFrames(List<CCSpriteFrame> frames, float delay)
        {
            InitWithSpriteFrames(frames, delay);
            return this;
        }

        protected virtual bool InitWithAnimationFrames(List<CCAnimationFrame> arrayOfAnimationFrames, float delayPerUnit, uint loops)
        {
            if (arrayOfAnimationFrames != null)
            {/*
                foreach (object frame in arrayOfAnimationFrames)
                {
                    Debug.Assert(frame is CCAnimationFrame, "element type is wrong!");
                }
              */
            }

            m_fDelayPerUnit = delayPerUnit;
            m_uLoops = loops;

            m_pFrames = new List<CCAnimationFrame>(arrayOfAnimationFrames);

            foreach (CCAnimationFrame pObj in m_pFrames)
            {
                var animFrame = (CCAnimationFrame) pObj;
                m_fTotalDelayUnits += animFrame.DelayUnits;
            }

            return true;
        }

        public CCAnimation WithAnimationFrames(List<CCAnimationFrame> frames, float delayPerUnit, uint loops)
        {
            InitWithAnimationFrames(frames, delayPerUnit, loops);
            return this;
        }

        public void AddSprite(CCSprite sprite)
        {
            CCSpriteFrame f = new CCSpriteFrame(sprite.Texture, new CCRect(0, 0, sprite.ContentSize.Width, sprite.ContentSize.Height));
            AddSpriteFrame(f);
        }

        public CCAnimation AddSpriteAnd(CCSprite sprite)
        {
            AddSprite(sprite);
            return this;
        }

        public void AddSpriteFrame(CCSpriteFrame pFrame)
        {
            var animFrame = new CCAnimationFrame();
            animFrame.InitWithSpriteFrame(pFrame, 1.0f, null);
            m_pFrames.Add(animFrame);

            // update duration
            m_fTotalDelayUnits++;
        }

        public CCAnimation AddSpriteFrameAnd(CCSpriteFrame frame)
        {
            AddSpriteFrame(frame);
            return this;
        }

        public void AddSpriteFrameWithFileName(string pszFileName)
        {
            CCTexture2D pTexture = CCTextureCache.SharedTextureCache.AddImage(pszFileName);
            CCRect rect = CCRect.Zero;
            rect.Size = pTexture.ContentSize;
            CCSpriteFrame pFrame = new CCSpriteFrame(pTexture, rect);
            AddSpriteFrame(pFrame);
        }

        public CCAnimation AddSpriteFrameWithFileNameAnd(string fileName)
        {
            AddSpriteFrameWithFileName(fileName);
            return this;
        }

        public void AddSpriteFrameWithTexture(CCTexture2D pobTexture, CCRect rect)
        {
            CCSpriteFrame pFrame = new CCSpriteFrame(pobTexture, rect);
            AddSpriteFrame(pFrame);
        }

        public CCAnimation AddSpriteFrameWithTextureAnd(CCTexture2D texture, CCRect rect)
        {
            AddSpriteFrameWithTexture(texture, rect);
            return this;
        }

		public CCAnimation Copy()
		{
			return (CCAnimation)Copy(null);
		}

        public object Copy(ICCCopyable pZone)
        {
            CCAnimation pCopy = null;
            if (pZone != null)
            {
                //in case of being called at sub class
                pCopy = (CCAnimation) (pZone);
            }
            else
            {
                pCopy = new CCAnimation();
            }

            pCopy.InitWithAnimationFrames(m_pFrames, m_fDelayPerUnit, m_uLoops);
            pCopy.RestoreOriginalFrame = m_bRestoreOriginalFrame;

            return pCopy;
        }
    }
}
