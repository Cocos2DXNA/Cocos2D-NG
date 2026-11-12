namespace CocosNG.Core.Nodes
{
    /// <summary>
    /// Fluent helpers for building node trees. Children live in CCNode's own child list,
    /// so drawing, z-ordering and the node lifecycle work as usual.
    /// </summary>
    public class CCContainer : CCNode
    {
        public CCContainer Add(CCNode child, int zOrder = 0)
        {
            AddChild(child, zOrder);
            return this;
        }

        public CCContainer Remove(CCNode child, bool cleanup = true)
        {
            RemoveChild(child, cleanup);
            return this;
        }
    }
}
