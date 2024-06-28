#if UNITY_EDITOR
using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using UnityEditor.SceneManagement;
//using System.IO;

public class AssetBundleAssigner : MonoBehaviour
{
  /*  private const string highQualityFolderPath = "Assets/Textures/HighQuality/";
    private const string lowQualityFolderPath = "Assets/Textures/LowQuality/";

    [MenuItem("Tools/Assign Selected Scenes to Asset Bundle")]
    public static void AssignSelectedScenesToBundle()
    {
        string sceneBundleName = "scenebundle"; // You can change this to your desired bundle name
        string objectBundleName = "objectbundle";
        string materialsBundleName = "materialsbundle";

        // Create the high and low quality folders if they don't exist
        if (!Directory.Exists(highQualityFolderPath))
        {
            Directory.CreateDirectory(highQualityFolderPath);
        }
        if (!Directory.Exists(lowQualityFolderPath))
        {
            Directory.CreateDirectory(lowQualityFolderPath);
        }

        // Get selected scenes in the Project window
        Object[] selectedScenes = Selection.objects;

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
                            AssignTextureToBundle(highQualityPath, $"{materialName}_highquality", "HighQuality");
                            AssignTextureToBundle(lowQualityPath, $"{materialName}_lowquality", "LowQuality");
                        }
                    }
                }
            }
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

        // Create a new texture with reduced resolution
        Texture2D lowQualityTexture = new Texture2D(width, height, highQualityTexture.format, false);
        lowQualityTexture.name = highQualityTexture.name + "_LowQuality";
        lowQualityTexture.SetPixels(highQualityTexture.GetPixels(0, 0, width, height));
        lowQualityTexture.Apply();

        return lowQualityTexture;
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
    }*/
}
#endif