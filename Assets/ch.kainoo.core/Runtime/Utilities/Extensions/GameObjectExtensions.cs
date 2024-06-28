using ch.kainoo.core.utilities.extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    public static void ChangeLayerRecursive(this GameObject self, string layerName)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex < 0)
        {
            Debug.LogError($"Layer '{layerName}' does not exist, could not set on object.", self);
            return;
        }

        ChangeLayerRecursive(self, layerIndex);
    }

    public static void ChangeLayerRecursive(this GameObject self, int layer)
    {
        var parent = self.transform;
        parent.gameObject.layer = layer;
        foreach (Transform child in parent)
        {
            ChangeLayerRecursive(child, layer);
        }
    }

    public static void ChangeLayerRecursive(this Transform self, string layerName)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex < 0)
        {
            Debug.LogError($"Layer '{layerName}' does not exist, could not set on object.", self.gameObject);
            return;
        }

        ChangeLayerRecursive(self, layerIndex);
    }

    public static void ChangeLayerRecursive(this Transform self, int layer)
    {
        self.gameObject.layer = layer;
        foreach (Transform child in self)
        {
            ChangeLayerRecursive(child, layer);
        }
    }

    public static void SetEnabledAll<T>(this T[] self, bool enabled) where T : MonoBehaviour
    {
        foreach (var c in self)
        {
            if (c == null) continue;
            c.enabled = enabled;
        }
    }

    public static void SetEnabledAll(this Collider[] self, bool enabled)
    {
        foreach (var c in self)
        {
            if (c == null) continue;
            c.enabled = enabled;
        }
    }

    public static void SetActiveAll(this GameObject[] input, bool isActive)
    {
        if (input == null)
        {
            return;
        }

        foreach (var g in input)
        {
            if (g == null) continue;
            g.SetActive(isActive);
        }
    }
    public static GameObject FindInChildren(this GameObject self, string name)
    {
        var res = self.transform.FindInChildren(name);
        if (res == null)
        {
            return null;
        }

        return res.gameObject;
    }

    public static Transform FindInChildren(this Transform self, string name)
    {
        foreach (Transform child in self)
        {
            if (child.name == name)
            {
                return child;
            }

            var childResult = child.FindInChildren(name);
            if (childResult != null)
            {
                return childResult;
            }
        }

        return null;
    }

}
