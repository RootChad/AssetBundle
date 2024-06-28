using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ch.kainoo.core
{
    public interface IProgressHandler
    {
        float GetProgress();
        void SetProgress(float progress);
        bool AddListener(Action<float> callback);
        bool RemoveListener(Action<float> callback);
    }

}
