using System.Collections.Generic;

namespace Box2DNG.Viewer.Samples
{
    public static class SampleCatalog
    {
        public static IReadOnlyList<ISample> All { get; } = new ISample[]
        {
            new AddPairSample(),
            new ApplyForceSample(),
            new BodyTypesSample(),
            new BreakableSample(),
            new BridgeSample(),
            new BulletTestSample(),
            new CantileverSample(),
            new CarSample(),
            new ChainSample(),
            new CharacterCollisionSample(),
            new CircleStressSample(),
            new CollisionFilteringSample(),
            new CompoundShapesSample(),
            new ConfinedSample(),
            new PyramidSample(),
            new DominosSample(),
            new EdgeShapesSample(),
            new PrismaticSample(),
            new UnstablePrismaticJointsSample(),
            new MultiplePrismaticSample(),
            new PulleysSample(),
            new TumblerSample(),
            new RevoluteSample(),
            new GearsSample(),
            new RopeSample(),
            new RopeJointSample(),
            new DistanceJointSample(),
            new WeldJointSample(),
            new FrictionJointSample(),
            new MotorJointSample(),
            new SliderCrankSample(),
            new TheoJansenSample(),
            new PinballSample(),
            new VaryingFrictionSample(),
            new VaryingRestitutionSample()
        };
    }
}
