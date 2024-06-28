#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class TestSpawn : MonoBehaviour
{

    #region Attributes
    public GameObject bundle;

    public string assetBundlesPath;

    public Texture2D texture;

    public AssetBundle myBundle, textureBundle;

    public GameObject trashObject;
    #endregion

    #region Fonctions
    void Start()
    {
        //Assigner le path de l'assetbundle sur un string
        assetBundlesPath = System.IO.Path.Combine(Application.dataPath, "../AssetBundles/StandaloneWindows/");
        assetBundlesPath = Path.GetFullPath(assetBundlesPath);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            LoadAsset();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            LoadSecond();
        }
    }
    public void LoadAssetUI()
    {
        LoadAsset();
    }
    public void LoadSecondUI()
    {
        LoadSecond();
    }

    public async UniTask LoadSecond()
    {
        textureBundle.Unload(false);
        textureBundle = await AssetBundle.LoadFromFileAsync(assetBundlesPath + "texture.hd");
        texture = textureBundle.LoadAsset<Texture2D>(textureBundle.GetAllAssetNames()[0]);

        trashObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
    }
    

    public async UniTask<String> LoadAsset()
    {

        myBundle = await AssetBundle.LoadFromFileAsync(assetBundlesPath + "object");
        if (myBundle != null)
        {
            Debug.Log("success");
            foreach (var currentBundle in myBundle.GetAllAssetNames())
            {
                Debug.Log(currentBundle);
            }
        }


        textureBundle = await AssetBundle.LoadFromFileAsync(assetBundlesPath + "texture.sd");
        var result = myBundle.LoadAsset<GameObject>(myBundle.GetAllAssetNames()[0]);
        trashObject = Instantiate(result);

        texture = textureBundle.LoadAsset<Texture2D>(textureBundle.GetAllAssetNames()[0]);
        trashObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", texture);
        return "success";
    }
    #endregion
}
#endif