namespace Box2DNG
{
    public struct WheelJointData
    {
        public int Id;
        public int BodyA;
        public int BodyB;
        public bool CollideConnected;

        public Vec2 LocalAnchorA;
        public Vec2 LocalAnchorB;
        public Vec2 LocalAxisA;
        public bool EnableMotor;
        public float MotorSpeed;
        public float MaxMotorTorque;
        public float FrequencyHz;
        public float DampingRatio;
        public bool EnableLimit;
        public float LowerTranslation;
        public float UpperTranslation;

        public float PerpMass;
        public float SpringMass;
        public float MotorMass;
        public float Impulse;
        public float SpringImpulse;
        public float MotorImpulse;
        public float LowerImpulse;
        public float UpperImpulse;
        internal Softness SpringSoftness;
    }
}
