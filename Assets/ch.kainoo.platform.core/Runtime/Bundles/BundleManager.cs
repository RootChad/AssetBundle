using ch.kainoo.core;
using ch.kainoo.core.utilities.extensions;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ch.kainoo.platform
{
    public delegate void BundleLoaded(string bundleName, AssetBundle bundle);

    [Serializable]
    public class BundleInformation
    {
        public string BundleIdentifier;
        public string Variant = "";
        public string[] Dependencies;

        public BundleInformation(string bundleName, string[] dependencies)
        {
            var (bid, bvar) = BundleUtilities.SplitBundleName(bundleName);
            BundleIdentifier = bid;
            Variant = bvar;
            Dependencies = dependencies;
        }

        public override string ToString()
        {
            return BundleIdentifier;
        }

        public static implicit operator string(BundleInformation bundleName)
        {
            return bundleName.ToString();
        }
    }

    public class BundleManager : MonoBehaviour
    {
        public static string BASE_URL => Path.Combine(Application.streamingAssetsPath, "assets");

        public static BundleManager Instance { get; private set; }

        private Dictionary<string, HashSet<BundleLoaded>> _bundledLoadedCallbacks = new Dictionary<string, HashSet<BundleLoaded>>();

        public bool AddListenerBundleLoaded(string bundleName, BundleLoaded callback)
        {
            if (_bundledLoadedCallbacks.TryGetValue(bundleName, out var set))
            {
                return set.Add(callback);
            }
            else
            {
                _bundledLoadedCallbacks.Add(bundleName, new HashSet<BundleLoaded>() { callback });
                return true;
            }
        }

        public bool RemoveListenerBundleLoaded(string bundleName, BundleLoaded callback)
        {
            if (_bundledLoadedCallbacks.TryGetValue(bundleName, out var set))
            {
                return set.Remove(callback);
            }
            else
            {
                return false;
            }
        }

        private void InvokeBundleLoaded(string bundleName, AssetBundle bundle)
        {
            if (_bundledLoadedCallbacks.TryGetValue(bundleName, out var set))
            {
                var setCopy = set.ToArray();
                foreach (var callback in setCopy)
                {
                    try
                    {
                        callback.Invoke(bundleName, bundle);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogException(exc);
                    }
                }
            }
        }

        [SerializeField]
        private IPlatformConfiguration m_configuration;

        private HashSet<string> _loadedBundles = new HashSet<string>();
        private List<AssetBundle> _loadedSceneBundles = new List<AssetBundle>();

        private void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("Bundle Manager already defined");
                return;
            }

            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance != this)
            {
                Debug.LogError("Another Bundle Manager was active, cannot unregister");
                return;
            }

            Instance = null;
        }

        public bool IsLoaded(string bundleName)
        {
            return _loadedBundles.Contains(bundleName);
        }

        public async Task<AssetBundle> LoadBundleIncludingDependencies(string bundleName, IProgress<float> callback = null)
        {
            AssetBundle bundle;
            // Early exit if the bundle is already loaded
            if (TryGetAssetBundle(bundleName, out bundle))
            {
                DebugEO.Log($"Got {bundleName} from memory");
                return bundle;
            }

            DebugEO.Log($"Downloading {bundleName}");
            var bundleDefTree = await m_configuration.GetBundleInfoDependencyTree(bundleName);
            var bundleList = bundleDefTree.ToList(listMode: TreeNodeListMode.BottomUp, removeDuplicates: true);

            var progress = new ProgressAggregator(callback, bundleList.Select(b => (double)b.Variants[0].Size).ToArray());

            var taskList = new List<UniTask<AssetBundle>>();
            int i = 0;
            foreach (var b in bundleList)
            {
                int idx = i++;
                // TODO: reimplement progress correctly
                // The callback chains breaks loading somehow
                var newCallback = new Progress<float>((p) =>
                {
                    int j = idx;
                    progress.Update(j, p * b.Variants[0].Size);
                });
                taskList.Add(LoadBundle(b, newCallback));
            }

            var results = await UniTask.WhenAll(taskList);

            // Return last bundle aka main bundle
            return results[results.Length - 1];
        }

        public async UniTask<AssetBundle> LoadBundle(BundleDefinition bundleDef, IProgress<float> callback = null)
        {
            // Prepare download path
            var uri = Path.Combine(BASE_URL, bundleDef.BundleName + ".bundle");
            Debug.Log($"Getting bundle '{bundleDef.BundleName}' at '{uri}'");

            AssetBundle bundle;
            if (TryGetAssetBundle(bundleDef.BundleName, out bundle))
            {
                DebugEO.Log($"Got {bundleDef.BundleName} from memory");
                return bundle;
            }

            // Download the asset bundle
            var downloadedBytes = await LoadByteData(uri, callback);

            // Load Asset Bundle from received bytes
            var loadOperation = AssetBundle.LoadFromMemoryAsync(downloadedBytes);
            while (!loadOperation.isDone)
            {
                //callback?.Invoke(Mathf.Lerp(0.9f, 1.0f, loadOperation.progress));
                await UniTask.Yield();
            }

            // Locally track the name of the loaded bundle.
            // The asset bundle can be retrieved via the `AssetBundle` class if needed.
            DebugEO.Log($"Loaded bundle '{bundleDef.BundleName}'");
            _loadedBundles.Add(bundleDef.BundleName);

            // Prepare and return bundle
            bundle = loadOperation.assetBundle;
            bundle.name = bundleDef.BundleName;

            // Unity does not add the scene bundles as loaded asset bundles in their
            // AssetBundle class. So if you call `AssetBundle.GetAllLoadedAssetBundles()` 
            // it will not return scene bundles. Therefore we track them manually below:
            if (!TryGetAssetBundle(bundleDef.BundleName, out _))
            {
                _loadedSceneBundles.Add(bundle);
            }

            // Invoke listeners
            InvokeBundleLoaded(bundle.name, bundle);

            // Update progress to 1.0 in case a rounding error gives back 0.99371...
            callback?.Report(1.0f);

            return bundle;
        }

        private async UniTask<byte[]> LoadByteData(string uri, IProgress<float> callback = null)
        {
            // Download bundle data
            var uwr = UnityWebRequest.Get(uri);
            var asyncOp = uwr.SendWebRequest();

            while (!asyncOp.isDone)
            {
                callback?.Report(uwr.downloadProgress);
                await UniTask.Yield();
            }
            callback?.Report(1f);

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                DebugEO.LogError($"Request for {uwr.uri} failed");
            }

            var downloadHandler = uwr.downloadHandler;
            return downloadHandler.data;
        }

        public bool TryGetAssetBundle(string bundleName, out AssetBundle bundle)
        {
            bundle = null;

            if (!_loadedBundles.Contains(bundleName))
            {
                return false;
            }

            foreach (var ab in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (ab.name == bundleName)
                {
                    bundle = ab;
                    return true;
                }
            }

            return false;
            //throw new System.ArgumentOutOfRangeException("Could not find given asset bundle");
        }

        public void UnloadAllBundlesAndAssets()
        {
            DebugEO.Log("Unloading all asset bundles and its contents now");

            foreach (var ab in AssetBundle.GetAllLoadedAssetBundles())
            {
                DebugEO.Log($"Unloading asset bundle '{ab.name}'");
                ab.Unload(true);
            }
            _loadedBundles.Clear();

            foreach (var ab in _loadedSceneBundles)
            {
                DebugEO.Log($"Unloading scene bundle '{ab.name}'");
                ab.Unload(true);
            }
            _loadedSceneBundles.Clear();

            //AssetBundle.UnloadAllAssetBundles(true);
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(BundleManager))]
        class BundleManagerEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var t = target as BundleManager;

                UnityEditor.EditorGUILayout.LabelField($"Loaded bundles ({t._loadedBundles.Count})");
                UnityEditor.EditorGUILayout.TextArea(string.Join("\n", t._loadedBundles));
                UnityEditor.EditorGUILayout.LabelField($"Loaded scene bundles ({t._loadedSceneBundles.Count})");
                UnityEditor.EditorGUILayout.TextArea(string.Join("\n", t._loadedSceneBundles));
            }
        }
#endif

    }

}