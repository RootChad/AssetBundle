using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Networking;

public class DeferredOnlineAssetLoader : MonoBehaviour
{
    public string url = "https://bundles-test-project.b-cdn.net/";
    public string sceneBundleName = "scenebundle";
    public string objectBundleName = "objectbundle";
    public string materialBundleName = "materialsbundle";
    public string sceneName = "YourSceneName";

    private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
    private List<Renderer> objectRenderers = new List<Renderer>();
    private Dictionary<string, Texture> lowQualityTextures = new Dictionary<string, Texture>();
    private Dictionary<string, Texture> highQualityTextures = new Dictionary<string, Texture>();
    private Dictionary<string, Material> loadedMaterials = new Dictionary<string, Material>();

    private List<string> lowQualityTexturesBundleInLoading = new List<string>();

    private bool canLoadHighQualityTextures = true;

    void Start()
    {
        LoadScene().Forget();
    }

    private async UniTask LoadScene()
    {
        await LoadSceneBundle();
    }

    public async UniTask LoadSceneBundle()
    {
        string sceneBundleUrl = string.Concat(this.url, sceneBundleName);
        string objectBundleUrl = string.Concat(this.url, objectBundleName);
        string materialBundleUrl = string.Concat(this.url, materialBundleName);

        // Download and put scene bundle inside loadedBundles list 
        await LoadBundle(sceneBundleUrl);

        // Load scene in bundle
        await LoadSceneAdditiveFromBundle(sceneBundleUrl, sceneName);

        // Download and put objects bundle inside loadedBundles list 
        await LoadBundle(objectBundleUrl);

        // Download and put materials bundle inside loadedBundles list 
        await LoadBundle(materialBundleUrl);

        // Load materials inside bundles and put them inside loadedMaterials list
        await LoadMaterialsFromBundle(materialBundleUrl);

        // Load textures for each loaded material
        foreach (var materialName in loadedMaterials.Keys)
        {
            await LoadAndCacheLowTexturesForMaterial(materialName);
        }
    }

    private async UniTask LoadBundle(string bundleUrl)
    {
        if (loadedBundles.ContainsKey(bundleUrl))
        {
            Debug.LogWarning($"Bundle already loaded: {bundleUrl}");
            return;
        }

        UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
        Debug.Log($"Sending web request to : {bundleUrl}");
        await uwr.SendWebRequest();
        if(uwr.isDone && uwr.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Attempting to load bundle: {bundleUrl}");
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            if (bundle != null)
            {
                loadedBundles[bundleUrl] = bundle;
                Debug.Log($"Loaded Bundle: {bundleUrl}");
            }
            else
            {
                Debug.LogError($"Failed to load Bundle: {bundleUrl}");
            }
        }
        else
        {
            Debug.LogError($"Error downloading bundle file: {bundleUrl}");
            return;
        }
    }

    private async UniTask LoadSceneAdditiveFromBundle(string bundleUrl, string sceneName)
    {
        if (loadedBundles.TryGetValue(bundleUrl, out var bundle))
        {
            string[] scenePaths = bundle.GetAllScenePaths();
            foreach (var scenePath in scenePaths)
            {
                if (Path.GetFileNameWithoutExtension(scenePath) == sceneName)
                {
                    var loadSceneAsync = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                    await loadSceneAsync;
                    Debug.Log($"Scene Loaded Additively: {sceneName}");
                    return;
                }
            }
            Debug.LogError($"Scene {sceneName} not found in bundle {bundleUrl}");
        }
        else
        {
            Debug.LogError($"Bundle not found in loaded bundles: {bundleUrl}");
        }
    }

    private async UniTask LoadRenderersFromObjectBundle(string bundleUrl)
    {
        if (loadedBundles.TryGetValue(bundleUrl, out var bundle))
        {
            foreach (var assetName in bundle.GetAllAssetNames())
            {
                var asset = bundle.LoadAsset<GameObject>(assetName);
                if (asset != null)
                {
                    var renderers = asset.GetComponentsInChildren<Renderer>(true); // Get renderers without instantiating the objects
                    objectRenderers.AddRange(renderers);
                    Debug.Log($"Loaded Renderers from Object: {assetName}");
                }
                else
                {
                    Debug.LogError($"Failed to load Object: {assetName}");
                }
            }
        }
        else
        {
            Debug.LogError($"Bundle not found in loaded bundles: {bundleUrl}");
        }
    }

    private async UniTask LoadMaterialsFromBundle(string bundleUrl)
    {
        if (loadedBundles.TryGetValue(bundleUrl, out var bundle))
        {
            foreach (var assetName in bundle.GetAllAssetNames())
            {
                var material = bundle.LoadAsset<Material>(assetName);
                if (material != null)
                {
                    loadedMaterials[Path.GetFileNameWithoutExtension(assetName)] = material;
                    Debug.Log($"Loaded Material: {assetName}");
                }
            }
        }
        else
        {
            Debug.LogError($"Bundle not found in loaded bundles: {bundleUrl}");
        }
    }

    private async UniTask LoadAndCacheLowTexturesForMaterial(string materialName)
    {
        string lowQualityBundleUrl = string.Concat(url, $"{materialName.Replace(' ', '_')}.lowquality");

        lowQualityTexturesBundleInLoading.Add(lowQualityBundleUrl);
        await LoadBundle(lowQualityBundleUrl);
        if (loadedBundles.ContainsKey(lowQualityBundleUrl))
        {
            await LoadTexturesFromBundle(lowQualityBundleUrl, lowQualityTextures, "lowquality");
            UnloadBundle(lowQualityBundleUrl);
        }
        lowQualityTexturesBundleInLoading.Remove(lowQualityBundleUrl);
        StartCoroutine(CheckForHighQualityTextureLoading());
        Material mat = loadedMaterials[materialName];
        AssignTextureVariant(mat, lowQualityTextures, "lowquality");
    }

    private System.Collections.IEnumerator CheckForHighQualityTextureLoading()
    {
        // Wait for all low quality bundles loading to be added to the lowQualityTexturesBundleInLoading list
        yield return new WaitForSeconds(.1f);
        if(lowQualityTexturesBundleInLoading.Count != 0 || !canLoadHighQualityTextures) { yield break; }

        canLoadHighQualityTextures = false;
        yield return new WaitForSeconds(5f);
        Debug.Log("Loading high textures");
        // Load textures for the loaded materials
        foreach (var materialName in loadedMaterials.Keys)
        {
            LoadAndCacheHighTexturesForMaterial(materialName);
        }
    }

    private async UniTask LoadAndCacheHighTexturesForMaterial(string materialName)
    {
        string highQualityBundleUrl = string.Concat(url, $"{materialName.Replace(' ', '_')}.highquality");

        await LoadBundle(highQualityBundleUrl);
        if (loadedBundles.ContainsKey(highQualityBundleUrl))
        {
            await LoadTexturesFromBundle(highQualityBundleUrl, highQualityTextures, "highquality");
            UnloadBundle(highQualityBundleUrl);
        }
        Material mat = loadedMaterials[materialName];
        AssignTextureVariant(mat, highQualityTextures, "highquality");
    }

    private async UniTask LoadTexturesFromBundle(string bundleUrl, Dictionary<string, Texture> textureDictionary, string variantKeyword)
    {
        if (loadedBundles.TryGetValue(bundleUrl, out var bundle))
        {
            foreach (var assetName in bundle.GetAllAssetNames())
            {
                var texture = bundle.LoadAsset<Texture>(assetName);
                if (texture != null)
                {
                    string textureKey = Path.GetFileNameWithoutExtension(assetName);
                    Debug.Log($"Adding {variantKeyword} Texture: {textureKey}");
                    textureDictionary[textureKey] = texture;
                }
            }
        }
        else
        {
            Debug.LogError($"Bundle not found in loaded bundles: {bundleUrl}");
        }
    }

    private void UnloadBundle(string bundleUrl)
    {
        if (loadedBundles.TryGetValue(bundleUrl, out var bundle))
        {
            bundle.Unload(false);
            loadedBundles.Remove(bundleUrl);
            Debug.Log($"Unloaded Bundle: {bundleUrl}");
        }
    }

    private void DebugTextureDictionaries()
    {
        Debug.Log("Debugging highQualityTextures:");
        foreach (var kvp in highQualityTextures)
        {
            Debug.Log($"Key: {kvp.Key}, Texture: {kvp.Value.name}");
        }

        Debug.Log("Debugging lowQualityTextures:");
        foreach (var kvp in lowQualityTextures)
        {
            Debug.Log($"Key: {kvp.Key}, Texture: {kvp.Value.name}");
        }
    }

    public async UniTask LoadLowQualityTextures()
    {
        Debug.Log("Assigning Low-Quality Textures");
        foreach (var renderer in objectRenderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                AssignTextureVariant(mat, lowQualityTextures, "lowquality");
            }
        }
    }

    public async UniTask LoadHighQualityTextures()
    {
        Debug.Log("Assigning High-Quality Textures");
        foreach (var renderer in objectRenderers)
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                AssignTextureVariant(mat, highQualityTextures, "highquality");
            }
        }
    }

    private void AssignTextureVariant(Material mat, Dictionary<string, Texture> textureVariants, string variantKeyword)
    {

        foreach (var texturePropertyName in mat.GetTexturePropertyNames())
        {
            Texture originalTexture = mat.GetTexture(texturePropertyName);
            if (originalTexture != null)
            {
                string textureName = originalTexture.name.Replace(" (Instance)", "").ToLower();
                Debug.Log($"Checking for texture variant: {textureName}");
                if (textureVariants.TryGetValue(textureName, out Texture variantTexture))
                {
                    mat.SetTexture(texturePropertyName, variantTexture);
                    Debug.Log($"Assigned {variantKeyword} texture {variantTexture.name} to material {mat.name}");
                }
                else
                {
                    Debug.LogWarning($"Texture variant not found: {textureName}");
                }
            }
        }
    }
}
