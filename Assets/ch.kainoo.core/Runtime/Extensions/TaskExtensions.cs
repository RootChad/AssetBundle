using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ch.kainoo.core.utilities.async
{

    public static class TaskUtilities
    {
        /// <summary>
        /// 
        /// <para/>
        /// This is essentially Task.Run(System.Action) but we make sure the system is executed on the same thread.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Task RunOnThread(System.Action action)
        {
            var task = new Task(action);
            task.Start(TaskScheduler.FromCurrentSynchronizationContext());
            return task;
        }

    }

    public class ObjectReference<T>
    {
        public T value;
    }

}