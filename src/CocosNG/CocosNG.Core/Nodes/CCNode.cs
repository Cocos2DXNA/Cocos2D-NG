namespace CocosNG.Core.Nodes
{
    public class CCNode
    {
        public virtual CocosNG.Core.Primitives.CCPoint Position { get; set; }
        public virtual float Z { get; set; }
        public virtual float Rotation { get; set; }
        public virtual float Scale { get; set; } = 1f;
        public virtual bool Visible { get; set; } = true;

        public virtual CCNode Init()
        {
            return this;
        }

        public CCNode WithPosition(CocosNG.Core.Primitives.CCPoint position)
        {
            Position = position;
            return this;
        }

        public CCNode WithZ(float z)
        {
            Z = z;
            return this;
        }

        public CCNode WithRotation(float rotation)
        {
            Rotation = rotation;
            return this;
        }

        public CCNode WithScale(float scale)
        {
            Scale = scale;
            return this;
        }

        public CCNode WithVisible(bool visible)
        {
            Visible = visible;
            return this;
        }
    }
}
