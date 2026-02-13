using System.IO;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core.Nodes.Particles
{
    public class CCParticleFire : CCParticleSystemQuad
    {
        public CCParticleFire () : base (250)
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
                modeA.gravity = new CCPoint(0, 0);

                // Gravity Mode: radial acceleration
                modeA.radialAccel = 0;
                modeA.radialAccelVar = 0;

                // Gravity Mode: speed of particles
                modeA.speed = 60;
                modeA.speedVar = 20;

                // starting angle
                m_fAngle = 90;
                m_fAngleVar = 10;

                // emitter position
                CCSize winSize = CCDirector.SharedDirector.WinSize;
                SetPosition(winSize.Width / 2, 60);
                m_tPosVar = new CCPoint(40, 20);

                // life of particles
                m_fLife = 3;
                m_fLifeVar = 0.25f;


                // size, in pixels
                m_fStartSize = 54.0f;
                m_fStartSizeVar = 10.0f;
                m_fEndSize = kCCParticleStartSizeEqualToEndSize;

                // emits per frame
                m_fEmissionRate = m_uTotalParticles / m_fLife;

                // color of particles
                m_tStartColor.R = 0.76f;
                m_tStartColor.G = 0.25f;
                m_tStartColor.B = 0.12f;
                m_tStartColor.A = 1.0f;
                m_tStartColorVar.R = 0.0f;
                m_tStartColorVar.G = 0.0f;
                m_tStartColorVar.B = 0.0f;
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
                BlendAdditive = true;

                Texture = CCParticleExample.DefaultTexture;

                return true;
            }
            return false;
        }
    }
}
