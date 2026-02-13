using System.IO;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core.Nodes.Particles
{
    public class CCParticleMeteor : CCParticleSystemQuad
    {
        public CCParticleMeteor () : base(150)
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
                modeA.gravity = new CCPoint(-200, 200);

                // Gravity Mode: speed of particles
                modeA.speed = 15;
                modeA.speedVar = 5;

                // Gravity Mode: radial
                modeA.radialAccel = 0;
                modeA.radialAccelVar = 0;

                // Gravity Mode: tagential
                modeA.tangentialAccel = 0;
                modeA.tangentialAccelVar = 0;

                // angle
                m_fAngle = 90;
                m_fAngleVar = 360;

                // emitter position
                CCSize winSize = CCDirector.SharedDirector.WinSize;
                Position = new CCPoint(winSize.Width / 2, winSize.Height / 2);
                m_tPosVar = CCPoint.Zero;

                // life of particles
                m_fLife = 2;
                m_fLifeVar = 1;

                // size, in pixels
                m_fStartSize = 60.0f;
                m_fStartSizeVar = 10.0f;
                m_fEndSize = kCCParticleStartSizeEqualToEndSize;

                // emits per second
                m_fEmissionRate = m_uTotalParticles / m_fLife;

                // color of particles
                m_tStartColor.R = 0.2f;
                m_tStartColor.G = 0.4f;
                m_tStartColor.B = 0.7f;
                m_tStartColor.A = 1.0f;
                m_tStartColorVar.R = 0.0f;
                m_tStartColorVar.G = 0.0f;
                m_tStartColorVar.B = 0.2f;
                m_tStartColorVar.A = 0.1f;
                m_tEndColor.R = 0.0f;
                m_tEndColor.G = 0.0f;
                m_tEndColor.B = 0.0f;
                m_tEndColor.A = 1.0f;
                m_tEndColorVar.R = 0.0f;
                m_tEndColorVar.G = 0.0f;
                m_tEndColorVar.B = 0.0f;
                m_tEndColorVar.A = 0.0f;

                // additive
                BlendAdditive = true;

                Texture = CCParticleExample.DefaultTexture;

                return true;
            }
            return false;
        }
    }
}
