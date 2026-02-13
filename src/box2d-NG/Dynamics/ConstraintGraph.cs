using System.Collections.Generic;

namespace Box2DNG
{
    internal sealed class ConstraintGraph
    {
        internal sealed class GraphColor
        {
            public BitSet? BodySet { get; private set; }
            public readonly List<Contact> Contacts = new List<Contact>();
            public readonly List<JointHandle> Joints = new List<JointHandle>();

            public GraphColor(bool useBodySet, uint bodyCapacity)
            {
                if (useBodySet)
                {
                    BodySet = new BitSet(bodyCapacity);
                    BodySet.SetBitCountAndClear(bodyCapacity);
                }
                else
                {
                    BodySet = null;
                }
            }

            public void Reset(uint bodyCapacity)
            {
                Contacts.Clear();
                Joints.Clear();
                if (BodySet != null)
                {
                    BodySet.SetBitCountAndClear(bodyCapacity);
                }
            }
        }

        public readonly GraphColor[] Colors;

        public ConstraintGraph(uint bodyCapacity)
        {
            Colors = new GraphColor[Constants.GraphColorCount];
            for (int i = 0; i < Constants.GraphColorCount; ++i)
            {
                bool useBodySet = i != Constants.GraphOverflowIndex;
                Colors[i] = new GraphColor(useBodySet, bodyCapacity);
            }
        }

        public void Reset(uint bodyCapacity)
        {
            for (int i = 0; i < Constants.GraphColorCount; ++i)
            {
                Colors[i].Reset(bodyCapacity);
            }
        }

        public int AssignContactColor(Body bodyA, Body bodyB)
        {
            return AssignColor(bodyA, bodyB);
        }

        public int AssignJointColor(Body bodyA, Body bodyB)
        {
            return AssignColor(bodyA, bodyB);
        }

        private int AssignColor(Body bodyA, Body bodyB)
        {
            BodyType typeA = bodyA.Type;
            BodyType typeB = bodyB.Type;
            System.Diagnostics.Debug.Assert(typeA != BodyType.Static || typeB != BodyType.Static);
            bool dynamicA = typeA != BodyType.Static;
            bool dynamicB = typeB != BodyType.Static;

            if (dynamicA && dynamicB)
            {
                for (int i = 0; i < Constants.DynamicGraphColorCount; ++i)
                {
                    GraphColor color = Colors[i];
                    BitSet? bodySet = color.BodySet;
                    if (bodySet == null)
                    {
                        continue;
                    }
                    if (bodySet.GetBit((uint)bodyA.Id) || bodySet.GetBit((uint)bodyB.Id))
                    {
                        continue;
                    }

                    bodySet.SetBitGrow((uint)bodyA.Id);
                    bodySet.SetBitGrow((uint)bodyB.Id);
                    return i;
                }
            }
            else if (dynamicA)
            {
                for (int i = Constants.GraphOverflowIndex - 1; i >= 1; --i)
                {
                    GraphColor color = Colors[i];
                    BitSet? bodySet = color.BodySet;
                    if (bodySet == null)
                    {
                        continue;
                    }
                    if (bodySet.GetBit((uint)bodyA.Id))
                    {
                        continue;
                    }

                    bodySet.SetBitGrow((uint)bodyA.Id);
                    return i;
                }
            }
            else if (dynamicB)
            {
                for (int i = Constants.GraphOverflowIndex - 1; i >= 1; --i)
                {
                    GraphColor color = Colors[i];
                    BitSet? bodySet = color.BodySet;
                    if (bodySet == null)
                    {
                        continue;
                    }
                    if (bodySet.GetBit((uint)bodyB.Id))
                    {
                        continue;
                    }

                    bodySet.SetBitGrow((uint)bodyB.Id);
                    return i;
                }
            }

            return Constants.GraphOverflowIndex;
        }
    }
}
