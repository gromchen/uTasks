﻿using System;
using System.Collections;

namespace uTasks
{
    public class Task<TResult> : Task
    {
        private readonly Func<TResult> _function;

        public Task(Func<TResult> function)
        {
            _function = function;
        }

        public Task()
        {
        }

        public TResult Result { get; private set; }

        public Task ContinueWithTask(Action<Task<TResult>> action)
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

        public Task<TNewResult> ContinueWithTaskResult<TNewResult>(Func<Task<TResult>, TNewResult> function)
        {
            var task = new Task<TNewResult>(() => function(this));

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

        internal bool TrySetResult(TResult result)
        {
            if (IsCompleted)
            {
                return false;
            }

            Result = result;

            Finish();
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            AddException(exception);
            Finish();
            return true;
        }

        public Task<TNewResult> ThenWithTaskResultAndWaitForInnerResult<TNewResult>(
            Func<TResult, Task<TNewResult>> function)
        {
            var tcs = new TaskCompletionSource<TNewResult>();

            var launchTask = new Task<Task<TNewResult>>(() =>
            {
                var newTask = function(Result);
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

        public Task ThenWithTaskAndWaitForInnerTask(Func<TResult, Task> function)
        {
            var newTask = new Task(() => { function(Result); });

            if (IsCompleted)
            {
                newTask.Start();
            }
            else
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletionAndStart(newTask));
            }

            return newTask;
        }

        public Task<TNewResult> ThenWithTaskResult<TNewResult>(Func<TResult, TNewResult> function)
        {
            var newTask = new Task<TNewResult>(() => function(Result));

            if (IsCompleted)
            {
                newTask.Start();
            }
            else
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletionAndStart(newTask));
            }

            return newTask;
        }

        public void CompleteWithAction(Action<Task<TResult>> action)
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

        #region Enumerations

        private IEnumerator WaitForCompletionAndExecute(Action<Task<TResult>> action)
        {
            while (IsCompleted == false)
            {
                yield return null;
            }

            action(this);
        }

        protected override IEnumerator WaitForMainTaskCompletion()
        {
            var asyncResult = _function.BeginInvoke(null, null);

            while (asyncResult.IsCompleted == false)
            {
                yield return null;
            }

            Result = _function.EndInvoke(asyncResult);
            IsCompleted = true;
        }

        #endregion
    }
}