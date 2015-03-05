using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace uTasks
{
    internal class UnityTaskScheduler : TaskScheduler
    {
        private readonly QueueBehaviour _behaviour;

        /// <summary>
        ///     Constructor should be called in main thread.
        /// </summary>
        public UnityTaskScheduler()
        {
            var behaviour = Object.FindObjectsOfType<QueueBehaviour>()
                .SingleOrDefault();

            if (behaviour != null)
            {
                _behaviour = behaviour;
                return;
            }

            var gameObject = new GameObject(typeof (UnityTaskScheduler).Name)
            {
                hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector
            };

            Object.DontDestroyOnLoad(gameObject);
            _behaviour = gameObject.AddComponent<QueueBehaviour>();
        }

        public override void RunInMainThread(Action action)
        {
            _behaviour.Enqueue(action);
        }

        public override void StartCoroutineInMainThread(IEnumerator enumerator)
        {
            RunInMainThread(() => { _behaviour.StartCoroutine(enumerator); });
        }

        public override void StopCoroutineInMainThread(IEnumerator enumerator)
        {
            RunInMainThread(() => { _behaviour.StopCoroutine(enumerator); });
        }
    }
}