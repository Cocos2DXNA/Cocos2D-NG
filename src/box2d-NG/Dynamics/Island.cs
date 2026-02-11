using System.Collections.Generic;

namespace Box2DNG
{
    public sealed class Island
    {
        public readonly List<Body> Bodies = new List<Body>();
        public readonly List<Contact> Contacts = new List<Contact>();
        public readonly List<JointHandle> Joints = new List<JointHandle>();
        public int Id { get; internal set; }
        public bool IsAwake { get; set; } = true;
        public int ConstraintRemoveCount { get; internal set; }
    }

    public readonly struct JointHandle
    {
        public readonly JointType Type;
        public readonly int Id;

        public JointHandle(JointType type, int id)
        {
            Type = type;
            Id = id;
        }
    }

    public enum JointType
    {
        Distance,
        Revolute,
        Prismatic,
        Wheel,
        Pulley,
        Weld,
        Motor,
        Gear,
        Rope,
        Friction
    }
}
