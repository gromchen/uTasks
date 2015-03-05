using System;
using System.Collections;

namespace uTasks
{
    public abstract class TaskScheduler
    {
        private static TaskScheduler _current;

        /// <summary>
        ///     Property should be set in main thread.
        /// </summary>
        public static TaskScheduler Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("Please initialize task scheduler.");

                return _current;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _current = value;
            }
        }

        public abstract void RunInMainThread(Action action);
        public abstract void StartCoroutineInMainThread(IEnumerator enumerator);
        public abstract void StopCoroutineInMainThread(IEnumerator enumerator);
    }
}