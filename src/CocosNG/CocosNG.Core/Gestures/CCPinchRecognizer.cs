using CocosNG.Core.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CocosNG.Core.Gestures
{
    public class CCPinchRecognizer
    {
        private readonly CCNode _target;
        private Dictionary<int, Vector2> _activeTouches = new();

        public event Action<float>? OnPinch; // scale delta

        public CCPinchRecognizer(CCNode target)
        {
            _target = target;
            _target.OnTouchBegan += t => _activeTouches[t.Id] = t.Position;
            _target.OnTouchMoved += t => {
                _activeTouches[t.Id] = t.Position;
                if (_activeTouches.Count == 2)
                {
                    var positions = _activeTouches.Values.ToArray();
                    var dist = Vector2.Distance(positions[0], positions[1]);
                    OnPinch?.Invoke(dist);
                }
            };
            _target.OnTouchEnded += t => _activeTouches.Remove(t.Id);
        }
    }
}
