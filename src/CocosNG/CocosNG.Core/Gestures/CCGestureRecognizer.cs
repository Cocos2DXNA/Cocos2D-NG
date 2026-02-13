using CocosNG.Core.Nodes;
using CocosNG.Core.Nodes;

namespace CocosNG.Core.Gestures
{
    public abstract class CCGestureRecognizer : IGestureRecognizer
    {
        public CCNode Target { get; private set; }

        public void Attach(CCNode node)
        {
            Target = node;
            node.OnTouchBegan += HandleTouchBegan;
            node.OnTouchMoved += HandleTouchMoved;
            node.OnTouchEnded += HandleTouchEnded;
        }

        public void Detach()
        {
            if (Target != null)
            {
                Target.OnTouchBegan -= HandleTouchBegan;
                Target.OnTouchMoved -= HandleTouchMoved;
                Target.OnTouchEnded -= HandleTouchEnded;
                Target = null;
            }
        }

        protected virtual void HandleTouchBegan(CCTouch touch) { }
        protected virtual void HandleTouchMoved(CCTouch touch) { }
        protected virtual void HandleTouchEnded(CCTouch touch) { }
    }
}