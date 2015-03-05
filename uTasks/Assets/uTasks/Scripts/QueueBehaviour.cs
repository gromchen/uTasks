using System;
using System.Collections.Generic;
using UnityEngine;

namespace uTasks
{
    public class QueueBehaviour : MonoBehaviour
    {
        private readonly Queue<Action> _queue = new Queue<Action>();

        public void Enqueue(Action action)
        {
            _queue.Enqueue(action);
        }

        private void Update()
        {
            if (_queue.Count <= 0) return;
            var action = _queue.Dequeue();
            action();
        }
    }
}