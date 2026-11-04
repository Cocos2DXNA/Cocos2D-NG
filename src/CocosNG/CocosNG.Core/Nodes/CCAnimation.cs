namespace CocosNG.Core.Nodes
{
    public class CCAnimation
    {
        public float DelayPerUnit { get; set; }

        public uint Loops { get; set; } = 1;

        public bool RestoreOriginalFrame { get; set; }

        public CCAnimation WithDelayPerUnit(float delayPerUnit)
        {
            DelayPerUnit = delayPerUnit;
            return this;
        }

        public CCAnimation WithLoops(uint loops)
        {
            Loops = loops;
            return this;
        }

        public CCAnimation WithRestoreOriginalFrame(bool restore)
        {
            RestoreOriginalFrame = restore;
            return this;
        }
    }
}
