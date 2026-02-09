using System;

namespace Box2DNG
{
    public sealed partial class World
    {
        private readonly WorldDef _def;
        private readonly WorldEvents _events = new WorldEvents();
        private readonly BroadPhase<Fixture> _broadPhase = new BroadPhase<Fixture>();
        private readonly System.Collections.Generic.List<Body> _bodies = new System.Collections.Generic.List<Body>();
        private readonly System.Collections.Generic.List<Contact> _contacts = new System.Collections.Generic.List<Contact>();
        private readonly System.Collections.Generic.Dictionary<ContactKey, Contact> _contactMap = new System.Collections.Generic.Dictionary<ContactKey, Contact>();
        private readonly System.Collections.Generic.HashSet<ContactKey> _sensorPairs = new System.Collections.Generic.HashSet<ContactKey>();
        private readonly System.Collections.Generic.Dictionary<ContactKey, (object? A, object? B)> _sensorUserData = new System.Collections.Generic.Dictionary<ContactKey, (object? A, object? B)>();
        private readonly System.Collections.Generic.List<DistanceJoint> _distanceJoints = new System.Collections.Generic.List<DistanceJoint>();
        private readonly System.Collections.Generic.List<RevoluteJoint> _revoluteJoints = new System.Collections.Generic.List<RevoluteJoint>();
        private readonly System.Collections.Generic.List<PrismaticJoint> _prismaticJoints = new System.Collections.Generic.List<PrismaticJoint>();
        private readonly System.Collections.Generic.List<WheelJoint> _wheelJoints = new System.Collections.Generic.List<WheelJoint>();
        private readonly System.Collections.Generic.List<PulleyJoint> _pulleyJoints = new System.Collections.Generic.List<PulleyJoint>();
        private readonly System.Collections.Generic.List<WeldJoint> _weldJoints = new System.Collections.Generic.List<WeldJoint>();
        private readonly System.Collections.Generic.List<MotorJoint> _motorJoints = new System.Collections.Generic.List<MotorJoint>();
        private readonly System.Collections.Generic.List<GearJoint> _gearJoints = new System.Collections.Generic.List<GearJoint>();
        private readonly System.Collections.Generic.List<RopeJoint> _ropeJoints = new System.Collections.Generic.List<RopeJoint>();
        private readonly System.Collections.Generic.List<FrictionJoint> _frictionJoints = new System.Collections.Generic.List<FrictionJoint>();
        private float _prevTimeStep;
        private readonly SolverPipeline _solverPipeline;
        private readonly ContactSolver _contactSolver;
        private readonly ContactSolverSimd _contactSolverSimd;
        private ContactSolverStats _lastContactSolverStats = new ContactSolverStats();
        private bool _islandsDirty = true;
        private readonly System.Collections.Generic.HashSet<int> _dirtyIslandIds = new System.Collections.Generic.HashSet<int>();
        private readonly System.Collections.Generic.Dictionary<Body, int> _bodyIslandIds = new System.Collections.Generic.Dictionary<Body, int>();

        public World(WorldDef def)
        {
            _def = def ?? throw new ArgumentNullException(nameof(def));
            _solverPipeline = new SolverPipeline(this);
            _contactSolver = new ContactSolver(this);
            _contactSolverSimd = new ContactSolverSimd(this);
        }

        public readonly struct ContactSolverStats
        {
            public readonly int SinglePointConstraints;
            public readonly int TwoPointConstraints;
            public readonly int ScalarConstraints;
            public readonly int Colors;
            public readonly int SimdBatches;
            public readonly int SimdLanes;

            public ContactSolverStats(
                int singlePointConstraints,
                int twoPointConstraints,
                int scalarConstraints,
                int colors,
                int simdBatches,
                int simdLanes)
            {
                SinglePointConstraints = singlePointConstraints;
                TwoPointConstraints = twoPointConstraints;
                ScalarConstraints = scalarConstraints;
                Colors = colors;
                SimdBatches = simdBatches;
                SimdLanes = simdLanes;
            }
        }

        public Vec2 Gravity => _def.Gravity;

        public WorldEvents Events => _events;

        public System.Collections.Generic.IReadOnlyList<Body> Bodies => _bodies;

        public System.Collections.Generic.IReadOnlyList<Contact> Contacts => _contacts;
        public System.Collections.Generic.IReadOnlyList<DistanceJoint> DistanceJoints => _distanceJoints;
        public System.Collections.Generic.IReadOnlyList<RevoluteJoint> RevoluteJoints => _revoluteJoints;
        public System.Collections.Generic.IReadOnlyList<PrismaticJoint> PrismaticJoints => _prismaticJoints;
        public System.Collections.Generic.IReadOnlyList<WheelJoint> WheelJoints => _wheelJoints;
        public System.Collections.Generic.IReadOnlyList<PulleyJoint> PulleyJoints => _pulleyJoints;
        public System.Collections.Generic.IReadOnlyList<WeldJoint> WeldJoints => _weldJoints;
        public System.Collections.Generic.IReadOnlyList<MotorJoint> MotorJoints => _motorJoints;
        public System.Collections.Generic.IReadOnlyList<GearJoint> GearJoints => _gearJoints;
        public System.Collections.Generic.IReadOnlyList<RopeJoint> RopeJoints => _ropeJoints;
        public System.Collections.Generic.IReadOnlyList<FrictionJoint> FrictionJoints => _frictionJoints;
        public System.Collections.Generic.IReadOnlyList<Island> LastIslands => _lastIslands;
        public SolverSet AwakeSet => _awakeSet;
        public SolverSet SleepingSet => _sleepingSet;
        public ContactSolverStats LastContactSolverStats => _lastContactSolverStats;
        public int? GetIslandId(Body body)
        {
            if (_bodyIslandIds.TryGetValue(body, out int id))
            {
                return id;
            }
            return null;
        }

        private System.Collections.Generic.List<Island> _lastIslands = new System.Collections.Generic.List<Island>();
        private readonly SolverSet _awakeSet = new SolverSet();
        private readonly SolverSet _sleepingSet = new SolverSet();
        private readonly System.Collections.Generic.Dictionary<Body, System.Collections.Generic.List<Contact>> _bodyContacts =
            new System.Collections.Generic.Dictionary<Body, System.Collections.Generic.List<Contact>>();
        private readonly System.Collections.Generic.Dictionary<Body, System.Collections.Generic.List<JointHandle>> _bodyJoints =
            new System.Collections.Generic.Dictionary<Body, System.Collections.Generic.List<JointHandle>>();
        private readonly System.Collections.Generic.Dictionary<int, SolverSet> _sleepingIslandSets =
            new System.Collections.Generic.Dictionary<int, SolverSet>();
        private readonly System.Collections.Generic.Dictionary<JointHandle, SolverSetType> _jointSolverSetTypes =
            new System.Collections.Generic.Dictionary<JointHandle, SolverSetType>();

        public Body CreateBody(BodyDef def)
        {
            Body body = new Body(this, def);
            _bodies.Add(body);
            EnsureBodyAdjacency(body);
            return body;
        }

        public DistanceJoint CreateJoint(DistanceJointDef def)
        {
            DistanceJoint joint = new DistanceJoint(def);
            _distanceJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Distance, _distanceJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public RevoluteJoint CreateJoint(RevoluteJointDef def)
        {
            RevoluteJoint joint = new RevoluteJoint(def);
            _revoluteJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Revolute, _revoluteJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public PrismaticJoint CreateJoint(PrismaticJointDef def)
        {
            PrismaticJoint joint = new PrismaticJoint(def);
            _prismaticJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Prismatic, _prismaticJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public WheelJoint CreateJoint(WheelJointDef def)
        {
            WheelJoint joint = new WheelJoint(def);
            _wheelJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Wheel, _wheelJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public PulleyJoint CreateJoint(PulleyJointDef def)
        {
            PulleyJoint joint = new PulleyJoint(def);
            _pulleyJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Pulley, _pulleyJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public WeldJoint CreateJoint(WeldJointDef def)
        {
            WeldJoint joint = new WeldJoint(def);
            _weldJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Weld, _weldJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public MotorJoint CreateJoint(MotorJointDef def)
        {
            MotorJoint joint = new MotorJoint(def);
            _motorJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Motor, _motorJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public GearJoint CreateJoint(GearJointDef def)
        {
            GearJoint joint = new GearJoint(def);
            _gearJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Gear, _gearJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public RopeJoint CreateJoint(RopeJointDef def)
        {
            RopeJoint joint = new RopeJoint(def);
            _ropeJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Rope, _ropeJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public FrictionJoint CreateJoint(FrictionJointDef def)
        {
            FrictionJoint joint = new FrictionJoint(def);
            _frictionJoints.Add(joint);
            JointHandle handle = new JointHandle(JointType.Friction, _frictionJoints.Count - 1);
            LinkJoint(handle, joint.BodyA, joint.BodyB);
            _jointSolverSetTypes[handle] = GetJointSolverSetType(joint.BodyA, joint.BodyB);
            MarkIslandDirty(joint.BodyA);
            MarkIslandDirty(joint.BodyB);
            HandleJointCreationAwakeState(joint.BodyA, joint.BodyB);
            return joint;
        }

        public bool DestroyJoint(DistanceJoint joint) => RemoveJoint(_distanceJoints, joint);
        public bool DestroyJoint(RevoluteJoint joint) => RemoveJoint(_revoluteJoints, joint);
        public bool DestroyJoint(PrismaticJoint joint) => RemoveJoint(_prismaticJoints, joint);
        public bool DestroyJoint(WheelJoint joint) => RemoveJoint(_wheelJoints, joint);
        public bool DestroyJoint(PulleyJoint joint) => RemoveJoint(_pulleyJoints, joint);
        public bool DestroyJoint(WeldJoint joint) => RemoveJoint(_weldJoints, joint);
        public bool DestroyJoint(MotorJoint joint) => RemoveJoint(_motorJoints, joint);
        public bool DestroyJoint(GearJoint joint) => RemoveJoint(_gearJoints, joint);
        public bool DestroyJoint(RopeJoint joint) => RemoveJoint(_ropeJoints, joint);
        public bool DestroyJoint(FrictionJoint joint) => RemoveJoint(_frictionJoints, joint);

        private bool RemoveJoint<TJoint>(System.Collections.Generic.List<TJoint> joints, TJoint joint) where TJoint : class
        {
            int index = joints.IndexOf(joint);
            bool removed = joints.Remove(joint);
            if (removed)
            {
                Body bodyA = GetJointBodyA(joint);
                Body bodyB = GetJointBodyB(joint);
                JointHandle handle = new JointHandle(GetJointType(joint), index);
                UnlinkJoint(handle, bodyA, bodyB);
                _jointSolverSetTypes.Remove(handle);
                MarkIslandDirty(bodyA);
                MarkIslandDirty(bodyB);
                if (index >= 0)
                {
                    RemoveJointFromSolverSets(GetJointType(joint), index);
                    UpdateJointHandleIndices(GetJointType(joint), index);
                }
                _islandsDirty = true;
            }
            return removed;
        }

        internal void NotifyAwakeChanged(Body body)
        {
            if (body.Type == BodyType.Static)
            {
                return;
            }

            MarkIslandDirty(body);

            if (body.Awake)
            {
                body.SolverSetType = SolverSetType.Awake;
                if (_sleepingSet.Bodies.Remove(body))
                {
                    if (!_awakeSet.Bodies.Contains(body))
                    {
                        _awakeSet.Bodies.Add(body);
                    }
                }
                else if (!_awakeSet.Bodies.Contains(body))
                {
                    _awakeSet.Bodies.Add(body);
                }

                if (_bodyIslandIds.TryGetValue(body, out int islandId) && islandId >= 0 && islandId < _lastIslands.Count)
                {
                    Island island = _lastIslands[islandId];
                    if (!island.IsAwake)
                    {
                        island.IsAwake = true;
                        WakeIslandFromSleepingSet(island);
                        RemoveSleepingSetForIsland(island);
                        for (int i = 0; i < island.Bodies.Count; ++i)
                        {
                            Body islandBody = island.Bodies[i];
                            if (islandBody.Type == BodyType.Static)
                            {
                                continue;
                            }
                            if (!islandBody.Awake)
                            {
                                islandBody.SetAwakeFromWorld(true, notifyWorld: false);
                            }
                            if (!_awakeSet.Bodies.Contains(islandBody))
                            {
                                _awakeSet.Bodies.Add(islandBody);
                            }
                            _sleepingSet.Bodies.Remove(islandBody);
                        }
                    }
                }
            }
            else
            {
                body.SolverSetType = SolverSetType.Sleeping;
                if (_awakeSet.Bodies.Remove(body))
                {
                    if (!_sleepingSet.Bodies.Contains(body))
                    {
                        _sleepingSet.Bodies.Add(body);
                    }
                }
                else if (!_sleepingSet.Bodies.Contains(body))
                {
                    _sleepingSet.Bodies.Add(body);
                }

                if (_bodyIslandIds.TryGetValue(body, out int islandId) && islandId >= 0 && islandId < _lastIslands.Count)
                {
                    Island island = _lastIslands[islandId];
                    if (island.IsAwake)
                    {
                        island.IsAwake = false;
                        MoveIslandBetweenSets(island, _awakeSet, _sleepingSet);
                        CreateSleepingSetForIsland(island);
                    }
                }
            }

            _islandsDirty = true;
        }

        private void MarkIslandDirty(Body body)
        {
            if (_bodyIslandIds.TryGetValue(body, out int islandId))
            {
                _dirtyIslandIds.Add(islandId);
            }
            _islandsDirty = true;
        }

        public System.Collections.Generic.IReadOnlyList<Island> BuildIslands(bool awakeOnly = true)
        {
            if (!_islandsDirty && _lastIslands.Count > 0)
            {
                return _lastIslands;
            }

            _islandsDirty = false;

            IslandAdjacency adjacency = BuildIslandAdjacency();
            System.Collections.Generic.List<Island> islands;
            if (_lastIslands.Count == 0 || _dirtyIslandIds.Count == 0)
            {
                islands = BuildAllIslands(adjacency);
            }
            else
            {
                islands = BuildDirtyIslands(adjacency);
            }

            AssignIslandIds(islands, adjacency.BodyIndex);
            UpdateIslandAwakeFlags(islands);
            NormalizeAwakeIslands(islands);
            RebuildSolverSets(islands);
            SortSolverSets();
            _lastIslands = islands;
            RebuildIslandIds();
            _dirtyIslandIds.Clear();

            return islands;
        }

        private sealed class IslandAdjacency
        {
            public readonly System.Collections.Generic.Dictionary<Body, int> BodyIndex;
            public readonly System.Collections.Generic.List<Contact>[] BodyContacts;
            public readonly System.Collections.Generic.List<JointHandle>[] BodyJoints;

            public IslandAdjacency(
                System.Collections.Generic.Dictionary<Body, int> bodyIndex,
                System.Collections.Generic.List<Contact>[] bodyContacts,
                System.Collections.Generic.List<JointHandle>[] bodyJoints)
            {
                BodyIndex = bodyIndex;
                BodyContacts = bodyContacts;
                BodyJoints = bodyJoints;
            }
        }

        private IslandAdjacency BuildIslandAdjacency()
        {
            System.Collections.Generic.Dictionary<Body, int> bodyIndex = new System.Collections.Generic.Dictionary<Body, int>(_bodies.Count);
            System.Collections.Generic.List<Contact>[] bodyContacts = new System.Collections.Generic.List<Contact>[_bodies.Count];
            System.Collections.Generic.List<JointHandle>[] bodyJoints = new System.Collections.Generic.List<JointHandle>[_bodies.Count];

            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                bodyIndex[body] = i;
                EnsureBodyAdjacency(body);
                bodyContacts[i] = new System.Collections.Generic.List<Contact>(_bodyContacts[body]);
                bodyContacts[i].Sort(CompareContacts);
                bodyJoints[i] = new System.Collections.Generic.List<JointHandle>(_bodyJoints[body]);
                bodyJoints[i].Sort(CompareJointHandles);
            }

            return new IslandAdjacency(bodyIndex, bodyContacts, bodyJoints);
        }

        private System.Collections.Generic.List<Island> BuildAllIslands(IslandAdjacency adjacency)
        {
            System.Collections.Generic.List<Island> islands = new System.Collections.Generic.List<Island>();
            bool[] bodyVisited = new bool[_bodies.Count];
            System.Collections.Generic.HashSet<Contact> contactAdded = new System.Collections.Generic.HashSet<Contact>();
            System.Collections.Generic.HashSet<JointHandle> jointAdded = new System.Collections.Generic.HashSet<JointHandle>();
            System.Collections.Generic.Stack<int> stack = new System.Collections.Generic.Stack<int>();

            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                if (body.Type == BodyType.Static)
                {
                    continue;
                }
                if (bodyVisited[i])
                {
                    continue;
                }

                Island island = new Island();
                stack.Push(i);
                bodyVisited[i] = true;
                BuildIslandFromSeed(adjacency, island, stack, bodyVisited, contactAdded, jointAdded);
                if (island.Bodies.Count > 0)
                {
                    islands.Add(island);
                }
            }

            return islands;
        }

        private System.Collections.Generic.List<Island> BuildDirtyIslands(IslandAdjacency adjacency)
        {
            bool[] dirtyBodies = new bool[_bodies.Count];
            foreach (int islandId in _dirtyIslandIds)
            {
                if (islandId < 0 || islandId >= _lastIslands.Count)
                {
                    continue;
                }

                Island island = _lastIslands[islandId];
                for (int b = 0; b < island.Bodies.Count; ++b)
                {
                    Body body = island.Bodies[b];
                    if (body.Type == BodyType.Static)
                    {
                        continue;
                    }
                    if (adjacency.BodyIndex.TryGetValue(body, out int bodyIndex))
                    {
                        dirtyBodies[bodyIndex] = true;
                    }
                }
            }

            System.Collections.Generic.List<Island> islands = new System.Collections.Generic.List<Island>();
            bool[] bodyVisited = new bool[_bodies.Count];
            System.Collections.Generic.HashSet<Contact> contactAdded = new System.Collections.Generic.HashSet<Contact>();
            System.Collections.Generic.HashSet<JointHandle> jointAdded = new System.Collections.Generic.HashSet<JointHandle>();
            System.Collections.Generic.Stack<int> stack = new System.Collections.Generic.Stack<int>();

            for (int i = 0; i < _bodies.Count; ++i)
            {
                if (!dirtyBodies[i])
                {
                    continue;
                }

                Body body = _bodies[i];
                if (body.Type == BodyType.Static)
                {
                    continue;
                }
                if (bodyVisited[i])
                {
                    continue;
                }

                Island island = new Island();
                stack.Push(i);
                bodyVisited[i] = true;
                BuildIslandFromSeed(adjacency, island, stack, bodyVisited, contactAdded, jointAdded);
                if (island.Bodies.Count > 0)
                {
                    islands.Add(island);
                }
            }

            for (int i = 0; i < _lastIslands.Count; ++i)
            {
                Island island = _lastIslands[i];
                bool rebuilt = false;
                for (int b = 0; b < island.Bodies.Count; ++b)
                {
                    Body body = island.Bodies[b];
                    if (adjacency.BodyIndex.TryGetValue(body, out int bodyIndex) && bodyVisited[bodyIndex])
                    {
                        rebuilt = true;
                        break;
                    }
                }
                if (!rebuilt)
                {
                    islands.Add(island);
                }
            }

            islands.Sort((a, b) => GetIslandSortKey(adjacency.BodyIndex, a).CompareTo(GetIslandSortKey(adjacency.BodyIndex, b)));
            return islands;
        }

        private static int GetIslandSortKey(System.Collections.Generic.Dictionary<Body, int> bodyIndex, Island island)
        {
            int best = int.MaxValue;
            for (int i = 0; i < island.Bodies.Count; ++i)
            {
                if (bodyIndex.TryGetValue(island.Bodies[i], out int index))
                {
                    best = Math.Min(best, index);
                }
            }
            return best;
        }

        private void BuildIslandFromSeed(
            IslandAdjacency adjacency,
            Island island,
            System.Collections.Generic.Stack<int> stack,
            bool[] bodyVisited,
            System.Collections.Generic.HashSet<Contact> contactAdded,
            System.Collections.Generic.HashSet<JointHandle> jointAdded)
        {
            while (stack.Count > 0)
            {
                int bodyIndexValue = stack.Pop();
                Body node = _bodies[bodyIndexValue];
                island.Bodies.Add(node);

                foreach (Contact contact in adjacency.BodyContacts[bodyIndexValue])
                {
                    if (contactAdded.Add(contact))
                    {
                        island.Contacts.Add(contact);
                    }

                    Fixture? fixtureA = contact.FixtureA;
                    Fixture? fixtureB = contact.FixtureB;
                    if (fixtureA == null || fixtureB == null)
                    {
                        continue;
                    }

                    int otherIndex = fixtureA.Body == node ? adjacency.BodyIndex[fixtureB.Body] : adjacency.BodyIndex[fixtureA.Body];
                    Body otherBody = _bodies[otherIndex];
                    if (otherBody.Type == BodyType.Static)
                    {
                        continue;
                    }
                    if (bodyVisited[otherIndex])
                    {
                        continue;
                    }

                    bodyVisited[otherIndex] = true;
                    stack.Push(otherIndex);
                }

                foreach (JointHandle handle in adjacency.BodyJoints[bodyIndexValue])
                {
                    if (jointAdded.Add(handle))
                    {
                        island.Joints.Add(handle);
                    }

                    Body? otherBody = GetJointOtherBody(handle, node);
                    if (otherBody == null || otherBody.Type == BodyType.Static)
                    {
                        continue;
                    }

                    int otherIndex = adjacency.BodyIndex[otherBody];
                    if (bodyVisited[otherIndex])
                    {
                        continue;
                    }

                    bodyVisited[otherIndex] = true;
                    stack.Push(otherIndex);
                }
            }
        }

        private void UpdateIslandAwakeFlags(System.Collections.Generic.List<Island> islands)
        {
            for (int i = 0; i < islands.Count; ++i)
            {
                Island island = islands[i];
                bool islandAwake = !_def.EnableSleep;
                if (_def.EnableSleep)
                {
                    for (int j = 0; j < island.Bodies.Count; ++j)
                    {
                        if (island.Bodies[j].Awake)
                        {
                            islandAwake = true;
                            break;
                        }
                    }
                }
                island.IsAwake = islandAwake;
            }
        }

        private void NormalizeAwakeIslands(System.Collections.Generic.List<Island> islands)
        {
            if (!_def.EnableSleep)
            {
                return;
            }

            for (int i = 0; i < islands.Count; ++i)
            {
                Island island = islands[i];
                if (!island.IsAwake)
                {
                    continue;
                }

                for (int j = 0; j < island.Bodies.Count; ++j)
                {
                    Body body = island.Bodies[j];
                    if (body.Type == BodyType.Static)
                    {
                        continue;
                    }
                    if (!body.Awake)
                    {
                        body.SetAwakeFromWorld(true, notifyWorld: false);
                    }
                    body.SolverSetType = SolverSetType.Awake;
                }
            }
        }

        private void AssignIslandIds(System.Collections.Generic.List<Island> islands, System.Collections.Generic.Dictionary<Body, int> bodyIndex)
        {
            for (int i = 0; i < islands.Count; ++i)
            {
                islands[i].Id = GetIslandKey(islands[i], bodyIndex);
            }
        }

        private static int GetIslandKey(Island island, System.Collections.Generic.Dictionary<Body, int> bodyIndex)
        {
            int best = int.MaxValue;
            for (int i = 0; i < island.Bodies.Count; ++i)
            {
                if (bodyIndex.TryGetValue(island.Bodies[i], out int index))
                {
                    best = Math.Min(best, index);
                }
            }
            return best == int.MaxValue ? 0 : best;
        }

        private void RebuildSolverSets(System.Collections.Generic.List<Island> islands)
        {
            _awakeSet.Bodies.Clear();
            _awakeSet.Islands.Clear();
            _awakeSet.Contacts.Clear();
            _awakeSet.Joints.Clear();
            _sleepingSet.Bodies.Clear();
            _sleepingSet.Islands.Clear();
            _sleepingSet.Contacts.Clear();
            _sleepingSet.Joints.Clear();
            _sleepingIslandSets.Clear();
            _jointSolverSetTypes.Clear();

            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                if (body.Type == BodyType.Static)
                {
                    body.SolverSetType = SolverSetType.Static;
                    continue;
                }
                if (_def.EnableSleep && body.Awake == false)
                {
                    _sleepingSet.Bodies.Add(body);
                    body.SolverSetType = SolverSetType.Sleeping;
                }
                else
                {
                    _awakeSet.Bodies.Add(body);
                    body.SolverSetType = SolverSetType.Awake;
                }
            }

            for (int i = 0; i < islands.Count; ++i)
            {
                Island island = islands[i];
                if (island.IsAwake)
                {
                    AddIslandToSet(island, _awakeSet);
                }
                else
                {
                    AddIslandToSet(island, _sleepingSet);
                    CreateSleepingSetForIsland(island);
                }
            }

            for (int i = 0; i < _awakeSet.Islands.Count; ++i)
            {
                Island island = _awakeSet.Islands[i];
                for (int j = 0; j < island.Contacts.Count; ++j)
                {
                    island.Contacts[j].SolverSetType = SolverSetType.Awake;
                }
                for (int j = 0; j < island.Joints.Count; ++j)
                {
                    _jointSolverSetTypes[island.Joints[j]] = SolverSetType.Awake;
                }
            }

            for (int i = 0; i < _sleepingSet.Islands.Count; ++i)
            {
                Island island = _sleepingSet.Islands[i];
                for (int j = 0; j < island.Contacts.Count; ++j)
                {
                    island.Contacts[j].SolverSetType = SolverSetType.Sleeping;
                }
                for (int j = 0; j < island.Joints.Count; ++j)
                {
                    _jointSolverSetTypes[island.Joints[j]] = SolverSetType.Sleeping;
                }
            }

            for (int i = 0; i < _awakeSet.Contacts.Count; ++i)
            {
                _awakeSet.Contacts[i].SolverSetType = SolverSetType.Awake;
            }
            for (int i = 0; i < _sleepingSet.Contacts.Count; ++i)
            {
                _sleepingSet.Contacts[i].SolverSetType = SolverSetType.Sleeping;
            }
            for (int i = 0; i < _awakeSet.Joints.Count; ++i)
            {
                _jointSolverSetTypes[_awakeSet.Joints[i]] = SolverSetType.Awake;
            }
            for (int i = 0; i < _sleepingSet.Joints.Count; ++i)
            {
                _jointSolverSetTypes[_sleepingSet.Joints[i]] = SolverSetType.Sleeping;
            }
        }

        private void SortSolverSets()
        {
            System.Collections.Generic.Dictionary<Body, int> bodyIndex = new System.Collections.Generic.Dictionary<Body, int>(_bodies.Count);
            for (int i = 0; i < _bodies.Count; ++i)
            {
                bodyIndex[_bodies[i]] = i;
            }

            _awakeSet.Islands.Sort(CompareIslands);
            _sleepingSet.Islands.Sort(CompareIslands);
            SortSolverSet(_awakeSet);
            SortSolverSet(_sleepingSet);
        }

        private void SortSolverSet(SolverSet set)
        {
            set.Contacts.Sort(CompareContacts);
            set.Joints.Sort(CompareJointHandles);
        }

        private static int CompareIslands(Island? a, Island? b)
        {
            if (ReferenceEquals(a, b))
            {
                return 0;
            }
            if (a == null)
            {
                return -1;
            }
            if (b == null)
            {
                return 1;
            }

            return a.Id.CompareTo(b.Id);
        }

        private static int CompareContacts(Contact? a, Contact? b)
        {
            if (ReferenceEquals(a, b))
            {
                return 0;
            }
            if (a == null)
            {
                return -1;
            }
            if (b == null)
            {
                return 1;
            }

            int keyA0 = GetContactKeyPart(a, first: true);
            int keyA1 = GetContactKeyPart(a, first: false);
            int keyB0 = GetContactKeyPart(b, first: true);
            int keyB1 = GetContactKeyPart(b, first: false);

            int cmp = keyA0.CompareTo(keyB0);
            return cmp != 0 ? cmp : keyA1.CompareTo(keyB1);
        }

        private static int GetContactKeyPart(Contact contact, bool first)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
            {
                return 0;
            }
            int a = contact.FixtureA.ProxyId;
            int b = contact.FixtureB.ProxyId;
            int min = a < b ? a : b;
            int max = a < b ? b : a;
            return first ? min : max;
        }

        private static int CompareJointHandles(JointHandle a, JointHandle b)
        {
            int typeCmp = a.Type.CompareTo(b.Type);
            return typeCmp != 0 ? typeCmp : a.Index.CompareTo(b.Index);
        }

        private void EnsureBodyAdjacency(Body body)
        {
            if (!_bodyContacts.ContainsKey(body))
            {
                _bodyContacts[body] = new System.Collections.Generic.List<Contact>();
            }
            if (!_bodyJoints.ContainsKey(body))
            {
                _bodyJoints[body] = new System.Collections.Generic.List<JointHandle>();
            }
        }

        private void LinkContact(Contact contact)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
            {
                return;
            }
            if (contact.FixtureA.IsSensor || contact.FixtureB.IsSensor)
            {
                return;
            }

            Body bodyA = contact.FixtureA.Body;
            Body bodyB = contact.FixtureB.Body;
            EnsureBodyAdjacency(bodyA);
            EnsureBodyAdjacency(bodyB);

            System.Collections.Generic.List<Contact> listA = _bodyContacts[bodyA];
            if (!listA.Contains(contact))
            {
                listA.Add(contact);
            }

            System.Collections.Generic.List<Contact> listB = _bodyContacts[bodyB];
            if (!listB.Contains(contact))
            {
                listB.Add(contact);
            }

            MarkIslandDirty(bodyA);
            MarkIslandDirty(bodyB);
        }

        private void UnlinkContact(Contact contact)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
            {
                return;
            }

            Body bodyA = contact.FixtureA.Body;
            Body bodyB = contact.FixtureB.Body;
            if (_bodyContacts.TryGetValue(bodyA, out System.Collections.Generic.List<Contact>? listA))
            {
                listA.Remove(contact);
            }
            if (_bodyContacts.TryGetValue(bodyB, out System.Collections.Generic.List<Contact>? listB))
            {
                listB.Remove(contact);
            }

            MarkIslandDirty(bodyA);
            MarkIslandDirty(bodyB);
        }

        private void LinkJoint(JointHandle handle, Body bodyA, Body bodyB)
        {
            EnsureBodyAdjacency(bodyA);
            EnsureBodyAdjacency(bodyB);

            System.Collections.Generic.List<JointHandle> listA = _bodyJoints[bodyA];
            if (!listA.Contains(handle))
            {
                listA.Add(handle);
            }

            System.Collections.Generic.List<JointHandle> listB = _bodyJoints[bodyB];
            if (!listB.Contains(handle))
            {
                listB.Add(handle);
            }
        }

        private void UnlinkJoint(JointHandle handle, Body bodyA, Body bodyB)
        {
            if (_bodyJoints.TryGetValue(bodyA, out System.Collections.Generic.List<JointHandle>? listA))
            {
                listA.Remove(handle);
            }
            if (_bodyJoints.TryGetValue(bodyB, out System.Collections.Generic.List<JointHandle>? listB))
            {
                listB.Remove(handle);
            }
        }

        private void UpdateJointHandleIndices(JointType type, int removedIndex)
        {
            foreach (var pair in _bodyJoints)
            {
                System.Collections.Generic.List<JointHandle> list = pair.Value;
                for (int i = list.Count - 1; i >= 0; --i)
                {
                    JointHandle handle = list[i];
                    if (handle.Type != type)
                    {
                        continue;
                    }
                    if (handle.Index == removedIndex)
                    {
                        list.RemoveAt(i);
                    }
                    else if (handle.Index > removedIndex)
                    {
                        JointHandle updated = new JointHandle(type, handle.Index - 1);
                        list[i] = updated;
                        if (_jointSolverSetTypes.TryGetValue(handle, out SolverSetType setType))
                        {
                            _jointSolverSetTypes.Remove(handle);
                            _jointSolverSetTypes[updated] = setType;
                        }
                    }
                }
            }
        }

        private SolverSetType GetJointSolverSetType(Body bodyA, Body bodyB)
        {
            if (!_def.EnableSleep)
            {
                return SolverSetType.Awake;
            }

            if (bodyA.SolverSetType == SolverSetType.Awake || bodyB.SolverSetType == SolverSetType.Awake)
            {
                return SolverSetType.Awake;
            }

            return SolverSetType.Sleeping;
        }

        internal bool TryGetJointSolverSetType(JointHandle handle, out SolverSetType type)
        {
            return _jointSolverSetTypes.TryGetValue(handle, out type);
        }

        private static void AddJointHandles<TJoint>(
            System.Collections.Generic.List<JointHandle>[] bodyJoints,
            System.Collections.Generic.Dictionary<Body, int> bodyIndex,
            System.Collections.Generic.List<TJoint> joints,
            JointType type) where TJoint : class
        {
            for (int i = 0; i < joints.Count; ++i)
            {
                TJoint joint = joints[i];
                Body bodyA = GetJointBodyA(joint);
                Body bodyB = GetJointBodyB(joint);
                int ia = bodyIndex[bodyA];
                int ib = bodyIndex[bodyB];
                JointHandle handle = new JointHandle(type, i);
                bodyJoints[ia].Add(handle);
                bodyJoints[ib].Add(handle);
            }
        }

        private static Body GetJointBodyA(object joint)
        {
            return joint switch
            {
                DistanceJoint j => j.BodyA,
                RevoluteJoint j => j.BodyA,
                PrismaticJoint j => j.BodyA,
                WheelJoint j => j.BodyA,
                PulleyJoint j => j.BodyA,
                WeldJoint j => j.BodyA,
                MotorJoint j => j.BodyA,
                GearJoint j => j.BodyA,
                RopeJoint j => j.BodyA,
                FrictionJoint j => j.BodyA,
                _ => throw new System.InvalidOperationException("Unknown joint type.")
            };
        }

        private static Body GetJointBodyB(object joint)
        {
            return joint switch
            {
                DistanceJoint j => j.BodyB,
                RevoluteJoint j => j.BodyB,
                PrismaticJoint j => j.BodyB,
                WheelJoint j => j.BodyB,
                PulleyJoint j => j.BodyB,
                WeldJoint j => j.BodyB,
                MotorJoint j => j.BodyB,
                GearJoint j => j.BodyB,
                RopeJoint j => j.BodyB,
                FrictionJoint j => j.BodyB,
                _ => throw new System.InvalidOperationException("Unknown joint type.")
            };
        }

        private void HandleJointCreationAwakeState(Body bodyA, Body bodyB)
        {
            if (!_def.EnableSleep)
            {
                return;
            }

            if (bodyA.Type == BodyType.Static && bodyB.Type == BodyType.Static)
            {
                return;
            }

            if (bodyA.SolverSetType == SolverSetType.Sleeping && bodyB.SolverSetType == SolverSetType.Sleeping)
            {
                return;
            }

            bool awakeA = bodyA.Type != BodyType.Static && bodyA.Awake;
            bool awakeB = bodyB.Type != BodyType.Static && bodyB.Awake;

            if (awakeA && !awakeB)
            {
                bodyB.SetAwake(true);
                return;
            }

            if (!awakeA && awakeB)
            {
                bodyA.SetAwake(true);
                return;
            }

            if (!awakeA && !awakeB)
            {
                MergeSleepingIslands(bodyA, bodyB);
            }
        }

        private void MergeSleepingIslands(Body bodyA, Body bodyB)
        {
            BuildIslands(awakeOnly: false);

            if (!_bodyIslandIds.TryGetValue(bodyA, out int islandAId) ||
                !_bodyIslandIds.TryGetValue(bodyB, out int islandBId) ||
                islandAId == islandBId ||
                islandAId < 0 || islandBId < 0 ||
                islandAId >= _lastIslands.Count || islandBId >= _lastIslands.Count)
            {
                return;
            }

            Island islandA = _lastIslands[islandAId];
            Island islandB = _lastIslands[islandBId];

            if (islandA.IsAwake || islandB.IsAwake)
            {
                return;
            }

            bool aIsPrimary = islandA.Bodies.Count >= islandB.Bodies.Count;
            if (islandA.Bodies.Count == islandB.Bodies.Count)
            {
                aIsPrimary = islandA.Id <= islandB.Id;
            }

            Island primary = aIsPrimary ? islandA : islandB;
            Island secondary = primary == islandA ? islandB : islandA;

            RemoveIslandFromSet(secondary, _sleepingSet);
            _lastIslands.Remove(secondary);
            if (_sleepingIslandSets.TryGetValue(secondary.Id, out SolverSet? secondarySet))
            {
                _sleepingIslandSets.Remove(secondary.Id);
                if (!_sleepingIslandSets.TryGetValue(primary.Id, out SolverSet? primarySet))
                {
                    CreateSleepingSetForIsland(primary);
                    _sleepingIslandSets.TryGetValue(primary.Id, out primarySet);
                }
                if (primarySet != null)
                {
                    AddIslandToSet(secondary, primarySet);
                    AddIslandBodiesToSet(secondary, primarySet);
                    SortSolverSet(primarySet);
                    for (int i = 0; i < secondary.Contacts.Count; ++i)
                    {
                        secondary.Contacts[i].SolverSetType = SolverSetType.Sleeping;
                    }
                    for (int i = 0; i < secondary.Joints.Count; ++i)
                    {
                        _jointSolverSetTypes[secondary.Joints[i]] = SolverSetType.Sleeping;
                    }
                }
            }

            for (int i = 0; i < secondary.Bodies.Count; ++i)
            {
                Body body = secondary.Bodies[i];
                primary.Bodies.Add(body);
            }

            for (int i = 0; i < secondary.Contacts.Count; ++i)
            {
                Contact contact = secondary.Contacts[i];
                primary.Contacts.Add(contact);
            }

            for (int i = 0; i < secondary.Joints.Count; ++i)
            {
                JointHandle handle = secondary.Joints[i];
                primary.Joints.Add(handle);
            }

            AddIslandToSet(primary, _sleepingSet);
            RebuildIslandIds();
        }


        private void RebuildIslandIds()
        {
            _bodyIslandIds.Clear();
            for (int i = 0; i < _lastIslands.Count; ++i)
            {
                Island island = _lastIslands[i];
                for (int j = 0; j < island.Bodies.Count; ++j)
                {
                    _bodyIslandIds[island.Bodies[j]] = i;
                }
            }
        }

        private void AddIslandToSet(Island island, SolverSet set)
        {
            if (!set.Islands.Contains(island))
            {
                set.Islands.Add(island);
            }

            for (int i = 0; i < island.Contacts.Count; ++i)
            {
                Contact contact = island.Contacts[i];
                if (!set.Contacts.Contains(contact))
                {
                    set.Contacts.Add(contact);
                }
            }

            for (int i = 0; i < island.Joints.Count; ++i)
            {
                JointHandle handle = island.Joints[i];
                if (!set.Joints.Contains(handle))
                {
                    set.Joints.Add(handle);
                }
            }
        }

        private void AddIslandBodiesToSet(Island island, SolverSet set)
        {
            for (int i = 0; i < island.Bodies.Count; ++i)
            {
                Body body = island.Bodies[i];
                if (body.Type == BodyType.Static)
                {
                    continue;
                }
                if (!set.Bodies.Contains(body))
                {
                    set.Bodies.Add(body);
                }
            }
        }

        private void RemoveIslandFromSet(Island island, SolverSet set)
        {
            set.Islands.Remove(island);

            for (int i = 0; i < island.Contacts.Count; ++i)
            {
                set.Contacts.Remove(island.Contacts[i]);
            }

            for (int i = 0; i < island.Joints.Count; ++i)
            {
                set.Joints.Remove(island.Joints[i]);
            }
        }

        private void WakeIslandFromSleepingSet(Island island)
        {
            MoveIslandBetweenSets(island, _sleepingSet, _awakeSet);

            if (_sleepingIslandSets.TryGetValue(island.Id, out SolverSet? islandSet))
            {
                for (int i = 0; i < islandSet.Bodies.Count; ++i)
                {
                    Body body = islandSet.Bodies[i];
                    if (body.Type == BodyType.Static)
                    {
                        continue;
                    }
                    body.SolverSetType = SolverSetType.Awake;
                    if (!_awakeSet.Bodies.Contains(body))
                    {
                        _awakeSet.Bodies.Add(body);
                    }
                    _sleepingSet.Bodies.Remove(body);
                }

                for (int i = 0; i < islandSet.Contacts.Count; ++i)
                {
                    Contact contact = islandSet.Contacts[i];
                    if (!_awakeSet.Contacts.Contains(contact))
                    {
                        _awakeSet.Contacts.Add(contact);
                    }
                    _sleepingSet.Contacts.Remove(contact);
                    contact.SolverSetType = SolverSetType.Awake;
                }

                for (int i = 0; i < islandSet.Joints.Count; ++i)
                {
                    JointHandle handle = islandSet.Joints[i];
                    if (!_awakeSet.Joints.Contains(handle))
                    {
                        _awakeSet.Joints.Add(handle);
                    }
                    _sleepingSet.Joints.Remove(handle);
                    _jointSolverSetTypes[handle] = SolverSetType.Awake;
                }
            }
        }

        private void CreateSleepingSetForIsland(Island island)
        {
            SolverSet set = new SolverSet();
            AddIslandToSet(island, set);
            AddIslandBodiesToSet(island, set);
            SortSolverSet(set);
            _sleepingIslandSets[island.Id] = set;
            for (int i = 0; i < set.Contacts.Count; ++i)
            {
                set.Contacts[i].SolverSetType = SolverSetType.Sleeping;
            }
            for (int i = 0; i < set.Joints.Count; ++i)
            {
                _jointSolverSetTypes[set.Joints[i]] = SolverSetType.Sleeping;
            }
        }

        private void RemoveSleepingSetForIsland(Island island)
        {
            _sleepingIslandSets.Remove(island.Id);
        }

        private void RemoveJointFromSolverSets(JointType type, int index)
        {
            JointHandle handle = new JointHandle(type, index);
            _awakeSet.Joints.Remove(handle);
            _sleepingSet.Joints.Remove(handle);
            _jointSolverSetTypes.Remove(handle);
        }

        private static JointType GetJointType(object joint)
        {
            return joint switch
            {
                DistanceJoint => JointType.Distance,
                RevoluteJoint => JointType.Revolute,
                PrismaticJoint => JointType.Prismatic,
                WheelJoint => JointType.Wheel,
                PulleyJoint => JointType.Pulley,
                WeldJoint => JointType.Weld,
                MotorJoint => JointType.Motor,
                GearJoint => JointType.Gear,
                RopeJoint => JointType.Rope,
                FrictionJoint => JointType.Friction,
                _ => throw new System.InvalidOperationException("Unknown joint type.")
            };
        }

        private void MoveIslandBetweenSets(Island island, SolverSet source, SolverSet target)
        {
            RemoveIslandFromSet(island, source);
            AddIslandToSet(island, target);
        }

        private Body? GetJointOtherBody(JointHandle handle, Body body)
        {
            return handle.Type switch
            {
                JointType.Distance => GetOtherBody(_distanceJoints[handle.Index], body),
                JointType.Revolute => GetOtherBody(_revoluteJoints[handle.Index], body),
                JointType.Prismatic => GetOtherBody(_prismaticJoints[handle.Index], body),
                JointType.Wheel => GetOtherBody(_wheelJoints[handle.Index], body),
                JointType.Pulley => GetOtherBody(_pulleyJoints[handle.Index], body),
                JointType.Weld => GetOtherBody(_weldJoints[handle.Index], body),
                JointType.Motor => GetOtherBody(_motorJoints[handle.Index], body),
                JointType.Gear => GetOtherBody(_gearJoints[handle.Index], body),
                JointType.Rope => GetOtherBody(_ropeJoints[handle.Index], body),
                JointType.Friction => GetOtherBody(_frictionJoints[handle.Index], body),
                _ => null
            };
        }

        private static Body GetOtherBody(DistanceJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(RevoluteJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(PrismaticJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(WheelJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(PulleyJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(WeldJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(MotorJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(GearJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(RopeJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;
        private static Body GetOtherBody(FrictionJoint joint, Body body) => joint.BodyA == body ? joint.BodyB : joint.BodyA;

        internal Fixture CreateFixture(Body body, Shape shape)
        {
            return CreateFixture(body, new FixtureDef(shape));
        }

        internal Fixture CreateFixture(Body body, FixtureDef def)
        {
            Fixture fixture = new Fixture(body, def.Shape)
            {
                Friction = def.Friction,
                Restitution = def.Restitution,
                Density = def.Density,
                IsSensor = def.IsSensor,
                Filter = def.Filter,
                UserData = def.UserData
            };
            fixture.Aabb = ShapeGeometry.ComputeAabb(def.Shape, body.Transform);
            fixture.ProxyId = _broadPhase.CreateProxy(fixture.Aabb, fixture);
            return fixture;
        }

        internal void DestroyFixture(Fixture fixture)
        {
            if (fixture.ProxyId >= 0)
            {
                _broadPhase.DestroyProxy(fixture.ProxyId);
            }

            for (int i = _contacts.Count - 1; i >= 0; --i)
            {
                Contact contact = _contacts[i];
                if (contact.FixtureA == fixture || contact.FixtureB == fixture)
                {
                    UnlinkContact(contact);
                    _contacts.RemoveAt(i);
                }
            }

            System.Collections.Generic.List<ContactKey> remove = new System.Collections.Generic.List<ContactKey>();
            foreach (var pair in _contactMap)
            {
                Contact contact = pair.Value;
                if (contact.FixtureA == fixture || contact.FixtureB == fixture)
                {
                    UnlinkContact(contact);
                    remove.Add(pair.Key);
                }
            }
            for (int i = 0; i < remove.Count; ++i)
            {
                _contactMap.Remove(remove[i]);
            }

            if (fixture.ProxyId >= 0 && _sensorPairs.Count > 0)
            {
                System.Collections.Generic.List<ContactKey> sensorRemove = new System.Collections.Generic.List<ContactKey>();
                foreach (var key in _sensorPairs)
                {
                    if (key.A == fixture.ProxyId || key.B == fixture.ProxyId)
                    {
                        sensorRemove.Add(key);
                    }
                }
                for (int i = 0; i < sensorRemove.Count; ++i)
                {
                    _sensorPairs.Remove(sensorRemove[i]);
                    _sensorUserData.Remove(sensorRemove[i]);
                }
            }
        }

        public Contact CreateContact(Shape shapeA, Transform transformA, Shape shapeB, Transform transformB)
        {
            Contact contact = new Contact(shapeA, transformA, shapeB, transformB);
            contact.Evaluate();
            return contact;
        }

        public void QueryAabb(Aabb aabb, System.Func<Fixture, bool> callback, QueryFilter? filter = null, bool includeSensors = true)
        {
            QueryFilter useFilter = filter ?? QueryFilter.Default;
            _broadPhase.Query(proxyId =>
            {
                Fixture? fixture = _broadPhase.GetUserData(proxyId);
                if (fixture == null)
                {
                    return true;
                }
                if (!includeSensors && fixture.IsSensor)
                {
                    return true;
                }
                if (!PassesQueryFilter(fixture.Filter, useFilter))
                {
                    return true;
                }
                return callback(fixture);
            }, aabb);
        }

        public System.Collections.Generic.List<Fixture> QueryAabb(Aabb aabb, QueryFilter? filter = null, bool includeSensors = true)
        {
            System.Collections.Generic.List<Fixture> results = new System.Collections.Generic.List<Fixture>();
            QueryAabb(aabb, fixture =>
            {
                results.Add(fixture);
                return true;
            }, filter, includeSensors);
            return results;
        }

        public bool RayCast(RayCastInput input, out RayCastHit hit, QueryFilter? filter = null, bool includeSensors = true)
        {
            if (!Collision.IsValidRay(input))
            {
                hit = default;
                return false;
            }

            QueryFilter useFilter = filter ?? QueryFilter.Default;
            float closest = input.MaxFraction;
            bool found = false;
            RayCastHit bestHit = default;

            _broadPhase.RayCast(proxyId =>
            {
                Fixture? fixture = _broadPhase.GetUserData(proxyId);
                if (fixture == null)
                {
                    return closest;
                }
                if (!includeSensors && fixture.IsSensor)
                {
                    return closest;
                }
                if (!PassesQueryFilter(fixture.Filter, useFilter))
                {
                    return closest;
                }

                if (!TryRayCastFixture(fixture, input, out RayCastOutput output))
                {
                    return closest;
                }

                if (output.Fraction < closest)
                {
                    closest = output.Fraction;
                    found = true;
                    bestHit = new RayCastHit(fixture, output.Point, output.Normal, output.Fraction);
                }

                return closest;
            }, input);

            hit = bestHit;
            return found;
        }

        public System.Collections.Generic.List<RayCastHit> RayCastAll(RayCastInput input, QueryFilter? filter = null, bool includeSensors = true)
        {
            System.Collections.Generic.List<RayCastHit> hits = new System.Collections.Generic.List<RayCastHit>();
            if (!Collision.IsValidRay(input))
            {
                return hits;
            }

            QueryFilter useFilter = filter ?? QueryFilter.Default;
            _broadPhase.RayCast(proxyId =>
            {
                Fixture? fixture = _broadPhase.GetUserData(proxyId);
                if (fixture == null)
                {
                    return input.MaxFraction;
                }
                if (!includeSensors && fixture.IsSensor)
                {
                    return input.MaxFraction;
                }
                if (!PassesQueryFilter(fixture.Filter, useFilter))
                {
                    return input.MaxFraction;
                }
                if (!TryRayCastFixture(fixture, input, out RayCastOutput output))
                {
                    return input.MaxFraction;
                }

                hits.Add(new RayCastHit(fixture, output.Point, output.Normal, output.Fraction));
                return input.MaxFraction;
            }, input);

            hits.Sort((a, b) => a.Fraction.CompareTo(b.Fraction));
            return hits;
        }

        public void UpdateContacts()
        {
            bool contactsChanged = false;
            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                for (int j = 0; j < body.Fixtures.Count; ++j)
                {
                    Fixture fixture = body.Fixtures[j];
                    Aabb newAabb = ShapeGeometry.ComputeAabb(fixture.Shape, body.Transform);
                    Vec2 oldCenter = fixture.Aabb.Center;
                    Vec2 newCenter = newAabb.Center;
                    Vec2 displacement = newCenter - oldCenter;
                    _broadPhase.MoveProxy(fixture.ProxyId, newAabb, displacement);
                    _broadPhase.TouchProxy(fixture.ProxyId);
                    fixture.Aabb = newAabb;
                }
            }

            _contacts.Clear();
            _awakeSet.Contacts.Clear();
            _sleepingSet.Contacts.Clear();
            System.Collections.Generic.HashSet<ContactKey> seen = new System.Collections.Generic.HashSet<ContactKey>();
            System.Collections.Generic.HashSet<ContactKey> sensorSeen = new System.Collections.Generic.HashSet<ContactKey>();

            System.Collections.Generic.List<ContactBeginEvent> beginEvents = new System.Collections.Generic.List<ContactBeginEvent>();
            System.Collections.Generic.List<ContactEndEvent> endEvents = new System.Collections.Generic.List<ContactEndEvent>();
            System.Collections.Generic.List<ContactHitEvent> hitEvents = new System.Collections.Generic.List<ContactHitEvent>();
            System.Collections.Generic.List<SensorBeginEvent> sensorBeginEvents = new System.Collections.Generic.List<SensorBeginEvent>();
            System.Collections.Generic.List<SensorEndEvent> sensorEndEvents = new System.Collections.Generic.List<SensorEndEvent>();

            _broadPhase.UpdatePairs((fixtureA, fixtureB) =>
            {
                if (fixtureA == null || fixtureB == null)
                {
                    return;
                }

                if (fixtureA.IsSensor || fixtureB.IsSensor)
                {
                    if (TestOverlap(fixtureA, fixtureB))
                    {
                        ContactKey sensorKey = new ContactKey(fixtureA.ProxyId, fixtureB.ProxyId);
                        sensorSeen.Add(sensorKey);
                        if (!_sensorPairs.Contains(sensorKey))
                        {
                            sensorBeginEvents.Add(new SensorBeginEvent(fixtureA.UserData, fixtureB.UserData));
                            _sensorUserData[sensorKey] = (fixtureA.UserData, fixtureB.UserData);
                        }
                    }
                    return;
                }

                if (!ShouldCollide(fixtureA, fixtureB))
                {
                    return;
                }

                ContactKey contactKey = new ContactKey(fixtureA.ProxyId, fixtureB.ProxyId);
                seen.Add(contactKey);

                if (!_contactMap.TryGetValue(contactKey, out Contact? contact))
                {
                    contact = new Contact(fixtureA, fixtureB);
                    contact.Evaluate();
                    _contactMap[contactKey] = contact;
                    contactsChanged = true;
                }
                else
                {
                    contact.Update(fixtureA.Body.Transform, fixtureB.Body.Transform);
                }

                if (contact.IsTouching)
                {
                    Body bodyA = fixtureA.Body;
                    Body bodyB = fixtureB.Body;
                    if (bodyA.Type != BodyType.Static && bodyB.Type != BodyType.Static)
                    {
                        if (bodyA.Type == BodyType.Kinematic || bodyB.Type == BodyType.Kinematic || bodyA.Awake || bodyB.Awake)
                        {
                            bodyA.SetAwake(true);
                            bodyB.SetAwake(true);
                        }
                        else if (_def.EnableSleep)
                        {
                            MergeSleepingIslands(bodyA, bodyB);
                        }
                    }
                }

                if (contact.IsTouching && !contact.WasTouching)
                {
                    beginEvents.Add(new ContactBeginEvent(fixtureA.UserData, fixtureB.UserData));
                    contactsChanged = true;
                    LinkContact(contact);
                    MarkIslandDirty(fixtureA.Body);
                    MarkIslandDirty(fixtureB.Body);
                }
                else if (!contact.IsTouching && contact.WasTouching)
                {
                    endEvents.Add(new ContactEndEvent(fixtureA.UserData, fixtureB.UserData));
                    contactsChanged = true;
                    UnlinkContact(contact);
                    MarkIslandDirty(fixtureA.Body);
                    MarkIslandDirty(fixtureB.Body);
                }

                if (contact.Manifold.PointCount > 0)
                {
                    _contacts.Add(contact);
                    if (_def.EnableSleep &&
                        fixtureA.Body.Type != BodyType.Static &&
                        fixtureB.Body.Type != BodyType.Static)
                    {
                        SolverSetType setType = (fixtureA.Body.SolverSetType == SolverSetType.Awake ||
                                                 fixtureB.Body.SolverSetType == SolverSetType.Awake)
                            ? SolverSetType.Awake
                            : SolverSetType.Sleeping;
                        if (setType == SolverSetType.Awake)
                        {
                            _awakeSet.Contacts.Add(contact);
                        }
                        else
                        {
                            _sleepingSet.Contacts.Add(contact);
                        }
                        contact.SolverSetType = setType;
                    }
                    else
                    {
                        _awakeSet.Contacts.Add(contact);
                        contact.SolverSetType = SolverSetType.Awake;
                    }
                    AddHitEvent(contact, hitEvents);
                }
            });

            int previousCount = _contactMap.Count;
            RemoveStaleContacts(seen, endEvents);
            if (_contactMap.Count != previousCount)
            {
                contactsChanged = true;
            }
            RemoveStaleSensors(sensorSeen, sensorEndEvents);

            if (contactsChanged)
            {
                _islandsDirty = true;
            }

            SortSolverSets();

            if (beginEvents.Count > 0 || endEvents.Count > 0 || hitEvents.Count > 0)
            {
                _events.Raise(new ContactEvents(beginEvents.ToArray(), endEvents.ToArray(), hitEvents.ToArray()));
            }
            if (sensorBeginEvents.Count > 0 || sensorEndEvents.Count > 0)
            {
                _events.Raise(new SensorEvents(sensorBeginEvents.ToArray(), sensorEndEvents.ToArray()));
            }
        }

        public void Step(float timeStep)
        {
            _solverPipeline.Step(timeStep);
        }

        internal void ResetSweeps()
        {
            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                Vec2 center = body.GetWorldCenter();
                float angle = body.Transform.Q.Angle;
                body.Sweep = new Sweep(body.LocalCenter, center, center, angle, angle, 0f);
            }
        }

        internal void SyncSweeps()
        {
            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                if (body.Type == BodyType.Static)
                {
                    continue;
                }

                Vec2 center = body.GetWorldCenter();
                float angle = body.Transform.Q.Angle;
                body.Sweep = new Sweep(body.LocalCenter, body.Sweep.C0, center, body.Sweep.A0, angle, body.Sweep.Alpha0);
            }
        }

        private void RaiseContactImpulseEvents()
        {
            if (_awakeSet.Contacts.Count == 0)
            {
                return;
            }

            System.Collections.Generic.List<ContactImpulseEvent> impulseEvents = new System.Collections.Generic.List<ContactImpulseEvent>();
            for (int i = 0; i < _awakeSet.Contacts.Count; ++i)
            {
                Contact contact = _awakeSet.Contacts[i];
                if (contact.FixtureA == null || contact.FixtureB == null)
                {
                    continue;
                }

                if (contact.Manifold.PointCount == 0)
                {
                    continue;
                }

                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                float radiusA = GetShapeRadius(fixtureA.Shape);
                float radiusB = GetShapeRadius(fixtureB.Shape);
                WorldManifold worldManifold = new WorldManifold();
                worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

                Vec2 normal = worldManifold.Normal;
                for (int p = 0; p < contact.Manifold.PointCount; ++p)
                {
                    ManifoldPoint mp = contact.Manifold.Points[p];
                    Vec2 point = worldManifold.Points[p];
                    impulseEvents.Add(new ContactImpulseEvent(
                        fixtureA.UserData,
                        fixtureB.UserData,
                        point,
                        normal,
                        mp.NormalImpulse,
                        mp.TangentImpulse));
                }
            }

            if (impulseEvents.Count > 0)
            {
                _events.Raise(new ContactImpulseEvents(impulseEvents.ToArray()));
            }
        }

        private void SolveContacts(float timeStep, bool warmStart)
        {
            SolveContacts(timeStep, warmStart, _contacts);
        }

        private void SolveContacts(float timeStep, bool warmStart, System.Collections.Generic.IReadOnlyList<Contact> contacts)
        {
            for (int i = 0; i < contacts.Count; ++i)
            {
                Contact contact = contacts[i];
                if (contact.FixtureA == null || contact.FixtureB == null)
                {
                    continue;
                }

                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                if (bodyA.Type == BodyType.Static && bodyB.Type == BodyType.Static)
                {
                    continue;
                }

                float radiusA = GetShapeRadius(fixtureA.Shape);
                float radiusB = GetShapeRadius(fixtureB.Shape);

                WorldManifold worldManifold = new WorldManifold();
                worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

                Vec2 normal = worldManifold.Normal;
                Vec2 tangent = new Vec2(-normal.Y, normal.X);

                float friction = MixFriction(fixtureA, fixtureB);
                float invMassA = bodyA.InverseMass;
                float invMassB = bodyB.InverseMass;
                float invIA = bodyA.InverseInertia;
                float invIB = bodyB.InverseInertia;
                int pointCount = contact.Manifold.PointCount;
                int solvePointCount = pointCount;
                int primaryIndex = 0;
                bool useBlockSolver = false;
                float k11 = 0f;
                float k22 = 0f;
                float k12 = 0f;
                float det = 0f;

                if (pointCount == 2)
                {
                    SolverPoint sp1 = contact.SolverPoints[0];
                    SolverPoint sp2 = contact.SolverPoints[1];

                    Vec2 rA1 = sp1.RA;
                    Vec2 rB1 = sp1.RB;
                    Vec2 rA2 = sp2.RA;
                    Vec2 rB2 = sp2.RB;

                    float rnA1 = Vec2.Cross(rA1, normal);
                    float rnB1 = Vec2.Cross(rB1, normal);
                    float rnA2 = Vec2.Cross(rA2, normal);
                    float rnB2 = Vec2.Cross(rB2, normal);

                    k11 = invMassA + invMassB + invIA * rnA1 * rnA1 + invIB * rnB1 * rnB1;
                    k22 = invMassA + invMassB + invIA * rnA2 * rnA2 + invIB * rnB2 * rnB2;
                    k12 = invMassA + invMassB + invIA * rnA1 * rnA2 + invIB * rnB1 * rnB2;

                    det = k11 * k22 - k12 * k12;
                    const float kMaxConditionNumber = 1000f;
                    if (det > Constants.Epsilon && k11 * k11 < kMaxConditionNumber * det)
                    {
                        useBlockSolver = true;
                    }
                    if (!useBlockSolver)
                    {
                        solvePointCount = 1;
                        PositionManifold pm0 = ComputePositionManifold(contact, bodyA.Transform, bodyB.Transform, radiusA, radiusB, 0);
                        PositionManifold pm1 = ComputePositionManifold(contact, bodyA.Transform, bodyB.Transform, radiusA, radiusB, 1);
                        if (pm1.Separation < pm0.Separation)
                        {
                            primaryIndex = 1;
                        }

                        int otherIndex = 1 - primaryIndex;
                        ManifoldPoint otherMp = contact.Manifold.Points[otherIndex];
                        if (otherMp.NormalImpulse != 0f || otherMp.TangentImpulse != 0f)
                        {
                            contact.Manifold.Points[otherIndex] = new ManifoldPoint(otherMp.LocalPoint, 0f, 0f, otherMp.Id);
                        }
                    }
                }

                // Warm start
                if (warmStart)
                {
                    if (solvePointCount == 1)
                    {
                        int index = pointCount == 2 ? primaryIndex : 0;
                        SolverPoint sp = contact.SolverPoints[index];
                        Vec2 rA = sp.RA;
                        Vec2 rB = sp.RB;
                        ManifoldPoint mp = contact.Manifold.Points[index];
                        if (mp.NormalImpulse != 0f || mp.TangentImpulse != 0f)
                        {
                            Vec2 warm = mp.NormalImpulse * normal + mp.TangentImpulse * tangent;
                            bodyA.LinearVelocity -= invMassA * warm;
                            bodyB.LinearVelocity += invMassB * warm;
                            bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, warm);
                            bodyB.AngularVelocity += invIB * Vec2.Cross(rB, warm);
                        }
                    }
                    else
                    {
                        for (int p = 0; p < pointCount; ++p)
                        {
                            SolverPoint sp = contact.SolverPoints[p];
                            Vec2 rA = sp.RA;
                            Vec2 rB = sp.RB;
                            ManifoldPoint mp = contact.Manifold.Points[p];
                            if (mp.NormalImpulse == 0f && mp.TangentImpulse == 0f)
                            {
                                continue;
                            }

                            Vec2 warm = mp.NormalImpulse * normal + mp.TangentImpulse * tangent;
                            bodyA.LinearVelocity -= invMassA * warm;
                            bodyB.LinearVelocity += invMassB * warm;
                            bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, warm);
                            bodyB.AngularVelocity += invIB * Vec2.Cross(rB, warm);
                        }
                    }
                }

                // Friction constraints
                if (solvePointCount == 1)
                {
                    int index = pointCount == 2 ? primaryIndex : 0;
                    SolverPoint sp = contact.SolverPoints[index];
                    Vec2 rA = sp.RA;
                    Vec2 rB = sp.RB;
                    ManifoldPoint mp = contact.Manifold.Points[index];

                    Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                    Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                    Vec2 dv = vB - vA;
                    float vt = Vec2.Dot(dv, tangent);

                    if (sp.TangentMass > 0f)
                    {
                        float tangentImpulse = -vt * sp.TangentMass;
                        float maxFriction = friction * mp.NormalImpulse;
                        float newTangentImpulse = MathFng.Clamp(mp.TangentImpulse + tangentImpulse, -maxFriction, maxFriction);
                        float appliedTangent = newTangentImpulse - mp.TangentImpulse;
                        Vec2 Pt = appliedTangent * tangent;

                        bodyA.LinearVelocity -= invMassA * Pt;
                        bodyB.LinearVelocity += invMassB * Pt;
                        bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, Pt);
                        bodyB.AngularVelocity += invIB * Vec2.Cross(rB, Pt);

                        mp = new ManifoldPoint(mp.LocalPoint, mp.NormalImpulse, newTangentImpulse, mp.Id);
                        contact.Manifold.Points[index] = mp;
                    }
                }
                else
                {
                    for (int p = 0; p < pointCount; ++p)
                    {
                        SolverPoint sp = contact.SolverPoints[p];
                        Vec2 rA = sp.RA;
                        Vec2 rB = sp.RB;
                        ManifoldPoint mp = contact.Manifold.Points[p];

                        Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                        Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                        Vec2 dv = vB - vA;
                        float vt = Vec2.Dot(dv, tangent);

                        if (sp.TangentMass > 0f)
                        {
                            float tangentImpulse = -vt * sp.TangentMass;
                            float maxFriction = friction * mp.NormalImpulse;
                            float newTangentImpulse = MathFng.Clamp(mp.TangentImpulse + tangentImpulse, -maxFriction, maxFriction);
                            float appliedTangent = newTangentImpulse - mp.TangentImpulse;
                            Vec2 Pt = appliedTangent * tangent;

                            bodyA.LinearVelocity -= invMassA * Pt;
                            bodyB.LinearVelocity += invMassB * Pt;
                            bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, Pt);
                            bodyB.AngularVelocity += invIB * Vec2.Cross(rB, Pt);

                            mp = new ManifoldPoint(mp.LocalPoint, mp.NormalImpulse, newTangentImpulse, mp.Id);
                            contact.Manifold.Points[p] = mp;
                        }
                    }
                }

                // Normal constraints
                if (solvePointCount == 1)
                {
                    int index = pointCount == 2 ? primaryIndex : 0;
                    SolverPoint sp = contact.SolverPoints[index];
                    Vec2 rA = sp.RA;
                    Vec2 rB = sp.RB;
                    ManifoldPoint mp = contact.Manifold.Points[index];

                    Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                    Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                    Vec2 dv = vB - vA;

                    float vn = Vec2.Dot(dv, normal);
                    float impulse = -(vn - sp.VelocityBias + sp.Bias + sp.Softness * mp.NormalImpulse) * sp.NormalMass;
                    float newNormalImpulse = MathF.Max(mp.NormalImpulse + impulse, 0f);
                    float appliedNormal = newNormalImpulse - mp.NormalImpulse;
                    Vec2 Pn = appliedNormal * normal;

                    bodyA.LinearVelocity -= invMassA * Pn;
                    bodyB.LinearVelocity += invMassB * Pn;
                    bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, Pn);
                    bodyB.AngularVelocity += invIB * Vec2.Cross(rB, Pn);

                    mp = new ManifoldPoint(mp.LocalPoint, newNormalImpulse, mp.TangentImpulse, mp.Id);
                    contact.Manifold.Points[index] = mp;
                }
                else if (useBlockSolver)
                {
                    SolverPoint sp1 = contact.SolverPoints[0];
                    SolverPoint sp2 = contact.SolverPoints[1];
                    ManifoldPoint mp1 = contact.Manifold.Points[0];
                    ManifoldPoint mp2 = contact.Manifold.Points[1];

                    Vec2 rA1 = sp1.RA;
                    Vec2 rB1 = sp1.RB;
                    Vec2 rA2 = sp2.RA;
                    Vec2 rB2 = sp2.RB;

                    float k11Soft = k11 + sp1.Softness;
                    float k22Soft = k22 + sp2.Softness;
                    float detSoft = k11Soft * k22Soft - k12 * k12;
                    float invDet = detSoft != 0f ? 1f / detSoft : 0f;
                    Mat22 normalMass = new Mat22(
                        new Vec2(invDet * k22Soft, -invDet * k12),
                        new Vec2(-invDet * k12, invDet * k11Soft));

                    Vec2 vA1 = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA1);
                    Vec2 vB1 = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB1);
                    Vec2 vA2 = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA2);
                    Vec2 vB2 = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB2);

                    float vn1 = Vec2.Dot(vB1 - vA1, normal);
                    float vn2 = Vec2.Dot(vB2 - vA2, normal);

                    float b1 = vn1 - sp1.VelocityBias + sp1.Bias + sp1.Softness * mp1.NormalImpulse;
                    float b2 = vn2 - sp2.VelocityBias + sp2.Bias + sp2.Softness * mp2.NormalImpulse;

                    float ax = mp1.NormalImpulse;
                    float ay = mp2.NormalImpulse;

                    // Compute b' = b - K * a
                    b1 -= k11Soft * ax + k12 * ay;
                    b2 -= k12 * ax + k22Soft * ay;

                    // Case 1: both impulses positive
                    Vec2 x = -Mat22.Mul(normalMass, new Vec2(b1, b2));
                    if (x.X >= 0f && x.Y >= 0f)
                    {
                        ApplyNormalBlock(x.X - ax, x.Y - ay, normal, rA1, rB1, rA2, rB2, ref bodyA, ref bodyB, invMassA, invIA, invMassB, invIB);
                        mp1 = new ManifoldPoint(mp1.LocalPoint, x.X, mp1.TangentImpulse, mp1.Id);
                        mp2 = new ManifoldPoint(mp2.LocalPoint, x.Y, mp2.TangentImpulse, mp2.Id);
                        contact.Manifold.Points[0] = mp1;
                        contact.Manifold.Points[1] = mp2;
                        continue;
                    }

                    // Case 2: x2 = 0
                    float x1 = k11Soft > 0f ? -b1 / k11Soft : 0f;
                    float vn2Case2 = k12 * x1 + b2;
                    if (x1 >= 0f && vn2Case2 >= 0f)
                    {
                        ApplyNormalBlock(x1 - ax, -ay, normal, rA1, rB1, rA2, rB2, ref bodyA, ref bodyB, invMassA, invIA, invMassB, invIB);
                        mp1 = new ManifoldPoint(mp1.LocalPoint, x1, mp1.TangentImpulse, mp1.Id);
                        mp2 = new ManifoldPoint(mp2.LocalPoint, 0f, mp2.TangentImpulse, mp2.Id);
                        contact.Manifold.Points[0] = mp1;
                        contact.Manifold.Points[1] = mp2;
                        continue;
                    }

                    // Case 3: x1 = 0
                    float x2 = k22Soft > 0f ? -b2 / k22Soft : 0f;
                    float vn1Case3 = k12 * x2 + b1;
                    if (x2 >= 0f && vn1Case3 >= 0f)
                    {
                        ApplyNormalBlock(-ax, x2 - ay, normal, rA1, rB1, rA2, rB2, ref bodyA, ref bodyB, invMassA, invIA, invMassB, invIB);
                        mp1 = new ManifoldPoint(mp1.LocalPoint, 0f, mp1.TangentImpulse, mp1.Id);
                        mp2 = new ManifoldPoint(mp2.LocalPoint, x2, mp2.TangentImpulse, mp2.Id);
                        contact.Manifold.Points[0] = mp1;
                        contact.Manifold.Points[1] = mp2;
                    }
                }
            }
        }

        private static void ApplyNormalBlock(
            float dx1,
            float dx2,
            Vec2 normal,
            Vec2 rA1,
            Vec2 rB1,
            Vec2 rA2,
            Vec2 rB2,
            ref Body bodyA,
            ref Body bodyB,
            float invMassA,
            float invIA,
            float invMassB,
            float invIB)
        {
            Vec2 P1 = dx1 * normal;
            Vec2 P2 = dx2 * normal;
            Vec2 P = P1 + P2;

            bodyA.LinearVelocity -= invMassA * P;
            bodyB.LinearVelocity += invMassB * P;
            bodyA.AngularVelocity -= invIA * (Vec2.Cross(rA1, P1) + Vec2.Cross(rA2, P2));
            bodyB.AngularVelocity += invIB * (Vec2.Cross(rB1, P1) + Vec2.Cross(rB2, P2));
        }

        private void SolveTOI()
        {
            if (_def.EnableContinuous == false)
            {
                return;
            }

            float timeStep = _prevTimeStep;
            int subSteps = _def.MaxSubSteps;

            if (timeStep <= 0f)
            {
                return;
            }

            System.Collections.Generic.HashSet<ContactKey> processed = new System.Collections.Generic.HashSet<ContactKey>();

            System.Collections.Generic.List<Contact> orderedContacts = new System.Collections.Generic.List<Contact>(_contactMap.Count);
            foreach (Contact contact in _contactMap.Values)
            {
                orderedContacts.Add(contact);
            }

            orderedContacts.Sort(CompareContacts);
            for (int i = 0; i < orderedContacts.Count; ++i)
            {
                Contact contact = orderedContacts[i];
                if (contact.FixtureA == null || contact.FixtureB == null)
                {
                    continue;
                }

                processed.Add(new ContactKey(contact.FixtureA.ProxyId, contact.FixtureB.ProxyId));
                ProcessTOI(contact, timeStep, subSteps);
            }

            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body bulletBody = _bodies[i];
                if (!bulletBody.Definition.Bullet || bulletBody.Type == BodyType.Static)
                {
                    continue;
                }

                Vec2 translation = bulletBody.Sweep.C - bulletBody.Sweep.C0;
                float minTranslation = MathF.Max(Constants.LinearSlop, 0.25f * bulletBody.Sweep.LocalCenter.Length);
                if (translation.LengthSquared <= minTranslation * minTranslation)
                {
                    continue;
                }

                for (int f = 0; f < bulletBody.Fixtures.Count; ++f)
                {
                    Fixture fixture = bulletBody.Fixtures[f];
                    if (fixture.IsSensor)
                    {
                        continue;
                    }

                    Transform startTransform = bulletBody.Sweep.GetTransform(0f);
                    ShapeProxy proxy = BuildWorldProxy(fixture.Shape, startTransform);
                    ShapeCastInput input = new ShapeCastInput(proxy, translation, 1f, false);
                    _broadPhase.ShapeCast(otherId =>
                    {
                        Fixture? otherFixture = _broadPhase.GetUserData(otherId);
                        if (otherFixture == null)
                        {
                            return 1f;
                        }

                        if (otherFixture == fixture)
                        {
                            return 1f;
                        }

                        if (otherFixture.IsSensor)
                        {
                            return 1f;
                        }

                        if (!ShouldCollide(fixture, otherFixture))
                        {
                            return 1f;
                        }

                        ContactKey key = new ContactKey(fixture.ProxyId, otherFixture.ProxyId);
                        if (processed.Contains(key))
                        {
                            return 1f;
                        }

                        processed.Add(key);
                        Contact temp = new Contact(fixture, otherFixture);
                        temp.Update(fixture.Body.Transform, otherFixture.Body.Transform);
                        ProcessTOI(temp, timeStep, subSteps);
                        return 1f;
                    }, input);
                }
            }
        }

        private void AdvanceBodyToTOI(Body body, float alpha)
        {
            if (body.Type == BodyType.Static)
            {
                return;
            }

            Sweep sweep = body.Sweep.Advance(alpha);
            body.SetTransformFromSweep(sweep);
        }

        private void ProcessTOI(Contact contact, float timeStep, int subSteps)
        {
            Body? bodyA = contact.FixtureA?.Body;
            Body? bodyB = contact.FixtureB?.Body;

            if (bodyA == null || bodyB == null)
            {
                return;
            }

            if (contact.FixtureA!.IsSensor || contact.FixtureB!.IsSensor)
            {
                return;
            }

            if (bodyA.Type == BodyType.Static && bodyB.Type == BodyType.Static)
            {
                return;
            }

            if (bodyA.Type == BodyType.Kinematic && bodyB.Type == BodyType.Kinematic &&
                !bodyA.Definition.Bullet && !bodyB.Definition.Bullet)
            {
                return;
            }

            if (contact.IsTouching && !bodyA.Definition.Bullet && !bodyB.Definition.Bullet)
            {
                return;
            }

            if (contact.ToiCount > _def.MaxSubSteps)
            {
                return;
            }

            bool collideA = bodyA.Definition.Bullet || bodyA.Type != BodyType.Dynamic;
            bool collideB = bodyB.Definition.Bullet || bodyB.Type != BodyType.Dynamic;
            if (!collideA && !collideB)
            {
                return;
            }

            float toi;
            if (contact.HasToi)
            {
                toi = contact.Toi;
            }
            else
            {
                ShapeProxy proxyA = ShapeGeometry.ToProxy(contact.ShapeA);
                ShapeProxy proxyB = ShapeGeometry.ToProxy(contact.ShapeB);
                ToiInput toiInput = new ToiInput(proxyA, proxyB, bodyA.Sweep, bodyB.Sweep, 1f);
                ToiOutput output = TimeOfImpact.Compute(toiInput);

                if (output.State == ToiState.Overlapped)
                {
                    toi = 0f;
                }
                else if (output.State != ToiState.Hit || output.Fraction <= 0f || output.Fraction > 1f)
                {
                    return;
                }
                else
                {
                    toi = output.Fraction;
                }

                contact.CacheToi(toi);
            }

            AdvanceBodyToTOI(bodyA, toi);
            AdvanceBodyToTOI(bodyB, toi);

            if (bodyA.Type != BodyType.Static)
            {
                bodyA.SetAwake(true);
            }
            if (bodyB.Type != BodyType.Static)
            {
                bodyB.SetAwake(true);
            }

            contact.Update(bodyA.Transform, bodyB.Transform);
            if (contact.Manifold.PointCount == 0)
            {
                return;
            }

            float remaining = timeStep * (1f - toi);
            if (remaining <= 0f)
            {
                return;
            }

            float subDt = remaining / subSteps;
            for (int s = 0; s < subSteps; ++s)
            {
                IntegrateForTOI(bodyA, subDt);
                IntegrateForTOI(bodyB, subDt);

                contact.Update(bodyA.Transform, bodyB.Transform);
                if (contact.Manifold.PointCount == 0)
                {
                    continue;
                }

                PrepareContact(contact, subDt, 1f);
                SolveContact(contact, warmStart: true);
                for (int it = 0; it < _def.VelocityIterations; ++it)
                {
                    SolveContact(contact, warmStart: false);
                }
                for (int it = 0; it < _def.PositionIterations; ++it)
                {
                    SolvePositionContact(contact);
                }
            }

            contact.IncrementToiCount();
        }

        private static ShapeProxy BuildWorldProxy(Shape shape, Transform transform)
        {
            ShapeProxy localProxy = ShapeGeometry.ToProxy(shape);
            Vec2[] points = new Vec2[localProxy.Count];
            for (int i = 0; i < localProxy.Count; ++i)
            {
                points[i] = Transform.Mul(transform, localProxy.Points[i]);
            }
            return new ShapeProxy(points, localProxy.Count, localProxy.Radius);
        }

        private Body? FindBodyForShape(Shape shape)
        {
            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                for (int j = 0; j < body.Fixtures.Count; ++j)
                {
                    if (body.Fixtures[j].Shape == shape)
                    {
                        return body;
                    }
                }
            }
            return null;
        }

        private void IntegrateForTOI(Body body, float dt)
        {
            if (body.Type == BodyType.Static)
            {
                return;
            }

            Vec2 oldCenter = body.GetWorldCenter();
            float oldAngle = body.Transform.Q.Angle;

            if (body.Type == BodyType.Dynamic)
            {
                Vec2 accel = body.GravityScale * Gravity;
                if (body.InverseMass > 0f)
                {
                    accel += body.InverseMass * body.Force;
                }
                body.LinearVelocity += dt * accel;
                if (body.InverseInertia > 0f)
                {
                    body.AngularVelocity += dt * body.InverseInertia * body.Torque;
                }
                body.LinearVelocity = ApplyLinearDamping(body.LinearVelocity, body.LinearDamping, dt);
                body.AngularVelocity = ApplyAngularDamping(body.AngularVelocity, body.AngularDamping, dt);
            }

            Vec2 newCenter = oldCenter + dt * body.LinearVelocity;
            float newAngle = oldAngle + dt * body.AngularVelocity;
            body.SetTransformFromCenter(newCenter, newAngle);
            body.Sweep = new Sweep(body.LocalCenter, oldCenter, newCenter, oldAngle, newAngle, 0f);
        }

        private void PrepareContact(Contact contact, float timeStep, float dtRatio)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
            {
                return;
            }

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;
            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            float radiusA = GetShapeRadius(fixtureA.Shape);
            float radiusB = GetShapeRadius(fixtureB.Shape);

            WorldManifold worldManifold = new WorldManifold();
            worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

            Vec2 normal = worldManifold.Normal;
            Vec2 centerA = bodyA.GetWorldCenter();
            Vec2 centerB = bodyB.GetWorldCenter();
            for (int p = 0; p < contact.Manifold.PointCount; ++p)
            {
                ManifoldPoint mp = contact.Manifold.Points[p];
                if (dtRatio != 1f)
                {
                    mp = new ManifoldPoint(mp.LocalPoint, mp.NormalImpulse * dtRatio, mp.TangentImpulse * dtRatio, mp.Id);
                    contact.Manifold.Points[p] = mp;
                }

                Vec2 point = worldManifold.Points[p];
                Vec2 rA = point - centerA;
                Vec2 rB = point - centerB;

                float rnA = Vec2.Cross(rA, normal);
                float rnB = Vec2.Cross(rB, normal);
                float kNormal = bodyA.InverseMass + bodyB.InverseMass +
                                bodyA.InverseInertia * rnA * rnA + bodyB.InverseInertia * rnB * rnB;
                float normalMass = kNormal > 0f ? 1f / kNormal : 0f;

                Vec2 tangent = new Vec2(-normal.Y, normal.X);
                float rtA = Vec2.Cross(rA, tangent);
                float rtB = Vec2.Cross(rB, tangent);
                float kTangent = bodyA.InverseMass + bodyB.InverseMass +
                                 bodyA.InverseInertia * rtA * rtA + bodyB.InverseInertia * rtB * rtB;
                float tangentMass = kTangent > 0f ? 1f / kTangent : 0f;

                Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                float vn = Vec2.Dot(vB - vA, normal);

                float velocityBias = 0f;
                float softness = 0f;
                float bias = 0f;
                float restitution = MixRestitution(fixtureA, fixtureB);
                if (vn < -_def.RestitutionThreshold)
                {
                    velocityBias = -restitution * vn;
                }
                if (_def.EnableContactSoftening)
                {
                    float separation = Vec2.Dot((centerB + rB) - (centerA + rA), normal);
                    if (_def.UseSoftConstraints)
                    {
                        ComputeContactSoftness(kNormal, timeStep, separation, out bias, out softness);
                        if (softness > 0f)
                        {
                            normalMass = 1f / (kNormal + softness);
                        }
                    }
                    else
                    {
                        float baumgarte = Constants.Baumgarte * (separation + Constants.LinearSlop) / timeStep;
                        float maxBias = MathF.Max(_def.ContactSpeed, 0f);
                        float legacyBias = MathFng.Clamp(baumgarte, -maxBias, 0f);
                        float baumgarteClamped = MathFng.Clamp(legacyBias, -Constants.MaxLinearCorrection, 0f);
                        velocityBias -= baumgarteClamped;
                    }
                }

                contact.SolverPoints[p] = new SolverPoint
                {
                    RA = rA,
                    RB = rB,
                    NormalMass = normalMass,
                    TangentMass = tangentMass,
                    VelocityBias = velocityBias,
                    Softness = softness,
                    Bias = bias
                };
            }
        }

        private void SolveContact(Contact contact, bool warmStart)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
            {
                return;
            }

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;
            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            if (bodyA.Type == BodyType.Static && bodyB.Type == BodyType.Static)
            {
                return;
            }

            float radiusA = GetShapeRadius(fixtureA.Shape);
            float radiusB = GetShapeRadius(fixtureB.Shape);

            WorldManifold worldManifold = new WorldManifold();
            worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

            Vec2 normal = worldManifold.Normal;
            Vec2 tangent = new Vec2(-normal.Y, normal.X);

            float friction = MathF.Sqrt(fixtureA.Friction * fixtureB.Friction);
            float invMassA = bodyA.InverseMass;
            float invMassB = bodyB.InverseMass;
            float invIA = bodyA.InverseInertia;
            float invIB = bodyB.InverseInertia;

            for (int p = 0; p < contact.Manifold.PointCount; ++p)
            {
                SolverPoint sp = contact.SolverPoints[p];
                Vec2 rA = sp.RA;
                Vec2 rB = sp.RB;

                Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                Vec2 dv = vB - vA;

                ManifoldPoint mp = contact.Manifold.Points[p];

                if (warmStart && (mp.NormalImpulse != 0f || mp.TangentImpulse != 0f))
                {
                    Vec2 warm = mp.NormalImpulse * normal + mp.TangentImpulse * tangent;
                    bodyA.LinearVelocity -= invMassA * warm;
                    bodyB.LinearVelocity += invMassB * warm;
                    bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, warm);
                    bodyB.AngularVelocity += invIB * Vec2.Cross(rB, warm);
                }

                Vec2 vA2 = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                Vec2 vB2 = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                Vec2 dv2 = vB2 - vA2;
                float vn = Vec2.Dot(dv2, normal);
                if (vn > 0f)
                {
                    continue;
                }

                float impulse = -(vn - sp.VelocityBias + sp.Bias + sp.Softness * mp.NormalImpulse) * sp.NormalMass;
                float newNormalImpulse = MathF.Max(mp.NormalImpulse + impulse, 0f);
                float appliedNormal = newNormalImpulse - mp.NormalImpulse;
                Vec2 Pn = appliedNormal * normal;
                bodyA.LinearVelocity -= invMassA * Pn;
                bodyB.LinearVelocity += invMassB * Pn;
                bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, Pn);
                bodyB.AngularVelocity += invIB * Vec2.Cross(rB, Pn);

                Vec2 vA3 = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                Vec2 vB3 = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                Vec2 dv3 = vB3 - vA3;
                float vt = Vec2.Dot(dv3, tangent);

                if (sp.TangentMass > 0f)
                {
                    float tangentImpulse = -vt * sp.TangentMass;
                    float maxFriction = friction * newNormalImpulse;
                    float newTangentImpulse = MathFng.Clamp(mp.TangentImpulse + tangentImpulse, -maxFriction, maxFriction);
                    float appliedTangent = newTangentImpulse - mp.TangentImpulse;
                    Vec2 Pt = appliedTangent * tangent;

                    bodyA.LinearVelocity -= invMassA * Pt;
                    bodyB.LinearVelocity += invMassB * Pt;
                    bodyA.AngularVelocity -= invIA * Vec2.Cross(rA, Pt);
                    bodyB.AngularVelocity += invIB * Vec2.Cross(rB, Pt);

                    mp = new ManifoldPoint(mp.LocalPoint, newNormalImpulse, newTangentImpulse, mp.Id);
                }
                else
                {
                    mp = new ManifoldPoint(mp.LocalPoint, newNormalImpulse, mp.TangentImpulse, mp.Id);
                }

                contact.Manifold.Points[p] = mp;
            }
        }

        private void SolvePositionContact(Contact contact)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
            {
                return;
            }

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;
            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            if (bodyA.Type == BodyType.Static && bodyB.Type == BodyType.Static)
            {
                return;
            }

            float radiusA = GetShapeRadius(fixtureA.Shape);
            float radiusB = GetShapeRadius(fixtureB.Shape);

            WorldManifold worldManifold = new WorldManifold();
            worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

            Vec2 centerA = bodyA.GetWorldCenter();
            Vec2 centerB = bodyB.GetWorldCenter();
            for (int p = 0; p < contact.Manifold.PointCount; ++p)
            {
                PositionManifold pm = ComputePositionManifold(contact, bodyA.Transform, bodyB.Transform, radiusA, radiusB, p);
                Vec2 normal = pm.Normal;
                Vec2 point = pm.Point;
                float separation = pm.Separation;

                Vec2 rA = point - centerA;
                Vec2 rB = point - centerB;

                float C = MathFng.Clamp(Constants.Baumgarte * (separation + Constants.LinearSlop), -Constants.MaxLinearCorrection, 0f);

                float invMassA = bodyA.InverseMass;
                float invMassB = bodyB.InverseMass;
                float invIA = bodyA.InverseInertia;
                float invIB = bodyB.InverseInertia;

                float rnA = Vec2.Cross(rA, normal);
                float rnB = Vec2.Cross(rB, normal);
                float k = invMassA + invMassB + invIA * rnA * rnA + invIB * rnB * rnB;
                if (k <= 0f)
                {
                    continue;
                }

                float impulse = -C / k;
                Vec2 P = impulse * normal;

                Vec2 newCenterA = centerA - invMassA * P;
                Vec2 newCenterB = centerB + invMassB * P;
                float newAngleA = bodyA.Transform.Q.Angle - invIA * Vec2.Cross(rA, P);
                float newAngleB = bodyB.Transform.Q.Angle + invIB * Vec2.Cross(rB, P);

                bodyA.SetTransformFromCenter(newCenterA, newAngleA);
                bodyB.SetTransformFromCenter(newCenterB, newAngleB);
                centerA = newCenterA;
                centerB = newCenterB;
            }
        }

        private static float GetShapeRadius(Shape shape)
        {
            switch (shape.Type)
            {
                case ShapeType.Circle:
                    return ((CircleShape)shape).Radius;
                case ShapeType.Capsule:
                    return ((CapsuleShape)shape).Radius;
                case ShapeType.Segment:
                    return Constants.PolygonRadius;
                case ShapeType.Polygon:
                    return ((PolygonShape)shape).Radius;
                case ShapeType.ChainSegment:
                    return Constants.PolygonRadius;
                default:
                    return 0f;
            }
        }

        private static Vec2 ApplyLinearDamping(Vec2 velocity, float damping, float timeStep)
        {
            float factor = MathF.Max(0f, 1f - timeStep * damping);
            return velocity * factor;
        }

        private void ComputeContactSoftness(float kNormal, float timeStep, float separation, out float bias, out float softness)
        {
            bias = 0f;
            softness = 0f;

            if (kNormal <= 0f)
            {
                return;
            }

            float hertz = _def.ContactHertz;
            if (hertz <= 0f)
            {
                return;
            }

            float mass = 1f / kNormal;
            float omega = 2f * MathFng.Pi * hertz;
            float d = 2f * mass * _def.ContactDampingRatio * omega;
            float k = mass * omega * omega;

            float gamma = timeStep * (d + timeStep * k);
            if (gamma > 0f)
            {
                gamma = 1f / gamma;
            }

            float c = MathF.Min(separation + Constants.LinearSlop, 0f);
            bias = c * timeStep * k * gamma;

            float maxBias = MathF.Max(_def.ContactSpeed, 0f);
            bias = MathFng.Clamp(bias, -maxBias, 0f);

            softness = gamma;
        }

        private float MixFriction(Fixture a, Fixture b)
        {
            if (_def.FrictionCallback != null)
            {
                return _def.FrictionCallback(a.Friction, a.Filter.CategoryBits, b.Friction, b.Filter.CategoryBits);
            }
            return MathF.Sqrt(a.Friction * b.Friction);
        }

        private float MixRestitution(Fixture a, Fixture b)
        {
            if (_def.RestitutionCallback != null)
            {
                return _def.RestitutionCallback(a.Restitution, a.Filter.CategoryBits, b.Restitution, b.Filter.CategoryBits);
            }
            return 0.5f * (a.Restitution + b.Restitution);
        }

        private static Vec2 ClampLinearSpeed(Vec2 velocity, float maxSpeed)
        {
            float speedSqr = velocity.LengthSquared;
            float maxSqr = maxSpeed * maxSpeed;
            if (speedSqr > maxSqr && speedSqr > 0f)
            {
                float scale = maxSpeed / MathF.Sqrt(speedSqr);
                return velocity * scale;
            }

            return velocity;
        }

        private static float ClampAngularSpeed(float angularVelocity, float maxSpeed)
        {
            float abs = MathF.Abs(angularVelocity);
            if (abs > maxSpeed && abs > 0f)
            {
                return MathF.Sign(angularVelocity) * maxSpeed;
            }
            return angularVelocity;
        }

        private static Vec2 ClampTranslation(Vec2 translation, float maxTranslation)
        {
            float lengthSqr = translation.LengthSquared;
            float maxSqr = maxTranslation * maxTranslation;
            if (lengthSqr > maxSqr && lengthSqr > 0f)
            {
                float scale = maxTranslation / MathF.Sqrt(lengthSqr);
                return translation * scale;
            }
            return translation;
        }

        private static float ClampRotation(float rotation, float maxRotation)
        {
            float abs = MathF.Abs(rotation);
            if (abs > maxRotation && abs > 0f)
            {
                return MathF.Sign(rotation) * maxRotation;
            }
            return rotation;
        }

        private static float ApplyAngularDamping(float angularVelocity, float damping, float timeStep)
        {
            float factor = MathF.Max(0f, 1f - timeStep * damping);
            return angularVelocity * factor;
        }

        private static bool ShouldCollide(Fixture a, Fixture b)
        {
            if (a.IsSensor || b.IsSensor)
            {
                return false;
            }

            if (a.Filter.GroupIndex != 0 && a.Filter.GroupIndex == b.Filter.GroupIndex)
            {
                return a.Filter.GroupIndex > 0;
            }

            if ((a.Filter.MaskBits & b.Filter.CategoryBits) == 0 || (b.Filter.MaskBits & a.Filter.CategoryBits) == 0)
            {
                return false;
            }

            return true;
        }

        private void SolvePositionConstraints()
        {
            SolvePositionConstraints(_contacts);
        }

        private void SolvePositionConstraints(System.Collections.Generic.IReadOnlyList<Contact> contacts)
        {
            for (int i = 0; i < contacts.Count; ++i)
            {
                Contact contact = contacts[i];
                if (contact.FixtureA == null || contact.FixtureB == null)
                {
                    continue;
                }

                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                if (bodyA.Type == BodyType.Static && bodyB.Type == BodyType.Static)
                {
                    continue;
                }

                float radiusA = GetShapeRadius(fixtureA.Shape);
                float radiusB = GetShapeRadius(fixtureB.Shape);

                WorldManifold worldManifold = new WorldManifold();
                worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

                Vec2 centerA = bodyA.GetWorldCenter();
                Vec2 centerB = bodyB.GetWorldCenter();
                for (int p = 0; p < contact.Manifold.PointCount; ++p)
                {
                    PositionManifold pm = ComputePositionManifold(contact, bodyA.Transform, bodyB.Transform, radiusA, radiusB, p);
                    Vec2 normal = pm.Normal;
                    Vec2 point = pm.Point;
                    float separation = pm.Separation;

                    Vec2 rA = point - centerA;
                    Vec2 rB = point - centerB;

                    float C = MathFng.Clamp(Constants.Baumgarte * (separation + Constants.LinearSlop), -Constants.MaxLinearCorrection, 0f);

                    float invMassA = bodyA.InverseMass;
                    float invMassB = bodyB.InverseMass;
                    float invIA = bodyA.InverseInertia;
                    float invIB = bodyB.InverseInertia;

                    float rnA = Vec2.Cross(rA, normal);
                    float rnB = Vec2.Cross(rB, normal);
                    float k = invMassA + invMassB + invIA * rnA * rnA + invIB * rnB * rnB;
                    if (k <= 0f)
                    {
                        continue;
                    }

                    float impulse = -C / k;
                    Vec2 P = impulse * normal;

                    Vec2 newCenterA = centerA - invMassA * P;
                    Vec2 newCenterB = centerB + invMassB * P;
                    float newAngleA = bodyA.Transform.Q.Angle - invIA * Vec2.Cross(rA, P);
                    float newAngleB = bodyB.Transform.Q.Angle + invIB * Vec2.Cross(rB, P);

                    bodyA.SetTransformFromCenter(newCenterA, newAngleA);
                    bodyB.SetTransformFromCenter(newCenterB, newAngleB);
                    centerA = newCenterA;
                    centerB = newCenterB;
                }
            }
        }

        private readonly struct PositionManifold
        {
            public readonly Vec2 Normal;
            public readonly Vec2 Point;
            public readonly float Separation;

            public PositionManifold(Vec2 normal, Vec2 point, float separation)
            {
                Normal = normal;
                Point = point;
                Separation = separation;
            }
        }

        private static PositionManifold ComputePositionManifold(Contact contact, Transform xfA, Transform xfB, float radiusA, float radiusB, int index)
        {
            Manifold manifold = contact.Manifold;
            switch (manifold.Type)
            {
                case ManifoldType.Circles:
                    {
                        Vec2 pointA = Transform.Mul(xfA, manifold.LocalPoint);
                        Vec2 pointB = Transform.Mul(xfB, manifold.Points[0].LocalPoint);
                        Vec2 normal = pointB - pointA;
                        float distance = normal.Length;
                        if (distance > Constants.Epsilon)
                        {
                            normal = normal / distance;
                        }
                        else
                        {
                            normal = new Vec2(1f, 0f);
                            distance = 0f;
                        }
                        Vec2 point = 0.5f * (pointA + pointB);
                        float separation = distance - (radiusA + radiusB);
                        return new PositionManifold(normal, point, separation);
                    }
                case ManifoldType.FaceA:
                    {
                        Vec2 normal = Rot.Mul(xfA.Q, manifold.LocalNormal);
                        Vec2 planePoint = Transform.Mul(xfA, manifold.LocalPoint);
                        Vec2 clipPoint = Transform.Mul(xfB, manifold.Points[index].LocalPoint);
                        float separation = Vec2.Dot(clipPoint - planePoint, normal) - radiusA - radiusB;
                        return new PositionManifold(normal, clipPoint, separation);
                    }
                case ManifoldType.FaceB:
                    {
                        Vec2 normal = Rot.Mul(xfB.Q, manifold.LocalNormal);
                        Vec2 planePoint = Transform.Mul(xfB, manifold.LocalPoint);
                        Vec2 clipPoint = Transform.Mul(xfA, manifold.Points[index].LocalPoint);
                        float separation = Vec2.Dot(clipPoint - planePoint, normal) - radiusA - radiusB;
                        normal = -normal;
                        return new PositionManifold(normal, clipPoint, separation);
                    }
                default:
                    return new PositionManifold(new Vec2(1f, 0f), Vec2.Zero, 0f);
            }
        }

        private void PrepareContacts(float timeStep, float dtRatio)
        {
            PrepareContacts(timeStep, dtRatio, _contacts);
        }

        private void PrepareContacts(float timeStep, float dtRatio, System.Collections.Generic.IReadOnlyList<Contact> contacts)
        {
            for (int i = 0; i < contacts.Count; ++i)
            {
                Contact contact = contacts[i];
                if (contact.FixtureA == null || contact.FixtureB == null)
                {
                    continue;
                }

                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                Body bodyA = fixtureA.Body;
                Body bodyB = fixtureB.Body;

                float radiusA = GetShapeRadius(fixtureA.Shape);
                float radiusB = GetShapeRadius(fixtureB.Shape);

                WorldManifold worldManifold = new WorldManifold();
                worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

                Vec2 normal = worldManifold.Normal;
                Vec2 centerA = bodyA.GetWorldCenter();
                Vec2 centerB = bodyB.GetWorldCenter();
                for (int p = 0; p < contact.Manifold.PointCount; ++p)
                {
                    ManifoldPoint mp = contact.Manifold.Points[p];
                    if (dtRatio != 1f)
                    {
                        mp = new ManifoldPoint(mp.LocalPoint, mp.NormalImpulse * dtRatio, mp.TangentImpulse * dtRatio, mp.Id);
                        contact.Manifold.Points[p] = mp;
                    }

                    Vec2 point = worldManifold.Points[p];
                    Vec2 rA = point - centerA;
                    Vec2 rB = point - centerB;

                    float rnA = Vec2.Cross(rA, normal);
                    float rnB = Vec2.Cross(rB, normal);
                    float kNormal = bodyA.InverseMass + bodyB.InverseMass +
                                    bodyA.InverseInertia * rnA * rnA + bodyB.InverseInertia * rnB * rnB;
                    float normalMass = kNormal > 0f ? 1f / kNormal : 0f;

                    Vec2 tangent = new Vec2(-normal.Y, normal.X);
                    float rtA = Vec2.Cross(rA, tangent);
                    float rtB = Vec2.Cross(rB, tangent);
                    float kTangent = bodyA.InverseMass + bodyB.InverseMass +
                                     bodyA.InverseInertia * rtA * rtA + bodyB.InverseInertia * rtB * rtB;
                    float tangentMass = kTangent > 0f ? 1f / kTangent : 0f;

                    Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
                    Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
                    float vn = Vec2.Dot(vB - vA, normal);

                    float velocityBias = 0f;
                    float softness = 0f;
                    float bias = 0f;
                    float restitution = MixRestitution(fixtureA, fixtureB);
                    if (vn < -_def.RestitutionThreshold)
                    {
                        velocityBias = -restitution * vn;
                    }
                    if (_def.EnableContactSoftening)
                    {
                        float separation = Vec2.Dot((centerB + rB) - (centerA + rA), normal);
                        if (_def.UseSoftConstraints)
                        {
                            ComputeContactSoftness(kNormal, timeStep, separation, out bias, out softness);
                            if (softness > 0f)
                            {
                                normalMass = 1f / (kNormal + softness);
                            }
                        }
                        else
                        {
                            float baumgarte = Constants.Baumgarte * (separation + Constants.LinearSlop) / timeStep;
                            float maxBias = MathF.Max(_def.ContactSpeed, 0f);
                            float legacyBias = MathFng.Clamp(baumgarte, -maxBias, 0f);
                            float baumgarteClamped = MathFng.Clamp(legacyBias, -Constants.MaxLinearCorrection, 0f);
                            velocityBias -= baumgarteClamped;
                        }
                    }

                    contact.SolverPoints[p] = new SolverPoint
                    {
                        RA = rA,
                        RB = rB,
                        NormalMass = normalMass,
                        TangentMass = tangentMass,
                        VelocityBias = velocityBias,
                        Softness = softness,
                        Bias = bias
                    };
                }
            }
        }

        private void RemoveStaleContacts(System.Collections.Generic.HashSet<ContactKey> live, System.Collections.Generic.List<ContactEndEvent> endEvents)
        {
            if (_contactMap.Count == 0)
            {
                return;
            }

            System.Collections.Generic.List<ContactKey> remove = new System.Collections.Generic.List<ContactKey>();
            foreach (var pair in _contactMap)
            {
                if (!live.Contains(pair.Key))
                {
                    remove.Add(pair.Key);
                }
            }

            for (int i = 0; i < remove.Count; ++i)
            {
                if (_contactMap.TryGetValue(remove[i], out Contact? contact) && contact.FixtureA != null && contact.FixtureB != null)
                {
                    endEvents.Add(new ContactEndEvent(contact.FixtureA.UserData, contact.FixtureB.UserData));
                    _awakeSet.Contacts.Remove(contact);
                    _sleepingSet.Contacts.Remove(contact);
                    UnlinkContact(contact);
                }
                _contactMap.Remove(remove[i]);
            }
        }

        private void RemoveStaleSensors(System.Collections.Generic.HashSet<ContactKey> live, System.Collections.Generic.List<SensorEndEvent> endEvents)
        {
            if (_sensorPairs.Count == 0)
            {
                _sensorPairs.UnionWith(live);
                return;
            }

            System.Collections.Generic.List<ContactKey> remove = new System.Collections.Generic.List<ContactKey>();
            foreach (var key in _sensorPairs)
            {
                if (!live.Contains(key))
                {
                    remove.Add(key);
                }
            }

            for (int i = 0; i < remove.Count; ++i)
            {
                _sensorPairs.Remove(remove[i]);
                if (_sensorUserData.TryGetValue(remove[i], out var data))
                {
                    endEvents.Add(new SensorEndEvent(data.A, data.B));
                    _sensorUserData.Remove(remove[i]);
                }
                else
                {
                    endEvents.Add(new SensorEndEvent(null, null));
                }
            }

            _sensorPairs.UnionWith(live);
        }

        private static bool TestOverlap(Fixture a, Fixture b)
        {
            ShapeProxy proxyA = ShapeGeometry.ToProxy(a.Shape);
            ShapeProxy proxyB = ShapeGeometry.ToProxy(b.Shape);
            DistanceInput input = new DistanceInput(proxyA, proxyB, a.Body.Transform, b.Body.Transform, true);
            DistanceOutput output = Distance.Compute(input);
            return output.Distance <= 0f;
        }

        private static bool PassesQueryFilter(Filter fixtureFilter, QueryFilter queryFilter)
        {
            if ((fixtureFilter.MaskBits & queryFilter.CategoryBits) == 0)
            {
                return false;
            }
            if ((queryFilter.MaskBits & fixtureFilter.CategoryBits) == 0)
            {
                return false;
            }
            return true;
        }

        private static bool TryRayCastFixture(Fixture fixture, RayCastInput input, out RayCastOutput output)
        {
            Transform xf = fixture.Body.Transform;
            Vec2 localOrigin = Transform.MulT(xf, input.Origin);
            Vec2 localTranslation = Rot.MulT(xf.Q, input.Translation);
            RayCastInput localInput = new RayCastInput(localOrigin, localTranslation, input.MaxFraction);

            CastOutput cast;
            switch (fixture.Shape.Type)
            {
                case ShapeType.Circle:
                    cast = Collision.RayCastCircle(ShapeGeometry.ToCircle((CircleShape)fixture.Shape), localInput);
                    break;
                case ShapeType.Capsule:
                    cast = Collision.RayCastCapsule(ShapeGeometry.ToCapsule((CapsuleShape)fixture.Shape), localInput);
                    break;
                case ShapeType.Segment:
                    cast = Collision.RayCastSegment(ShapeGeometry.ToSegment((SegmentShape)fixture.Shape), localInput);
                    break;
                case ShapeType.Polygon:
                    cast = Collision.RayCastPolygon(ShapeGeometry.ToPolygon((PolygonShape)fixture.Shape), localInput);
                    break;
                case ShapeType.ChainSegment:
                    cast = Collision.RayCastChainSegment(ShapeGeometry.ToChainSegment((ChainSegmentShape)fixture.Shape), localInput);
                    break;
                default:
                    cast = new CastOutput(Vec2.Zero, Vec2.Zero, 0f, 0, false);
                    break;
            }

            if (!cast.Hit)
            {
                output = default;
                return false;
            }

            Vec2 worldPoint = Transform.Mul(xf, cast.Point);
            Vec2 worldNormal = Rot.Mul(xf.Q, cast.Normal).Normalize();
            output = new RayCastOutput(worldPoint, worldNormal, cast.Fraction);
            return true;
        }

        public readonly struct RayCastHit
        {
            public readonly Fixture Fixture;
            public readonly Vec2 Point;
            public readonly Vec2 Normal;
            public readonly float Fraction;

            public RayCastHit(Fixture fixture, Vec2 point, Vec2 normal, float fraction)
            {
                Fixture = fixture;
                Point = point;
                Normal = normal;
                Fraction = fraction;
            }
        }

        private void AddHitEvent(Contact contact, System.Collections.Generic.List<ContactHitEvent> hitEvents)
        {
            if (contact.FixtureA == null || contact.FixtureB == null)
            {
                return;
            }

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;
            Body bodyA = fixtureA.Body;
            Body bodyB = fixtureB.Body;

            float radiusA = GetShapeRadius(fixtureA.Shape);
            float radiusB = GetShapeRadius(fixtureB.Shape);
            WorldManifold worldManifold = new WorldManifold();
            worldManifold.Initialize(contact.Manifold, bodyA.Transform, radiusA, bodyB.Transform, radiusB);

            Vec2 normal = worldManifold.Normal;
            if (contact.Manifold.PointCount == 0)
            {
                return;
            }

            Vec2 point = worldManifold.Points[0];
            Vec2 centerA = bodyA.GetWorldCenter();
            Vec2 centerB = bodyB.GetWorldCenter();
            Vec2 rA = point - centerA;
            Vec2 rB = point - centerB;
            Vec2 vA = bodyA.LinearVelocity + Vec2.Cross(bodyA.AngularVelocity, rA);
            Vec2 vB = bodyB.LinearVelocity + Vec2.Cross(bodyB.AngularVelocity, rB);
            float speed = MathF.Abs(Vec2.Dot(vB - vA, normal));

            if (speed >= _def.HitEventThreshold)
            {
                hitEvents.Add(new ContactHitEvent(fixtureA.UserData, fixtureB.UserData, point, normal, speed));
            }
        }

        private void UpdateSleep(float timeStep)
        {
            if (_awakeSet.Islands.Count == 0)
            {
                return;
            }

            float linearTolSqr = Constants.LinearSleepTolerance * Constants.LinearSleepTolerance;
            float angularTol = Constants.AngularSleepTolerance;

            for (int i = 0; i < _awakeSet.Islands.Count; ++i)
            {
                Island island = _awakeSet.Islands[i];
                float minSleepTime = float.MaxValue;

                for (int j = 0; j < island.Bodies.Count; ++j)
                {
                    Body body = island.Bodies[j];
                    if (body.Type == BodyType.Static)
                    {
                        continue;
                    }

                    if (body.AllowSleep == false ||
                        body.LinearVelocity.LengthSquared > linearTolSqr ||
                        MathF.Abs(body.AngularVelocity) > angularTol)
                    {
                        body.SleepTime = 0f;
                        minSleepTime = 0f;
                    }
                    else
                    {
                        body.SleepTime += timeStep;
                        minSleepTime = MathF.Min(minSleepTime, body.SleepTime);
                    }
                }

                if (minSleepTime >= Constants.TimeToSleep)
                {
                    island.IsAwake = false;
                    MoveIslandBetweenSets(island, _awakeSet, _sleepingSet);
                    CreateSleepingSetForIsland(island);
                    i--;

                    for (int j = 0; j < island.Bodies.Count; ++j)
                    {
                        Body body = island.Bodies[j];
                        if (body.Type == BodyType.Static || body.AllowSleep == false || body.Awake == false)
                        {
                            continue;
                        }
                        body.SetAwakeFromWorld(false, notifyWorld: false);
                        body.SolverSetType = SolverSetType.Sleeping;
                        _awakeSet.Bodies.Remove(body);
                        if (!_sleepingSet.Bodies.Contains(body))
                        {
                            _sleepingSet.Bodies.Add(body);
                        }
                    }
                }
            }
        }

        private readonly struct ContactKey : System.IEquatable<ContactKey>
        {
            public readonly int A;
            public readonly int B;

            public ContactKey(int proxyIdA, int proxyIdB)
            {
                if (proxyIdA < proxyIdB)
                {
                    A = proxyIdA;
                    B = proxyIdB;
                }
                else
                {
                    A = proxyIdB;
                    B = proxyIdA;
                }
            }

            public bool Equals(ContactKey other) => A == other.A && B == other.B;
            public override bool Equals(object? obj) => obj is ContactKey other && Equals(other);
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = (hash * 31) + A.GetHashCode();
                    hash = (hash * 31) + B.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
