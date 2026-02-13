using System.IO;
using Microsoft.Xna.Framework.Graphics;
using CocosNG.Core;
using CocosNG.Core.Primitives;
using CocosNG.Core.Resources;

namespace CocosNG.Core.Nodes.Particles
{
    public class CCParticleSnow : CCParticleSystemQuad
    {
        public CCParticleSnow () : base(700)
        { }

        public override bool InitWithTotalParticles(int numberOfParticles)
        {
            if (base.InitWithTotalParticles(numberOfParticles))
            {
                // duration
                m_fDuration = kCCParticleDurationInfinity;

                // set gravity mode.
                m_nEmitterMode = CCEmitterMode.Gravity;

                // Gravity Mode: gravity
                modeA.gravity = new CCPoint(0, -1);

                // Gravity Mode: speed of particles
                modeA.speed = 5;
                modeA.speedVar = 1;

                // Gravity Mode: radial
                modeA.radialAccel = 0;
                modeA.radialAccelVar = 1;

                // Gravity mode: tagential
                modeA.tangentialAccel = 0;
                modeA.tangentialAccelVar = 1;

                // emitter position
                CCSize winSize = CCDirector.SharedDirector.WinSize;
                Position = new CCPoint(winSize.Width / 2, winSize.Height + 10);
                m_tPosVar = new CCPoint(winSize.Width / 2, 0);

                // angle
                m_fAngle = -90;
                m_fAngleVar = 5;

                // life of particles
                m_fLife = 45;
                m_fLifeVar = 15;

                // size, in pixels
                m_fStartSize = 10.0f;
                m_fStartSizeVar = 5.0f;
                m_fEndSize = kCCParticleStartSizeEqualToEndSize;

                // emits per second
                m_fEmissionRate = 10;

                // color of particles
                m_tStartColor.R = 1.0f;
                m_tStartColor.G = 1.0f;
                m_tStartColor.B = 1.0f;
                m_tStartColor.A = 1.0f;
                m_tStartColorVar.R = 0.0f;
                m_tStartColorVar.G = 0.0f;
                m_tStartColorVar.B = 0.0f;
                m_tStartColorVar.A = 0.0f;
                m_tEndColor.R = 1.0f;
                m_tEndColor.G = 1.0f;
                m_tEndColor.B = 1.0f;
                m_tEndColor.A = 0.0f;
                m_tEndColorVar.R = 0.0f;
                m_tEndColorVar.G = 0.0f;
                m_tEndColorVar.B = 0.0f;
                m_tEndColorVar.A = 0.0f;

                // additive
                BlendAdditive = false;

                Texture = CCFireParticleImage.DefaultTexture;

                return true;
            }
            return false;
        }
    }
}
