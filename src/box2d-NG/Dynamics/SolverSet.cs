using System.Collections.Generic;

namespace Box2DNG
{
    public sealed class SolverSet
    {
        public readonly List<Body> Bodies = new List<Body>();
        public readonly List<Island> Islands = new List<Island>();
        public readonly List<Contact> Contacts = new List<Contact>();
        public readonly List<Contact> NonTouchingContacts = new List<Contact>();
        public readonly List<JointHandle> Joints = new List<JointHandle>();
    }

    internal enum SolverSetType
    {
        Static,
        Disabled,
        Awake,
        Sleeping
    }
}
