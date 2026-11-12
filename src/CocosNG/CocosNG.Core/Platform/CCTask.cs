using System;
#if WINDOWS_PHONE|| XBOX360
using System.ComponentModel;
#else
using System.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;

namespace CocosNG.Core
{
    public static class CCTask
    {

        #region Threading
            /// <summary>
            /// Checks if the code is currently running on the UI thread.
            /// </summary>
            /// <returns>true if the code is currently running on the UI thread.</returns>
            public static bool IsOnUIThread()
            {
#if MONOGAME && WINDOWS_PHONE
                return (Microsoft.Xna.Framework.Threading.IsOnUIThread());
#else
                return (true);
#endif

            }

            /// <summary>
            /// Throws an exception if the code is not currently running on the UI thread.
            /// </summary>
            /// <exception cref="InvalidOperationException">Thrown if the code is not currently running on the UI thread.</exception>
            public static void EnsureUIThread()
            {
#if MONOGAME && WINDOWS_PHONE
                Microsoft.Xna.Framework.Threading.EnsureUIThread();
#endif
            }

            public static void RunOnUiThread(Action action)
            {
#if WINDOWS_PHONE && MONOGAME
                Microsoft.Xna.Framework.Threading.RunOnUIThread(action);
#else
                action();
#endif
            }

#if WINDOWS_PHONE && !XNA

            public static void RunOnContainerThread(System.Windows.Threading.Dispatcher target, Action action)
            {
                Microsoft.Xna.Framework.Threading.RunOnContainerThread(target, action);
            }

            public static void BlockOnContainerThread(System.Windows.Threading.Dispatcher target, Action action)
            {
                Microsoft.Xna.Framework.Threading.BlockOnContainerThread(target, action);
            }
#endif

            /// <summary>
            /// Runs the given action on the UI thread and blocks the current thread while the action is running.
            /// If the current thread is the UI thread, the action will run immediately.
            /// </summary>
            /// <param name="action">The action to be run on the UI thread</param>
            public static void BlockOnUIThread(Action action)
            {
#if WINDOWS_PHONE && MONOGAME
                Microsoft.Xna.Framework.Threading.BlockOnUIThread(action);
#else
                action();
#endif
            }

        #endregion

        private class TaskSelector : ICCSelectorProtocol
        {
            public void Update(float dt)
            {
            }
        }

        private static ICCSelectorProtocol _taskSelector = new TaskSelector();

        public static void RunOnScheduler(Action action)
        {
            // Thread-safe: runs on the main thread at the start of the next scheduler tick.
            s_mainThreadQueue.Enqueue(action);
        }

        private static readonly System.Collections.Concurrent.ConcurrentQueue<Action> s_mainThreadQueue =
            new System.Collections.Concurrent.ConcurrentQueue<Action>();

        /// <summary>
        /// Runs queued work on the calling (main) thread. Called by CCScheduler each tick, so
        /// background tasks never touch the scheduler directly.
        /// </summary>
        internal static void RunPendingOnMainThread()
        {
            Action action;
            while (s_mainThreadQueue.TryDequeue(out action))
            {
                action();
            }
        }

        public static object RunAsync(Action action)
        {
            return RunAsync(action, null);
        }
		
        public static object RunAsync(Action action, Action<object> taskCompleted)
        {
#if WINDOWS_PHONE || XBOX360
            var worker = new BackgroundWorker();
            
            worker.DoWork +=
                (sender, args) =>
                {
                    action();
                };

            if (taskCompleted != null)
            {
                worker.RunWorkerCompleted +=
                    (sender, args) =>
                    {
                        var scheduler = CCDirector.SharedDirector.Scheduler;
                        scheduler.ScheduleSelector(f => taskCompleted(worker), _taskSelector, 0, 0, 0, false);
                    };
            }

            worker.RunWorkerAsync();

            return worker;
#else
            var task = new Task(
                () =>
                {
                    action();

                    if (taskCompleted != null)
                    {
                        RunOnScheduler(() => taskCompleted(null));
                    }
                }
                );
                    
            task.Start();

            return task;
#endif
        }
    }
}
