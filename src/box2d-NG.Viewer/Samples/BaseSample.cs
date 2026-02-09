namespace Box2DNG.Viewer.Samples
{
    public abstract class BaseSample : ISample
    {
        public abstract string Name { get; }
        public virtual int SubSteps => 1;

        public virtual WorldDef CreateWorldDef() => new WorldDef();

        public abstract void Build(World world);

        public virtual void Step(World world, float dt)
        {
        }

        public virtual void OnKey(char key)
        {
        }
    }
}
