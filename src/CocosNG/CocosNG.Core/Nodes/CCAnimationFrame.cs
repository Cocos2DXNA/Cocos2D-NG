using CocosNG.Core;

namespace CocosNG.Core.Nodes
{
    public class CCAnimationFrame : ICCCopyable
    {
        private float m_fDelayUnits;
        private CCSpriteFrame m_pSpriteFrame;
        private PlistDictionary m_pUserInfo;

        public CCSpriteFrame SpriteFrame
        {
            get { return m_pSpriteFrame; }
        }

        public CCAnimationFrame WithSpriteFrame(CCSpriteFrame spriteFrame)
        {
            m_pSpriteFrame = spriteFrame;
            return this;
        }

        public float DelayUnits
        {
            get { return m_fDelayUnits; }
        }

        public CCAnimationFrame WithDelayUnits(float delayUnits)
        {
            m_fDelayUnits = delayUnits;
            return this;
        }

        public PlistDictionary UserInfo
        {
            get { return m_pUserInfo; }
        }

        public CCAnimationFrame WithUserInfo(PlistDictionary userInfo)
        {
            m_pUserInfo = userInfo;
            return this;
        }

		public CCAnimationFrame Copy()
		{
			return (CCAnimationFrame)Copy(null);
		}

        public object Copy(ICCCopyable pZone)
        {
            CCAnimationFrame pCopy;
            if (pZone != null)
            {
                //in case of being called at sub class
                pCopy = (CCAnimationFrame) (pZone);
            }
            else
            {
                pCopy = new CCAnimationFrame();
            }

            pCopy.InitWithSpriteFrame((CCSpriteFrame) m_pSpriteFrame.Copy(), m_fDelayUnits, m_pUserInfo);

            return pCopy;
        }

        public bool InitWithSpriteFrame(CCSpriteFrame spriteFrame, float delayUnits, PlistDictionary userInfo)
        {
            m_pSpriteFrame = spriteFrame;
            m_fDelayUnits = delayUnits;
            m_pUserInfo = userInfo;
            return true;
        }

        public CCAnimationFrame WithSpriteFrame(CCSpriteFrame spriteFrame, float delayUnits, PlistDictionary userInfo)
        {
            InitWithSpriteFrame(spriteFrame, delayUnits, userInfo);
            return this;
        }
    }
}
