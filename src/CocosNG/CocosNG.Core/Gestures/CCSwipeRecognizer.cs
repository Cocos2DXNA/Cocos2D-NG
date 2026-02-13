using CocosNG.Core.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CocosNG.Core.Gestures
{
    public class CCSwipeRecognizer
    {
        private readonly CCNode _target;
        private Vector2 _start;

        public event Action<Vector2, Vector2>? OnSwipe;
        // start → end

        public CCSwipeRecognizer(CCNode target)
        {
            _target = target;
            _target.OnTouchBegan += t => _start = t.Position;
            _target.OnTouchEnded += t => {
                var delta = t.Position - _start;
                if (delta.Length() > 50f)
                { // threshold
                    OnSwipe?.Invoke(_start, t.Position);
                }
            };
        }
    }
}
