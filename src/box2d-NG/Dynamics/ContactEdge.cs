namespace Box2DNG
{
    public sealed class ContactEdge
    {
        public int Id { get; internal set; } = -1;
        public Contact Contact { get; internal set; } = null!;
        public Body Body { get; internal set; } = null!;
        public Body Other { get; internal set; } = null!;
        public ContactEdge? Prev { get; internal set; }
        public ContactEdge? Next { get; internal set; }
    }
}
