using System;
using System.Collections;

namespace uTasks
{
    public class Task
    {
        private readonly Action _action;
        private IEnumerator _taskEnumerator;

        public Task()
        {
        }

        public Task(Action action)
        {
            _action = action;
        }

        public AggregateException Exception { get; private set; }
        public bool IsCompleted { get; protected set; }

        protected virtual IEnumerator WaitForMainTaskCompletion()
        {
            var asyncResult = _action.BeginInvoke(null, null);

            while (asyncResult.IsCompleted == false)
            {
                yield return null;
            }

            _action.EndInvoke(asyncResult);
            IsCompleted = true;
        }

        public void Start()
        {
            if (_taskEnumerator != null)
            {
                throw new InvalidOperationException("Task is already started.");
            }

            _taskEnumerator = WaitForMainTaskCompletion();
            TaskScheduler.Current.StartCoroutineInMainThread(_taskEnumerator);
        }

        internal void AddException(Exception exception)
        {
            if (Exception == null)
            {
                Exception = new AggregateException();
            }

            Exception.AddInnerException(exception);
        }

        internal void Finish()
        {
            if (_taskEnumerator != null)
            {
                TaskScheduler.Current.StopCoroutineInMainThread(_taskEnumerator);
            }

            IsCompleted = true;
        }

        public Task ContinueWithTask(Action<Task> action)
        {
            var task = new Task(() => action(this));

            if (IsCompleted)
            {
                task.Start();
            }
            else
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletionAndStart(task));
            }

            return task;
        }

        public Task ThenWithTask(Action action)
        {
            var task = new Task(action);

            if (IsCompleted)
            {
                task.Start();
            }
            else
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletionAndStart(task));
            }

            return task;
        }

        public void CompleteWithAction(Action<Task> action)
        {
            if (IsCompleted)
            {
                action(this);
            }
            else
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletionAndExecute(action));
            }
        }

        public Task<TResult> ThenWithTaskResultAndWaitForInnerResult<TResult>(Func<Task<TResult>> function)
        {
            var tcs = new TaskCompletionSource<TResult>();

            var launchTask = new Task<Task<TResult>>(() =>
            {
                var newTask = function();
                newTask.CompleteWithAction(t => tcs.SetResult(t.Result));
                return newTask;
            });

            if (IsCompleted)
            {
                launchTask.Start();
            }
            else
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletionAndStart(launchTask));
            }

            return tcs.Task;
        }

        #region Enumerations

        private IEnumerator WaitForCompletionAndExecute(Action<Task> action)
        {
            while (IsCompleted == false)
            {
                yield return null;
            }

            action(this);
        }

        protected IEnumerator WaitForCompletionAndStart(Task task)
        {
            while (IsCompleted == false)
            {
                yield return null;
            }

            task.Start();
        }

        #endregion

    }
}