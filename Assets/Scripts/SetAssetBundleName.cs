#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SetAssetBundleName : MonoBehaviour
{
    [MenuItem("Kainoo/Asset Bundles/Set AssetBundle Names for Scene")]
    static void SetAssetBundleNamesForScene()
    {
        // Get the active scene
        string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        if (!string.IsNullOrEmpty(scenePath))
        {
            // Set AssetBundle name for the scene
            AssetImporter sceneImporter = AssetImporter.GetAtPath(scenePath);
            if (sceneImporter != null)
            {
                sceneImporter.assetBundleName = "scenebundle";
                sceneImporter.SaveAndReimport();
                Debug.Log($"AssetBundle name set to 'scenebundle' for scene at path: {scenePath}");
            }
            else
            {
                Debug.LogError("Could not get AssetImporter for the scene.");
            }

            // Find all prefabs in the scene
            GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in allObjects)
            {
                print("Prefab " + obj.name);
                AddPrefabsInHierarchy(obj);
            }
        }
        else
        {
            Debug.LogError("No active scene found.");
        }
    }

    static void AddPrefabsInHierarchy(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            AddPrefabsInHierarchy(child.gameObject);
        }
        string assetPath = AssetDatabase.GetAssetPath(obj);
        print("AssetPath " + assetPath);
        if (!string.IsNullOrEmpty(assetPath) && PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab)
        {
            print("IsPrefab true");
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
            if (assetImporter != null)
            {
                assetImporter.assetBundleName = "prefabbundle";
                assetImporter.SaveAndReimport();
                Debug.Log($"AssetBundle name set to 'prefabbundle' for prefab at path: {assetPath}");
            }
        }
    }

    [MenuItem("Kainoo/Asset Bundles/Set Material Textures")]
    static void SetMaterialTextures()
    {
        // Get the selected material
        Object selectedObject = Selection.activeObject;

        if (selectedObject != null && selectedObject is Material)
        {
            Material material = (Material)selectedObject;
            string materialPath = AssetDatabase.GetAssetPath(material);

            Texture2D lowResTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/low_res_texture.png");
            Texture2D highResTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/high_res_texture.png");

            if (lowResTexture != null && highResTexture != null)
            {
                material.SetTexture("_MainTex", lowResTexture);
                material.SetTexture("_DetailAlbedoMap", highResTexture);
                AssetDatabase.SaveAssets();

                Debug.Log($"Low and high resolution textures set for material at path: {materialPath}");
            }
            else
            {
                Debug.LogError("Could not find low or high resolution textures.");
            }
        }
        else
        {
            Debug.LogError("No material selected or selected object is not a material.");
        }
    }
   
}
#endif