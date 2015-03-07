using System;
using System.Collections;

namespace uTasks
{
    public class Task
    {
        private readonly Action _action;
        private CancellationToken _cancellationToken;
        private IEnumerator _taskEnumerator;

        public Task()
        {
            Status = TaskStatus.Created;
        }

        public Task(Action action) : this()
        {
            _action = action;
        }

        public AggregateException Exception { get; private set; }

        public bool IsCompleted
        {
            get { return Status == TaskStatus.RanToCompletion; }
        }

        public TaskStatus Status { get; protected set; }

        public bool IsFaulted
        {
            get { return Status == TaskStatus.Faulted; }
        }

        public bool IsCanceled
        {
            get { return Status == TaskStatus.Canceled || Status == TaskStatus.Faulted; }
        }

        protected void RecordInternalCancellationRequest(CancellationToken tokenToRecord,
            Exception cancellationException)
        {
            _cancellationToken = tokenToRecord;
            AddException(cancellationException);
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

        internal void Finish(TaskStatus status = TaskStatus.RanToCompletion)
        {
            if (_taskEnumerator != null)
            {
                TaskScheduler.Current.StopCoroutineInMainThread(_taskEnumerator);
            }

            Status = status;
        }

        public Task ContinueWithTask(Action<Task> action)
        {
            return ThenWith(new Task(() => action(this)));
        }

        public Task ThenWithTask(Action action)
        {
            return ThenWith(new Task(action));
        }

        public Task ThenWithTask(Func<Task> function)
        {
            return ThenWith(new Task<Task>(function));
        }

        public Task ThenWithTask<T1, T2>(Func<T1, T2, Task> successor, T1 arg1, T2 arg2)
        {
            return ThenWith(new Task<Task>(() => successor(arg1, arg2)));
        }

        private Task ThenWith(Task task)
        {
            switch (Status)
            {
                case TaskStatus.Faulted:
                case TaskStatus.Canceled:
                    return this;

                case TaskStatus.RanToCompletion:
                    task.Start();
                    break;

                default:
                    TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletionAndStart(task));
                    break;
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

        protected virtual IEnumerator WaitForMainTaskCompletion()
        {
            var asyncResult = _action.BeginInvoke(null, null);
            Status = TaskStatus.Running;

            while (asyncResult.IsCompleted == false)
            {
                yield return null;
            }

            try
            {
                _action.EndInvoke(asyncResult);
                Status = TaskStatus.RanToCompletion;
            }
            catch (Exception exception)
            {
                AddException(exception);
                Status = TaskStatus.Faulted;
            }
        }

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