#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class FlexibleAssetBundleAssigner : MonoBehaviour
{
    private const string highQualityFolderPath = "Assets/Textures/HighQuality/";
    private const string lowQualityFolderPath = "Assets/Textures/LowQuality/";

    [MenuItem("Tools/Assign Selected Scenes to Asset Bundle")]
    public static void AssignSelectedScenesToBundle()
    {
        string sceneBundleName = "scenebundle"; // You can change this to your desired bundle name
        string objectBundleName = "objectbundle";
        string materialsBundleName = "materialsbundle";

        // Ensure the high and low quality folders exist
        EnsureFolderExists(highQualityFolderPath);
        EnsureFolderExists(lowQualityFolderPath);

        // Get selected scenes in the Project window
        Object[] selectedScenes = Selection.objects;
        if (selectedScenes.Length == 0)
        {
            Debug.LogWarning("No scenes selected. Please select one or more scenes in the Project window.");
            return;
        }
        foreach (Object scene in selectedScenes)
        {
            if (scene is SceneAsset)
            {
                string scenePath = AssetDatabase.GetAssetPath(scene);
                Debug.Log($"Assigning scene {scenePath} to bundle {sceneBundleName}");
                AssignSceneToBundle(scenePath, sceneBundleName);
                AssignPrefabsMaterialsAndTexturesInSceneToBundles(scenePath, objectBundleName, materialsBundleName);
            }
        }

        Debug.Log("Selected scenes, their prefabs, materials, and textures have been assigned to asset bundles.");
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
            Debug.Log($"Created folder at {folderPath}");
        }
    }

    private static void AssignSceneToBundle(string scenePath, string bundleName)
    {
        AssetImporter.GetAtPath(scenePath).SetAssetBundleNameAndVariant(bundleName, "");
    }

    private static void AssignPrefabsMaterialsAndTexturesInSceneToBundles(string scenePath, string objectBundleName, string materialsBundleName)
    {
        // Open the scene to find prefabs, materials, and textures
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        EditorSceneManager.OpenScene(scenePath);

        // Find all GameObjects in the scene
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Check if the object is a prefab instance
            if (PrefabUtility.IsPartOfAnyPrefab(obj))
            {
                GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefabAsset != null)
                {
                    string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
                    Debug.Log($"Prefab asset: {prefabAsset.name}, path: {prefabPath}");
                    if (!string.IsNullOrEmpty(prefabPath))
                    {
                        Debug.Log($"Assigning prefab {prefabPath} to bundle {objectBundleName}");
                        AssignPrefabToBundle(prefabPath, objectBundleName);
                    }
                    else
                    {
                        Debug.LogWarning($"Prefab path is empty for {obj.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Prefab asset is null for {obj.name}");
                }
            }
            else
            {
                Debug.LogWarning($"Object {obj.name} is not a prefab instance.");
            }

            // Assign materials to MaterialsBundle
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        string materialPath = AssetDatabase.GetAssetPath(material);
                        if (!string.IsNullOrEmpty(materialPath) && !materialPath.StartsWith("Resources/unity_builtin_extra"))
                        {
                            Debug.Log($"Assigning material {materialPath} to bundle {materialsBundleName}");
                            AssignMaterialToBundle(materialPath, materialsBundleName);
                            AssignTexturesToVariant(material);
                        }
                    }
                }
            }
        }
    }

    private static void AssignPrefabToBundle(string prefabPath, string bundleName)
    {
        if (!string.IsNullOrEmpty(prefabPath))
        {
            AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
            if (importer != null)
            {
                Debug.Log($"Assigning prefab at path {prefabPath} to bundle {bundleName}");
                importer.SetAssetBundleNameAndVariant(bundleName, "");
            }
            else
            {
                Debug.LogError($"AssetImporter is null for prefab at path {prefabPath}");
            }
        }
        else
        {
            Debug.LogError("Prefab path is null or empty.");
        }
    }

    private static void AssignMaterialToBundle(string materialPath, string bundleName)
    {
        if (!string.IsNullOrEmpty(materialPath))
        {
            AssetImporter importer = AssetImporter.GetAtPath(materialPath);
            if (importer != null)
            {
                Debug.Log($"Assigning material at path {materialPath} to bundle {bundleName}");
                importer.SetAssetBundleNameAndVariant(bundleName, "");
            }
            else
            {
                Debug.LogError($"AssetImporter is null for material at path {materialPath}");
            }
        }
        else
        {
            Debug.LogError("Material path is null or empty.");
        }
    }

    private static void AssignTexturesToVariant(Material material)
    {
        if (material != null)
        {
            foreach (string texturePropertyName in material.GetTexturePropertyNames())
            {
                Texture texture = material.GetTexture(texturePropertyName);
                if (texture != null)
                {
                    string texturePath = AssetDatabase.GetAssetPath(texture);
                    if (!string.IsNullOrEmpty(texturePath))
                    {
                        // Make the texture readable
                        MakeTextureReadable(texturePath);

                        // Move high quality texture to high quality folder
                        string highQualityPath = highQualityFolderPath + Path.GetFileName(texturePath);
                        AssetDatabase.MoveAsset(texturePath, highQualityPath);
                        Debug.Log($"Moved high quality texture to {highQualityPath}");

                        // Generate low quality texture
                        Texture2D highQualityTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(highQualityPath);
                        Texture2D lowQualityTexture = GenerateLowQualityTexture(highQualityTexture);
                        if (lowQualityTexture != null)
                        {
                            string lowQualityPath = lowQualityFolderPath + lowQualityTexture.name + ".png";
                            File.WriteAllBytes(lowQualityPath, lowQualityTexture.EncodeToPNG());
                            AssetDatabase.ImportAsset(lowQualityPath);
                            Debug.Log($"Generated low quality texture at {lowQualityPath}");

                            // Assign high and low quality textures to variants
                            string materialName = material.name.Replace(" ", "_").ToLower();
                            AssignTextureToBundle(highQualityPath, $"{materialName}", "HighQuality");
                           // AssignTextureToBundle(highQualityPath, $"{materialName}_highquality", "HighQuality");
                           // AssignTextureToBundle(lowQualityPath, $"{materialName}_lowquality", "LowQuality");
                            AssignTextureToBundle(lowQualityPath, $"{materialName}", "LowQuality");
                        }
                    }
                }
            }
        }
    }

    private static void MakeTextureReadable(string texturePath)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (textureImporter != null)
        {
            textureImporter.isReadable = true;
            textureImporter.SaveAndReimport();
            Debug.Log($"Made texture at path {texturePath} readable.");
        }
        else
        {
            Debug.LogError($"Failed to get TextureImporter for texture at path {texturePath}");
        }
    }

    private static Texture2D GenerateLowQualityTexture(Texture2D highQualityTexture)
    {
        if (highQualityTexture == null)
        {
            Debug.LogError("High quality texture is null.");
            return null;
        }

        // Adjust dimensions to be multiples of 4
        int width = (highQualityTexture.width / 2) / 4 * 4;
        int height = (highQualityTexture.height / 2) / 4 * 4;

        // Create a temporary RenderTexture
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        // Copy the high-quality texture to the RenderTexture
        Graphics.Blit(highQualityTexture, rt);

        // Create a new Texture2D with the correct format
        RenderTexture.active = rt;
        Texture2D lowQualityTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        lowQualityTexture.name = highQualityTexture.name ;
        lowQualityTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        lowQualityTexture.Apply();

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.active = null;

        return lowQualityTexture;
    }
    private static Texture2D GenerateHighQualityTexture(Texture2D sourceTexture)
    {
        if (sourceTexture == null)
        {
            Debug.LogError("Source texture is null.");
            return null;
        }

        // Set desired high quality dimensions
        int highQualityWidth = sourceTexture.width * 2;  // For example, doubling the resolution
        int highQualityHeight = sourceTexture.height * 2;

        // Create a new high-quality texture with the desired dimensions
        Texture2D highQualityTexture = new Texture2D(highQualityWidth, highQualityHeight, sourceTexture.format, false);

        // Copy the source texture to the high-quality texture using bilinear filtering
        for (int y = 0; y < highQualityHeight; y++)
        {
            for (int x = 0; x < highQualityWidth; x++)
            {
                // Calculate the corresponding pixel in the source texture
                float u = (float)x / (highQualityWidth - 1);
                float v = (float)y / (highQualityHeight - 1);
                Color color = sourceTexture.GetPixelBilinear(u, v);

                // Set the pixel color in the high-quality texture
                highQualityTexture.SetPixel(x, y, color);
            }
        }

        // Apply changes to the high-quality texture
        highQualityTexture.Apply();

        // Optionally, save the high-quality texture to a file
        string highQualityPath = highQualityFolderPath + sourceTexture.name + "_high.png";
        File.WriteAllBytes(highQualityPath, highQualityTexture.EncodeToPNG());
        AssetDatabase.ImportAsset(highQualityPath);
        Debug.Log($"Generated high quality texture at {highQualityPath}");

        return highQualityTexture;
    }
    private static void AssignTextureToBundle(string texturePath, string bundleName, string variant)
    {
        AssetImporter importer = AssetImporter.GetAtPath(texturePath);
        if (importer != null)
        {
            Debug.Log($"Assigning texture at path {texturePath} to bundle {bundleName} with variant {variant}");
            importer.SetAssetBundleNameAndVariant(bundleName, variant);
        }
        else
        {
            Debug.LogError($"AssetImporter is null for texture at path {texturePath}");
        }
    }
}
#endif