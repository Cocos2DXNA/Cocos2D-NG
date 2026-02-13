using System.Diagnostics;
using System;

namespace CocosNG.Core.Actions.Interval
{
    public interface ICCActionTweenDelegate
    {
        void UpdateTweenAction(float value, string key);
    }

    public class CCActionTween : CCActionInterval
    {
        public CCActionTween WithTween(string key, float from, float to, float duration)
        {
            InitWithDuration(duration, key, from, to);
            return this;
        }

        public CCActionTween WithTween(string key, float from, float to, float duration, Action<float, string> tweenAction)
        {
            InitWithDuration(duration, key, from, to);
            _tweenAction = tweenAction;
            return this;
        }

        public CCActionTween WithTweenAction(Action<float, string> tweenAction)
        {
            _tweenAction = tweenAction;
            return this;
        }

        protected float m_fDelta;
        protected float m_fFrom, m_fTo;
        protected string m_strKey;
        protected Action<float, string> _tweenAction;
 
        public CCActionTween(float aDuration, string key, float from, float to)
        {
            InitWithDuration(aDuration, key, from, to);
        }

        public CCActionTween(float aDuration, string key, float from, float to, Action<float,string> tweenAction)
        {
            InitWithDuration(aDuration, key, from, to);
            _tweenAction = tweenAction;
        }

        protected bool InitWithDuration(float aDuration, string key, float from, float to)
        {
            if (base.InitWithDuration(aDuration))
            {
                m_strKey = key;
                m_fTo = to;
                m_fFrom = from;
                return true;
            }

            return false;
        }

        protected internal override void StartWithTarget(CCNode target)
        {
            Debug.Assert(_tweenAction != null || target is ICCActionTweenDelegate, "target must implement CCActionTweenDelegate");
            base.StartWithTarget(target);
            m_fDelta = m_fTo - m_fFrom;
        }

        public override void Update(float dt)
        {
            float amt = m_fTo - m_fDelta * (1 - dt);
            if (_tweenAction != null)
            {
                _tweenAction(amt, m_strKey);
            }
            else if(m_pTarget is ICCActionTweenDelegate)
            {
                ((ICCActionTweenDelegate)m_pTarget).UpdateTweenAction(amt, m_strKey);
            }
        }

        public override CCFiniteTimeAction Reverse()
        {
            return new CCActionTween(m_fDuration, m_strKey, m_fTo, m_fFrom, _tweenAction);
        }
    }
}
