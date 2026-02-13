using System;
using System.Threading;
using System.Threading.Tasks;

namespace CocosNG.Core
{
    public static class CCSchedulerExtensions
    {
        private sealed class SchedulerTaskTarget : ISchedulerTarget
        {
            private readonly CCScheduler _scheduler;
            private readonly TaskCompletionSource<bool> _tcs;
            private readonly Action _action;
            private readonly CancellationToken _cancellationToken;
            private Action<float> _selector;

            public SchedulerTaskTarget(CCScheduler scheduler, TaskCompletionSource<bool> tcs, Action action, CancellationToken cancellationToken)
            {
                _scheduler = scheduler;
                _tcs = tcs;
                _action = action;
                _cancellationToken = cancellationToken;
                _selector = OnTick;
            }

            public void Update(float dt)
            {
            }

            public Action<float> Selector => _selector;

            private void OnTick(float dt)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    TryCancel();
                    return;
                }

                try
                {
                    _action?.Invoke();
                    _tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _tcs.TrySetException(ex);
                }
                finally
                {
                    _scheduler.UnscheduleSelector(_selector, this);
                }
            }

            public void TryCancel()
            {
                _tcs.TrySetCanceled();
                _scheduler.UnscheduleSelector(_selector, this);
            }
        }

        public static Task DelayAsync(this CCScheduler scheduler, float delaySeconds, CancellationToken cancellationToken = default)
        {
            return ScheduleOnceAsync(scheduler, null, delaySeconds, cancellationToken);
        }

        public static Task ScheduleOnceAsync(this CCScheduler scheduler, Action action, float delaySeconds = 0f, CancellationToken cancellationToken = default)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var target = new SchedulerTaskTarget(scheduler, tcs, action, cancellationToken);

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(target.TryCancel);
            }

            scheduler.ScheduleSelector(target.Selector, target, 0f, 0, delaySeconds, false);

            return tcs.Task;
        }

        public static Task ScheduleOnceAsync(this CCScheduler scheduler, Action<float> action, float delaySeconds = 0f, CancellationToken cancellationToken = default)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var target = new SchedulerTaskTarget(scheduler, tcs, null, cancellationToken);
            Action<float> selector = dt =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.TrySetCanceled();
                    scheduler.UnscheduleSelector(selector, target);
                    return;
                }

                try
                {
                    action?.Invoke(dt);
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                finally
                {
                    scheduler.UnscheduleSelector(selector, target);
                }
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(target.TryCancel);
            }

            scheduler.ScheduleSelector(selector, target, 0f, 0, delaySeconds, false);

            return tcs.Task;
        }

        public static Task ScheduleOnceAsync(this CCScheduler scheduler, Action<float> action, float delaySeconds, bool passElapsedTime, CancellationToken cancellationToken = default)
        {
            if (!passElapsedTime)
            {
                return ScheduleOnceAsync(scheduler, action, delaySeconds, cancellationToken);
            }

            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var target = new SchedulerTaskTarget(scheduler, tcs, null, cancellationToken);
            var elapsed = 0f;

            Action<float> selector = dt =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.TrySetCanceled();
                    scheduler.UnscheduleSelector(selector, target);
                    return;
                }

                elapsed += dt;
                if (elapsed < delaySeconds)
                {
                    return;
                }

                try
                {
                    action?.Invoke(elapsed);
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
                finally
                {
                    scheduler.UnscheduleSelector(selector, target);
                }
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(target.TryCancel);
            }

            scheduler.ScheduleSelector(selector, target, 0f, CCScheduler.kCCRepeatForever, 0f, false);

            return tcs.Task;
        }

        public static Task ScheduleRepeatingAsync(this CCScheduler scheduler, Action<float> action, float intervalSeconds, CancellationToken cancellationToken = default)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var target = new SchedulerTaskTarget(scheduler, tcs, null, cancellationToken);

            Action<float> selector = dt =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    target.TryCancel();
                    return;
                }

                try
                {
                    action?.Invoke(dt);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                    scheduler.UnscheduleSelector(selector, target);
                }
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(target.TryCancel);
            }

            scheduler.ScheduleSelector(selector, target, intervalSeconds, CCScheduler.kCCRepeatForever, 0f, false);

            return tcs.Task;
        }

        public static Task ScheduleRepeatingAsync(this CCScheduler scheduler, Action<float> action, float intervalSeconds, float initialDelaySeconds, CancellationToken cancellationToken = default)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException(nameof(scheduler));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            var tcs = new TaskCompletionSource<bool>();
            var target = new SchedulerTaskTarget(scheduler, tcs, null, cancellationToken);

            Action<float> selector = dt =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    target.TryCancel();
                    return;
                }

                try
                {
                    action?.Invoke(dt);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                    scheduler.UnscheduleSelector(selector, target);
                }
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(target.TryCancel);
            }

            scheduler.ScheduleSelector(selector, target, intervalSeconds, CCScheduler.kCCRepeatForever, initialDelaySeconds, false);

            return tcs.Task;
        }
    }
}
