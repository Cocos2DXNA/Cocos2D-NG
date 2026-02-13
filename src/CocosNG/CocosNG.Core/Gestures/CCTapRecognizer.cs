using CocosNG.Core.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocosNG.Core.Gestures
{
    public class CCTapRecognizer
    {
        private readonly CCNode _target;
        private DateTime _lastTap = DateTime.MinValue;

        public event Action? OnTap;
        public event Action? OnDoubleTap;

        public CCTapRecognizer(CCNode target)
        {
            _target = target;
            _target.OnTouchEnded += t => {
                var now = DateTime.Now;
                if ((now - _lastTap).TotalMilliseconds < 250)
                {
                    OnDoubleTap?.Invoke();
                    _lastTap = DateTime.MinValue;
                }
                else
                {
                    OnTap?.Invoke();
                    _lastTap = now;
                }
            };
        }
    }
}
