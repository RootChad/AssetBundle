using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class DeferredAssetLoader : MonoBehaviour
{
    public string sceneBundleUrl = Path.Combine(Application.streamingAssetsPath, "scenebundle");
    public string objectBundleUrl = Path.Combine(Application.streamingAssetsPath, "objectbundle");
    public string materialBundleUrl = Path.Combine(Application.streamingAssetsPath, "materialsbundle");
    public string sceneName = "YourSceneName";
    public Button loadLowQualityButton;
    public Button loadHighQualityButton;
    public Button loadSceneButton;
    public Button DebugTexturesLogButton;

    private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
    private List<Renderer> objectRenderers = new List<Renderer>();
    private Dictionary<string, Texture> lowQualityTextures = new Dictionary<string, Texture>();
    private Dictionary<string, Texture> highQualityTextures = new Dictionary<string, Texture>();
    private Dictionary<string, Material> loadedMaterials = new Dictionary<string, Material>();

    void Start()
    {
        // Assign button listeners
        loadLowQualityButton.onClick.AddListener(() => LoadLowQualityTextures().Forget());
        loadHighQualityButton.onClick.AddListener(() => LoadHighQualityTextures().Forget());
        loadSceneButton.onClick.AddListener(() => LoadSceneBundle().Forget());
        DebugTexturesLogButton.onClick.AddListener(() => DebugTextureDictionaries());
        // Find and load all asset bundles
        FindAndLoadAllBundles().Forget();
    }

    private async UniTask FindAndLoadAllBundles()
    {
        string directoryPath = Path.GetDirectoryName(materialBundleUrl);
        string[] bundleFiles = Directory.GetFiles(directoryPath, "*.bundle");

        foreach (var bundleFile in bundleFiles)
        {
            Debug.Log($"Found bundle file: {bundleFile}");
            await LoadBundle(bundleFile);
        }
    }

    public async UniTask LoadSceneBundle()
    {
        Debug.Log("Loading Scene Bundle: " + sceneBundleUrl);
        await LoadBundle(sceneBundleUrl);

        Debug.Log("Loading Scene: " + sceneName);
        await LoadSceneAdditiveFromBundle(sceneBundleUrl, sceneName);

        //Debug.Log("Loading Scene: " + sceneName);
        //var loadSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        //await loadSceneAsync;
        //Debug.Log("Scene Loaded Additively: " + sceneName);

        Debug.Log("Loading Object Bundle: " + objectBundleUrl);
        await LoadBundle(objectBundleUrl);

        Debug.Log("Loading Material Bundle: " + materialBundleUrl);
        await LoadBundle(materialBundleUrl);

        await LoadRenderersFromObjectBundle(objectBundleUrl);
        await LoadMaterialsFromBundle(materialBundleUrl);

        // Load textures for the loaded materials
        foreach (var materialName in loadedMaterials.Keys)
        {
            await LoadTexturesForMaterial(materialName);
        }

        // Debug the contents of the texture dictionaries
        DebugTextureDictionaries();
    }

    private async UniTask LoadBundle(string bundleUrl)
    {
        if (!File.Exists(bundleUrl))
        {
            Debug.LogWarning($"Bundle file not found: {bundleUrl}");
            return;
        }

        if (loadedBundles.ContainsKey(bundleUrl))
        {
            Debug.LogWarning($"Bundle already loaded: {bundleUrl}");
            return;
        }

        Debug.Log($"Attempting to load bundle: {bundleUrl}");
        var bundleRequest = AssetBundle.LoadFromFileAsync(bundleUrl);
        await bundleRequest;

        var bundle = bundleRequest.assetBundle;
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

    private async UniTask LoadTexturesForMaterial(string materialName)
    {
        string directoryPath = Path.GetDirectoryName(materialBundleUrl);
        string highQualityBundleUrl = Path.Combine(directoryPath, $"{materialName}.highquality");
        string lowQualityBundleUrl = Path.Combine(directoryPath, $"{materialName}.lowquality");

        if (File.Exists(highQualityBundleUrl))
        {
            await LoadBundle(highQualityBundleUrl);
            if (loadedBundles.ContainsKey(highQualityBundleUrl))
            {
                await LoadTexturesFromBundle(highQualityBundleUrl, highQualityTextures, "highquality");
                UnloadBundle(highQualityBundleUrl);
            }
        }
        else
        {
            Debug.LogWarning($"High quality texture bundle not found: {highQualityBundleUrl}");
        }

        if (File.Exists(lowQualityBundleUrl))
        {
            await LoadBundle(lowQualityBundleUrl);
            if (loadedBundles.ContainsKey(lowQualityBundleUrl))
            {
                await LoadTexturesFromBundle(lowQualityBundleUrl, lowQualityTextures, "lowquality");
                UnloadBundle(lowQualityBundleUrl);
            }
        }
        else
        {
            Debug.LogWarning($"Low quality texture bundle not found: {lowQualityBundleUrl}");
        }
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
