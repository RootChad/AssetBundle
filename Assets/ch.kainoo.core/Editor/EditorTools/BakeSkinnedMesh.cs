using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class BakeSkinnedMesh : MonoBehaviour {

    [MenuItem("Tools/Utilities/Bake Skinned Mesh")]
    static void Bake() {
        foreach (GameObject rootGameObject in Selection.gameObjects) {
            var meshRenderer = rootGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            var originalMesh = meshRenderer.sharedMesh;
            var originalMeshPath = AssetDatabase.GetAssetPath(originalMesh);
            var originalMeshDirectory = Path.GetDirectoryName(originalMeshPath);

            Mesh newMesh = new Mesh();
            newMesh.name = originalMesh.name + "_baked";
            meshRenderer.BakeMesh(newMesh);

            AssetDatabase.CreateAsset(newMesh, Path.Combine(originalMeshDirectory, newMesh.name + ".asset"));
            AssetDatabase.SaveAssets();
        }
    }

}