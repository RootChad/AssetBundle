using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ch.kainoo.core
{
    public delegate void ActionFinished();
    public delegate void ActionSuccessful(bool success);
    public delegate void ProgressCallback(float progress);
    public delegate void AssetBundleCallback(AssetBundle bundle);
}