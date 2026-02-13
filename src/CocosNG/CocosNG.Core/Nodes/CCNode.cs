using CocosNG.Core.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using CocosNG.Core;
using CocosNG.Core.Primitives;
using CocosNG.Core.Actions;
using CocosNG.Core.Actions.Interval;

namespace CocosNG.Core.Nodes
{
    internal enum CCNodeTag
    {
        Invalid = -1,
    };


    public class CCNode 
    {
        private readonly List<CCAction> _runningActions = new List<CCAction>();

        public CCPoint Position { get; set; }
        public float Z { get; set; } = 0f;
        public float Rotation { get; set; }
        public float Scale { get; set; }
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Used to break z ties
        /// </summary>
        protected int __seq_id = 0;

        public CCNode? Parent { get; protected set; }

        public CCNode RunAction(CCAction action, Func<CCNode, bool>? predicate = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (predicate == null || predicate(this))
            {
                var director = CCDirector.SharedDirector;
                if (director != null && director.ActionManager != null)
                {
                    director.ActionManager.AddAction(action, this, false);
                }
                else
                {
                    _runningActions.Add(action);
                    action.StartWithTarget(this);
                }
            }

            return this;
        }

        public Task<CCNode> RunActionAsync(CCFiniteTimeAction action, Func<CCNode, bool>? predicate = null)
        {
            return RunActionAsync(action, CancellationToken.None, predicate);
        }

        public Task<CCNode> RunActionAsync(CCFiniteTimeAction action, CancellationToken cancellationToken, Func<CCNode, bool>? predicate = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (predicate != null && !predicate(this))
            {
                return Task.FromResult(this);
            }

            var tcs = new TaskCompletionSource<CCNode>();
            var completion = new CCCallFunc(() => tcs.TrySetResult(this));
            var sequence = new CCSequence(action, completion);

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() =>
                {
                    tcs.TrySetCanceled();
                    var director = CCDirector.SharedDirector;
                    if (director != null && director.ActionManager != null)
                    {
                        director.ActionManager.RemoveAction(sequence);
                    }
                    else
                    {
                        _runningActions.Remove(sequence);
                        sequence.Stop();
                    }
                });
            }

            RunAction(sequence);
            return tcs.Task;
        }

        public Task<CCNode> RunActionAsync(params CCFiniteTimeAction[] actions)
        {
            return RunActionAsync(CancellationToken.None, actions);
        }

        public Task<CCNode> RunActionAsync(CancellationToken cancellationToken, params CCFiniteTimeAction[] actions)
        {
            if (actions == null || actions.Length == 0)
            {
                return Task.FromResult(this);
            }

            if (actions.Length == 1)
            {
                return RunActionAsync(actions[0], cancellationToken);
            }

            return RunActionAsync(new CCSequence(actions), cancellationToken);
        }

        public CCNode WithPosition(CCPoint position)
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

        public CCNode WithUserData(object userData)
        {
            UserData = userData;
            return this;
        }

        public CCNode OnTouchBeganDo(Action<CCTouch> handler)
        {
            OnTouchBegan += handler;
            return this;
        }

        public CCNode OnTouchMovedDo(Action<CCTouch> handler)
        {
            OnTouchMoved += handler;
            return this;
        }

        public CCNode OnTouchEndedDo(Action<CCTouch> handler)
        {
            OnTouchEnded += handler;
            return this;
        }


        public CCNode()
        {
        }


        // Touch event hooks (expressive, composable)
        public event Action<CCTouch>? OnTouchBegan;
        public event Action<CCTouch>? OnTouchMoved;
        public event Action<CCTouch>? OnTouchEnded;

        internal bool HitTest(Vector2 point)
        {
            // default: bounding box check
            return false;
        }

        internal void HandleTouchBegan(CCTouch t) => OnTouchBegan?.Invoke(t);
        internal void HandleTouchMoved(CCTouch t) => OnTouchMoved?.Invoke(t);
        internal void HandleTouchEnded(CCTouch t) => OnTouchEnded?.Invoke(t);

        public virtual CCNode Init()
        {
            return this;
        }

        /// <summary>
        /// Data assigned to this node that is controlled by the application. 
        /// </summary>
        public object UserData { get; set; } = null;

    }
}
