using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ch.kainoo.platform
{

    [Serializable]
    public class AssetReference<T>
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
        where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        [SerializeField] private T asset;
#endif

        [SerializeField] private string assetPath = "";

#if UNITY_EDITOR
        public T Asset { get { return asset; } }
#endif

        public string AssetPath
        {
            get
            {
#if UNITY_EDITOR
                return GetAssetPathFromAsset();
#else
            return assetPath;
#endif
            }
            set
            {
                assetPath = value;
#if UNITY_EDITOR
                asset = GetAssetFromAssetPath();
#endif
            }
        }

#if UNITY_EDITOR
        private T GetAssetFromAssetPath()
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            else
            {
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
        }

        private string GetAssetPathFromAsset()
        {
            if (asset == null)
            {
                return "";
            }
            else
            {
                return AssetDatabase.GetAssetPath(asset);
            }
        }
#endif

#if UNITY_EDITOR
        public void OnBeforeSerialize()
        {
            if (asset == null)
            {
                asset = GetAssetFromAssetPath();
            }
            else
            {
                assetPath = GetAssetPathFromAsset();
            }
        }

        public void OnAfterDeserialize()
        {
            //asset = GetAssetFromAssetPath();
        }
#endif

    }

}