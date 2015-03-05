using System;
using System.Collections;

namespace uTasks
{
    public class TaskCompletionSource<TResult>
    {
        public Task<TResult> Task { get; private set; }

        public TaskCompletionSource()
        {
            Task = new Task<TResult>();
        }

        public bool TrySetResult(TResult result)
        {
            var flag = Task.TrySetResult(result);

            if (flag == false && Task.IsCompleted == false)
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletion());
            }

            return flag;
        }

        private IEnumerator WaitForCompletion()
        {
            while (Task.IsCompleted == false)
            {
                yield return null;
            }
        }

        public void SetResult(TResult result)
        {
            var flag = TrySetResult(result);

            if (flag == false)
            {
                throw new InvalidOperationException("Task is already completed.");
            }
        }

        public void SetException(Exception exception)
        {
            var flag = TrySetException(exception);

            if (flag == false)
            {
                throw new InvalidOperationException("Task is already completed.");
            }
        }

        public bool TrySetException(Exception exception)
        {
            var flag = Task.TrySetException(exception);

            if (flag == false && Task.IsCompleted == false)
            {
                TaskScheduler.Current.StartCoroutineInMainThread(WaitForCompletion());
            }

            return flag;
        }
    }
}