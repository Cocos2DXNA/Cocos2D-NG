using CocosNG.Core;

namespace CocosNG.Core.Nodes.Transitions
{
    public class CCTransitionSplitRows : CCTransitionSplitCols
    {
        public override CCActionInterval Action()
        {
            return new CCSplitRows(m_fDuration / 2.0f, 3);
        }
        
        public CCTransitionSplitRows() { }

        public CCTransitionSplitRows(float t, CCScene scene) : base(t, scene)
        {
            InitWithDuration(t, scene);
        }
    }
}
