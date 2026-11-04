using System;

namespace Box2DNG
{
    public sealed class GearJoint
    {
        internal World World;
        internal int Id;
        internal int Index;

        internal GearJoint(World world, int id, int index)
        {
            World = world;
            Id = id;
            Index = index;
        }

        public Body BodyA => World.GetBody(World._gearJointsData[Index].BodyA);
        public Body BodyB => World.GetBody(World._gearJointsData[Index].BodyB);
        public Body BodyC => World.GetBody(World._gearJointsData[Index].BodyC);
        public Body BodyD => World.GetBody(World._gearJointsData[Index].BodyD);
        public float Ratio => World._gearJointsData[Index].Ratio;
        public bool CollideConnected => World._gearJointsData[Index].CollideConnected;
    }
}
