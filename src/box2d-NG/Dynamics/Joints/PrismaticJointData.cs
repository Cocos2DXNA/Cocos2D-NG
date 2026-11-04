using System;

namespace Box2DNG
{
    public struct PrismaticJointData
    {
        public int Id;
        public int BodyA;
        public int BodyB;
        public bool CollideConnected;
        
        public Vec2 LocalAnchorA;
        public Vec2 LocalAnchorB;
        public Vec2 LocalAxisA;
        public float ReferenceAngle;
        
        public bool EnableSpring;
        public float FrequencyHz;
        public float DampingRatio;
        public float TargetTranslation;
        
        public bool EnableLimit;
        public float LowerTranslation;
        public float UpperTranslation;
        
        public bool EnableMotor;
        public float MotorSpeed;
        public float MaxMotorForce;
        
        public float ConstraintHertz;
        public float ConstraintDampingRatio;

        // State
        public Vec2 Impulse;
        public float MotorImpulse;
        public float SpringImpulse;
        public float LowerImpulse;
        public float UpperImpulse;
        
        // Solver Temp
        public Vec2 Axis;
        public Vec2 Perp;
        public float S1, S2;
        public float A1, A2;
        public Mat22 K;
        public float AxialMass;
        internal Softness SpringSoftness;
        internal Softness ConstraintSoftness;
    }
}
