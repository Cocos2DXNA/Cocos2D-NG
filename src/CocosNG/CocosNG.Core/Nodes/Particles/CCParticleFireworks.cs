using System.IO;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core.Nodes.Particles
{
    public class CCParticleFireworks : CCParticleSystemQuad
    {
        public CCParticleFireworks () : base(1500)
        { }

        public override bool InitWithTotalParticles(int numberOfParticles)
        {
            if (base.InitWithTotalParticles(numberOfParticles))
            {
                // duration
                m_fDuration = kCCParticleDurationInfinity;

                // Gravity Mode
                m_nEmitterMode = CCEmitterMode.Gravity;

                // Gravity Mode: gravity
                modeA.gravity = new CCPoint(0, -90);

                // Gravity Mode:  radial
                modeA.radialAccel = 0;
                modeA.radialAccelVar = 0;

                //  Gravity Mode: speed of particles
                modeA.speed = 180;
                modeA.speedVar = 50;

                // emitter position
                CCSize winSize = CCDirector.SharedDirector.WinSize;
                Position = new CCPoint(winSize.Width / 2, winSize.Height / 2);

                // angle
                m_fAngle = 90;
                m_fAngleVar = 20;

                // life of particles
                m_fLife = 3.5f;
                m_fLifeVar = 1;

                // emits per frame
                m_fEmissionRate = m_uTotalParticles / m_fLife;

                // color of particles
                m_tStartColor.R = 0.5f;
                m_tStartColor.G = 0.5f;
                m_tStartColor.B = 0.5f;
                m_tStartColor.A = 1.0f;
                m_tStartColorVar.R = 0.5f;
                m_tStartColorVar.G = 0.5f;
                m_tStartColorVar.B = 0.5f;
                m_tStartColorVar.A = 0.1f;
                m_tEndColor.R = 0.1f;
                m_tEndColor.G = 0.1f;
                m_tEndColor.B = 0.1f;
                m_tEndColor.A = 0.2f;
                m_tEndColorVar.R = 0.1f;
                m_tEndColorVar.G = 0.1f;
                m_tEndColorVar.B = 0.1f;
                m_tEndColorVar.A = 0.2f;

                // size, in pixels
                m_fStartSize = 8.0f;
                m_fStartSizeVar = 2.0f;
                m_fEndSize = kCCParticleStartSizeEqualToEndSize;

                // additive
                BlendAdditive = false;

                Texture = CCParticleExample.DefaultTexture;

                return true;
            }
            return false;
        }
    }
}
