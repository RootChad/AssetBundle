#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class FlexibleAssetBundleAssigner : MonoBehaviour
{
    private static Dictionary<Texture, (string, string)> bundledTextures = new Dictionary<Texture, (string, string)>();
    private static List<Material> bundledMaterials = new List<Material>();
    private const string lowQualityFolderPath = "Assets/Textures/LowQuality/";

    [MenuItem("Kainoo/Tools/Assign Selected Scenes to Asset Bundle")]
    public static void AssignSelectedScenesToBundle()
    {
        string sceneBundleName = "scenebundle"; // You can change this to your desired bundle name
        string objectBundleName = "objectbundle";
        string materialsBundleName = "materialsbundle";

        bundledTextures.Clear();
        bundledMaterials.Clear();

        // Ensure the low quality folder exists
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
                        bool duplicateMaterialName = false;
                        foreach(var mat in bundledMaterials)
                        {
                            if (mat.name.Equals(material.name))
                            {
                                duplicateMaterialName = true;
                            }
                        }
                        if (!bundledMaterials.Contains(material) && duplicateMaterialName)
                        {
                            AssetDatabase.RenameAsset(materialPath, material.name + "_01");
                            AssetDatabase.Refresh();
                            materialPath = AssetDatabase.GetAssetPath(material);
                        }
                        if (!string.IsNullOrEmpty(materialPath) && !materialPath.StartsWith("Resources/unity_builtin_extra"))
                        {
                            Debug.Log($"Assigning material {materialPath} to bundle {materialsBundleName}");
                            AssignMaterialToBundle(materialPath, materialsBundleName);
                            AssignTexturesToVariant(material);
                            bundledMaterials.Add(material);
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
                    if (bundledTextures.ContainsKey(texture))
                    {
                        string materialName = material.name.Replace(" ", "_").ToLower();
                        Texture highQualityTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(bundledTextures[texture].Item1);
                        material.SetTexture(texturePropertyName, highQualityTexture);
                    }
                    else
                    {
                        string texturePath = AssetDatabase.GetAssetPath(texture);
                        if (!string.IsNullOrEmpty(texturePath))
                        {
                            // Ensure the texture is readable
                            MakeTextureReadable(texturePath);

                            string materialDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(material));
                            //string texturesFolderPath = Path.Combine(materialDirectory, "Textures");
                            //if (!Directory.Exists(texturesFolderPath))
                            //{
                            //    Directory.CreateDirectory(texturesFolderPath);
                            //    AssetDatabase.Refresh();
                            //}
                            //string highQualityFolderPath = Path.Combine(texturesFolderPath, "High_Quality");
                            //if (!Directory.Exists(highQualityFolderPath))
                            //{
                            //    Directory.CreateDirectory(highQualityFolderPath);
                            //    AssetDatabase.Refresh();
                            //}

                            Texture2D highQualityTexture = null;
                            string lowQualityPath = null;

                            // Check if the image is PNG or needs conversion
                            if (Path.GetExtension(texturePath).ToLower() != ".png")
                            {
                                // Generate high quality texture
                                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                                highQualityTexture = GenerateHighQualityTexture(texture2D);
                                if (highQualityTexture != null)
                                {
                                    texturePath = Path.ChangeExtension(texturePath, "png");
                                    File.WriteAllBytes(texturePath, highQualityTexture.EncodeToPNG());
                                    AssetDatabase.ImportAsset(texturePath);
                                    Debug.Log($"Generated high quality texture at {texturePath}");
                                }
                            }

                            // Assign high and high quality textures to variants
                            highQualityTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                            string materialName = material.name.Replace(" ", "_").ToLower();
                            AssignTextureToBundle(texturePath, $"{materialName}", "HighQuality");
                            material.SetTexture(texturePropertyName, highQualityTexture);

                            // Generate low quality texture
                            Texture2D lowQualityTexture = GenerateLowQualityTexture(highQualityTexture);
                            if (lowQualityTexture != null)
                            {
                                lowQualityPath = Path.Combine(lowQualityFolderPath, lowQualityTexture.name + ".png");
                                File.WriteAllBytes(lowQualityPath, lowQualityTexture.EncodeToPNG());
                                AssetDatabase.ImportAsset(lowQualityPath);
                                Debug.Log($"Generated low quality texture at {lowQualityPath}");

                                // Assign high and low quality textures to variants
                                materialName = material.name.Replace(" ", "_").ToLower();
                                AssignTextureToBundle(lowQualityPath, $"{materialName}", "LowQuality");
                            }

                            bundledTextures.Add(texture, (texturePath, lowQualityPath));
                        }
                    }
                }
            }
        }
    }

    private static string ConvertToPng(Texture2D texture, string texturePath)
    {
        byte[] textureBytes = File.ReadAllBytes(texturePath);
        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        newTexture.LoadImage(textureBytes);

        string pngPath = Path.ChangeExtension(texturePath, ".png");
        byte[] pngBytes = newTexture.EncodeToPNG();
        File.WriteAllBytes(pngPath, pngBytes);
        AssetDatabase.ImportAsset(pngPath);

        return pngPath;
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

    private static Texture2D GenerateHighQualityTexture(Texture2D highQualityTexture)
    {
        if (highQualityTexture == null)
        {
            Debug.LogError("High quality texture is null.");
            return null;
        }

        // Adjust dimensions to be multiples of 4
        int width = highQualityTexture.width;
        int height = highQualityTexture.height;

        // Create a temporary RenderTexture
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        // Copy the high-quality texture to the RenderTexture
        Graphics.Blit(highQualityTexture, rt);

        // Create a new Texture2D with the correct format
        RenderTexture.active = rt;
        Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        newTexture.name = highQualityTexture.name;
        newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        newTexture.Apply();

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.active = null;

        return newTexture;
    }

    private static Texture2D GenerateLowQualityTexture(Texture2D highQualityTexture)
    {
        if (highQualityTexture == null)
        {
            Debug.LogError("High quality texture is null.");
            return null;
        }

        // Adjust dimensions to be multiples of 4
        int width = (highQualityTexture.width / 6) / 4 * 4;
        int height = (highQualityTexture.height / 6) / 4 * 4;

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

    private static void AssignTextureToBundle(string texturePath, string bundleName, string variant)
    {
        AssetImporter importer = AssetImporter.GetAtPath(texturePath);
        if (importer != null)
        {
            Debug.Log($"Assigning texture at path {texturePath} to bundle {bundleName} with variant {variant}");
            if (!string.IsNullOrEmpty(importer.assetBundleName) && !string.IsNullOrEmpty(importer.assetBundleVariant))
            {
                Debug.Log($"Texture is already assigned to a bundle with variant");
                return;
            }
            importer.SetAssetBundleNameAndVariant(bundleName, variant);
        }
        else
        {
            Debug.LogError($"AssetImporter is null for texture at path {texturePath}");
        }
    }
}
#endif