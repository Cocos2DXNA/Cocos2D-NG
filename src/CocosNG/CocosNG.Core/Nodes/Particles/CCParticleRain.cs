using System.IO;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;

namespace CocosNG.Core.Nodes.Particles
{
    public class CCParticleRain : CCParticleSystemQuad
    {
        public CCParticleRain () : base(1000)
        { }

        public override bool InitWithTotalParticles(int numberOfParticles)
        {
            if (base.InitWithTotalParticles(numberOfParticles))
            {
                // duration
                m_fDuration = kCCParticleDurationInfinity;

                m_nEmitterMode = CCEmitterMode.Gravity;

                // Gravity Mode: gravity
                modeA.gravity = new CCPoint(10, -10);

                // Gravity Mode: radial
                modeA.radialAccel = 0;
                modeA.radialAccelVar = 1;

                // Gravity Mode: tagential
                modeA.tangentialAccel = 0;
                modeA.tangentialAccelVar = 1;

                // Gravity Mode: speed of particles
                modeA.speed = 130;
                modeA.speedVar = 30;

                // angle
                m_fAngle = -90;
                m_fAngleVar = 5;


                // emitter position
                CCSize winSize = CCDirector.SharedDirector.WinSize;
                Position = new CCPoint(winSize.Width / 2, winSize.Height);
                m_tPosVar = new CCPoint(winSize.Width / 2, 0);

                // life of particles
                m_fLife = 4.5f;
                m_fLifeVar = 0;

                // size, in pixels
                m_fStartSize = 4.0f;
                m_fStartSizeVar = 2.0f;
                m_fEndSize = kCCParticleStartSizeEqualToEndSize;

                // emits per second
                m_fEmissionRate = 20;

                // color of particles
                m_tStartColor.R = 0.7f;
                m_tStartColor.G = 0.8f;
                m_tStartColor.B = 1.0f;
                m_tStartColor.A = 1.0f;
                m_tStartColorVar.R = 0.0f;
                m_tStartColorVar.G = 0.0f;
                m_tStartColorVar.B = 0.0f;
                m_tStartColorVar.A = 0.0f;
                m_tEndColor.R = 0.7f;
                m_tEndColor.G = 0.8f;
                m_tEndColor.B = 1.0f;
                m_tEndColor.A = 0.5f;
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
