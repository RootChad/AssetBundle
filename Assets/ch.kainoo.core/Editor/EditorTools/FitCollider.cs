using UnityEngine;
using UnityEditor;
using System.Collections;

public class ColliderToFit : MonoBehaviour {

    [MenuItem("Tools/Utilities/Fit Box Collider to Children")]
    static void FitToChildren() {
        foreach (GameObject rootGameObject in Selection.gameObjects) {
            Collider collider = rootGameObject.GetComponent<Collider>();
            if (!(collider is BoxCollider))
                continue;

            Renderer[] renderersInChildren = rootGameObject.GetComponentsInChildren<Renderer>();
            Bounds bounds = renderersInChildren[0].bounds;
            foreach (Renderer childRenderer in renderersInChildren) {
                if (childRenderer != null) {
                    bounds.Encapsulate(childRenderer.bounds);
                }
            }

            BoxCollider boxCollider = collider as BoxCollider;
            boxCollider.center = bounds.center - rootGameObject.transform.position;
            boxCollider.size = bounds.size;
        }
    }

}