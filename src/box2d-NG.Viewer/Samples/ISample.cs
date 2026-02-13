namespace Box2DNG.Viewer.Samples
{
    public interface ISample
    {
        string Name { get; }
        int SubSteps { get; }
        WorldDef CreateWorldDef();
        void Build(World world);
        void Step(World world, float dt);
        void OnKey(char key);
    }
}
