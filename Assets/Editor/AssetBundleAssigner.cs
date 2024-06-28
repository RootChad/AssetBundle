using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AssetBundleAssigner : EditorWindow
{
    [MenuItem("Tools/Assign Objects to AssetBundles")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleAssigner>("Assign AssetBundles");
    }

    private string assetBundleName = "myAssetBundle";

    void OnGUI()
    {
        GUILayout.Label("Assign All Objects in Scene to AssetBundle", EditorStyles.boldLabel);
        assetBundleName = EditorGUILayout.TextField("AssetBundle Name", assetBundleName);

        if (GUILayout.Button("Assign"))
        {
            AssignAllObjectsToAssetBundle();
        }
    }

    void AssignAllObjectsToAssetBundle()
    {
        // Récupérer tous les objets actifs dans la scène
        List<GameObject> allObjects = new List<GameObject>();
        foreach (GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            allObjects.Add(rootObj);
            foreach (Transform child in rootObj.GetComponentsInChildren<Transform>(true))
            {
                allObjects.Add(child.gameObject);
            }
        }

        foreach (GameObject obj in allObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(assetBundleName, "");
            }
        }

        Debug.Log("Assigned all objects in the scene to AssetBundle: " + assetBundleName);
    }
}
