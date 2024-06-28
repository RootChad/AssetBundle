using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core
{

    public sealed class TimeProgress : ProgressHandler
    {
        [SerializeField]
        private float progressFillTime = 2f;

        private float startTime = 0f;

        private void OnEnable()
        {
            startTime = Time.time;
        }

        private void Update()
        {
            var timeDiff = (Time.time - startTime) % progressFillTime;
            SetProgress(timeDiff / progressFillTime);
        }

    }

}