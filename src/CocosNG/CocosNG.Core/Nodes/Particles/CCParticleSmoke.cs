using System.IO;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core.Nodes.Particles
{
    public class CCParticleSmoke : CCParticleSystemQuad
    {
        public CCParticleSmoke () : base(200)
        { }

        public override bool InitWithTotalParticles(int numberOfParticles)
        {
            if (base.InitWithTotalParticles(numberOfParticles))
            {
                // duration
                m_fDuration = kCCParticleDurationInfinity;

                // Emitter mode: Gravity Mode
                m_nEmitterMode = CCEmitterMode.Gravity;

                // Gravity Mode: gravity
                modeA.gravity = new CCPoint(0, 0);

                // Gravity Mode: radial acceleration
                modeA.radialAccel = 0;
                modeA.radialAccelVar = 0;

                // Gravity Mode: speed of particles
                modeA.speed = 25;
                modeA.speedVar = 10;

                // angle
                m_fAngle = 90;
                m_fAngleVar = 5;

                // emitter position
                CCSize winSize = CCDirector.SharedDirector.WinSize;
                Position = new CCPoint(winSize.Width / 2, 0);
                m_tPosVar = new CCPoint(20, 0);

                // life of particles
                m_fLife = 4;
                m_fLifeVar = 1;

                // size, in pixels
                m_fStartSize = 60.0f;
                m_fStartSizeVar = 10.0f;
                m_fEndSize = kCCParticleStartSizeEqualToEndSize;

                // emits per frame
                m_fEmissionRate = m_uTotalParticles / m_fLife;

                // color of particles
                m_tStartColor.R = 0.8f;
                m_tStartColor.G = 0.8f;
                m_tStartColor.B = 0.8f;
                m_tStartColor.A = 1.0f;
                m_tStartColorVar.R = 0.02f;
                m_tStartColorVar.G = 0.02f;
                m_tStartColorVar.B = 0.02f;
                m_tStartColorVar.A = 0.0f;
                m_tEndColor.R = 0.0f;
                m_tEndColor.G = 0.0f;
                m_tEndColor.B = 0.0f;
                m_tEndColor.A = 1.0f;
                m_tEndColorVar.R = 0.0f;
                m_tEndColorVar.G = 0.0f;
                m_tEndColorVar.B = 0.0f;
                m_tEndColorVar.A = 0.0f;

                // additive
                BlendAdditive = false;

                Texture = CCParticleExample.DefaultTexture;

                return true;
            }
            return false;
        }
    }
}
