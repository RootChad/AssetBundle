using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ch.kainoo.core
{

    public class ProgressAggregator
    {
        private IProgress<float> _callback;

        private double[] _subProgresses;
        private double[] _subProgressTotals;

        public ProgressAggregator(IProgress<float> callback, int numberOfSubprogresses)
        {
            _callback = callback;

            _subProgresses = new double[numberOfSubprogresses];
            _subProgressTotals = Enumerable
                    .Range(0, numberOfSubprogresses)
                    .Select(i => 1.0)
                    .ToArray();
        }

        public ProgressAggregator(IProgress<float> callback, double[] subprogressTotals)
        {
            _callback = callback;

            _subProgresses = Enumerable
                    .Range(0, subprogressTotals.Length)
                    .Select(i => 0.0)
                    .ToArray();
            _subProgressTotals = subprogressTotals.ToArray();
        }


        public void Update(int subProgressIdx, double progress)
        {
            if (progress < 0) progress = 0;
            if (progress > _subProgressTotals[subProgressIdx]) progress = _subProgressTotals[subProgressIdx];

            _subProgresses[subProgressIdx] = progress;

            _callback?.Report(Progress);
        }

        public float Progress
        {
            get
            {
                var unclampedProgress = (float)(_subProgresses.Sum() / _subProgressTotals.Sum());
                return Mathf.Clamp(unclampedProgress, 0, 1);
            }
        }

    }

}