
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if IOS
using UIKit;
using CoreGraphics;
using Foundation;
#endif
namespace CocosNG.Core
{

    public class CCConfiguration
    {
        protected int m_nMaxTextureSize = 0x0D33;
        protected int m_nMaxModelviewStackDepth;
        protected bool m_bSupportsPVRTC;
        protected bool m_bSupportsNPOT;
        protected bool m_bSupportsBGRA8888;
        protected bool m_bSupportsDiscardFramebuffer;
        protected bool m_bInited;
        protected uint m_uOSVersion;
        protected int m_nMaxSamplesAllowed;
        protected string m_pGlExtensions;

        private CCConfiguration()
        { }

        public bool DisplayStats
        {
            get { return CCDirector.SharedDirector.DisplayStats; }
            set { CCDirector.SharedDirector.DisplayStats = value; }
        }

        /// <summary>
        /// Set this to true if you want to enforce graph priority for touch delegation.
        /// </summary>
        public bool UseGraphPriority { get; set; }

        /// <summary>
        /// OpenGL Max texture size.
        /// </summary>
        public int MaxTextureSize
        {
            get { return m_nMaxTextureSize; }
        }

        /// <summary>
        /// OpenGL Max Modelview Stack Depth
        /// </summary>
        public int MaxModelviewStackDepth
        {
            get { return m_nMaxModelviewStackDepth; }
        }

        /// <summary>
        /// Whether or not the GPU supports NPOT (Non Power Of Two) textures.
        /// NPOT textures have the following limitations:
        ///- They can't have mipmaps
        ///- They only accept GL_CLAMP_TO_EDGE in GL_TEXTURE_WRAP_{S,T}
        /// @since v0.99.2
        /// </summary>
        public bool IsSupportsNPOT
        {
            get { return m_bSupportsNPOT; }
        }

        /// <summary>
        /// Whether or not PVR Texture Compressed is supported
        /// </summary>
        public bool IsSupportsPVRTC
        {
            get { return m_bSupportsPVRTC; }
        }

        /// <summary>
        /// Whether or not BGRA8888 textures are supported.
        /// @since v0.99.2
        /// </summary>
        public bool IsSupportsBGRA8888
        {
            get { return m_bSupportsBGRA8888; }
        }

        /// <summary>
        /// Whether or not glDiscardFramebufferEXT is supported
        /// @since v0.99.2
        /// </summary>
        public bool IsSupportsDiscardFramebuffer
        {
            get { return m_bSupportsDiscardFramebuffer; }
        }

        /// <summary>
        /// Returns the Android Version "Release" name, on iOS it returns the MonoTouch product version.
        /// @since v0.99.5
        /// </summary>
        public virtual string OSVersion
        {
            get 
            {
                return (Environment.OSVersion.Version.ToString());
            }
        }

        /// <summary>
        /// returns whether or not an OpenGL is supported
        /// </summary>
        public bool CheckForGLExtension(string searchName)
        {
            throw new NotImplementedException();
        }

        public bool Init()
        {
            return true;
        }

        static CCConfiguration m_sharedConfiguration = new CCConfiguration();

        /// <summary>
        /// returns a shared instance of the CCConfiguration
        /// </summary>
        public static CCConfiguration SharedConfiguration
        {
            get {
            if (!m_sharedConfiguration.m_bInited)
            {
                m_sharedConfiguration.Init();
                m_sharedConfiguration.m_bInited = true;
            }

            return m_sharedConfiguration;
        }
    }
}
}
