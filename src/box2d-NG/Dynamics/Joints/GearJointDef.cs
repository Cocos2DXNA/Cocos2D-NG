using System;

namespace Box2DNG
{
    public sealed class GearJointDef
    {
        public object JointA { get; private set; }
        public object JointB { get; private set; }
        public float Ratio { get; private set; } = 1f;
        public bool CollideConnected { get; private set; }

        public GearJointDef(object jointA, object jointB, float ratio = 1f)
        {
            JointA = jointA ?? throw new ArgumentNullException(nameof(jointA));
            JointB = jointB ?? throw new ArgumentNullException(nameof(jointB));
            Ratio = ratio;
        }

        public GearJointDef WithRatio(float ratio)
        {
            Ratio = ratio;
            return this;
        }

        public GearJointDef WithCollideConnected(bool collideConnected = true)
        {
            CollideConnected = collideConnected;
            return this;
        }
    }
}
