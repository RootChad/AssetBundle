using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace ch.kainoo.core.utilities.async
{

    public class AsyncExecutor
    {
        public const string NAME = "ASYNC_EXECUTOR";

        private static GameObject parentElement = null;
        private static uint executionIndex = 0;

        public static void ExecuteAsync(IEnumerator enumerator)
        {
            var executor = SpawnExecutor();
            executor.StartCoroutine(_ExecuteAndCleanup(executor, enumerator));
        }

        private static IEnumerator _ExecuteAndCleanup(MonoBehaviour behavior, IEnumerator enumerator)
        {
            try
            {
                yield return behavior.StartCoroutine(enumerator);
            }
            finally
            {
                GameObject.Destroy(behavior.gameObject);
            }
        }

        private static MonoBehaviour SpawnExecutor()
        {
            if (parentElement == null)
            {
                parentElement = new GameObject(NAME);
            }

            var newElement = new GameObject($"Executor - {executionIndex++}");
            newElement.transform.SetParent(parentElement.transform);

            return newElement.AddComponent<ExecBehavior>();
        }



        public class ExecBehavior : MonoBehaviour { }

    }

}