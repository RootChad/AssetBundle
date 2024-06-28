using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core
{

    public abstract class ProgressHandler : MonoBehaviour, IProgressHandler
    {
        private float _progress = 0f;

        private HashSet<Action<float>> actions = new HashSet<Action<float>>();

        public float GetProgress()
        {
            return _progress;
        }

        public void SetProgress(float progress)
        {
            _progress = Mathf.Clamp01(progress);

            foreach (var action in actions)
            {
                action.Invoke(_progress);
            }
        }

        public bool AddListener(Action<float> callback)
        {
            return actions.Add(callback);
        }

        public bool RemoveListener(Action<float> callback)
        {
            return actions.Remove(callback);
        }
    }

}