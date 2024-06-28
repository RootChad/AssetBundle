using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RenderingTools
{
    public static Bounds GetTotalBounds(GameObject go)
    {
        if (go == null)
        {
            throw new ArgumentNullException(nameof(go));
        }

        var boundsAccu = new BoundsAccumulator();
        var renderers = go.GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            var b = r.bounds;
            boundsAccu.Accumulate(b);
        }

        return boundsAccu.GetBounds();
    }

    public static Bounds GetTotalBounds(params GameObject[] gameObjects)
    {
        if (gameObjects.Length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(gameObjects));
        }

        var boundsAccu = new BoundsAccumulator();
        foreach (var go in gameObjects)
        {
            try
            {
                var newBounds = GetTotalBounds(go);
                boundsAccu.Accumulate(newBounds);
            } catch (BoundsUninitializedException) { }
        }

        return boundsAccu.GetBounds();
    }

    public static Bounds GetSceneBounds(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            throw new ArgumentException("Scene invalid");
        }

        var rootGameobjects = scene.GetRootGameObjects();
        return GetTotalBounds(rootGameobjects);
    }

    public static Bounds GetActiveSceneBounds()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        return GetSceneBounds(sceneName);
    }

    private class BoundsAccumulator
    {
        private Bounds bounds = new Bounds();
        private bool initialized = false;

        public BoundsAccumulator()
        {
        }

        public BoundsAccumulator(Bounds b)
        {
            bounds = b;
            initialized = true;
        }

        public Bounds Accumulate(Bounds b)
        {
            if (initialized)
            {
                bounds.Encapsulate(b);
                return bounds;
            }

            bounds = b;
            initialized = true;
            return bounds;
        }

        public Bounds GetBounds()
        {
            if (!initialized)
            {
                throw new BoundsUninitializedException();
            }

            return bounds;
        }

    }

}

public class BoundsUninitializedException : Exception
{
    public BoundsUninitializedException() : base() { }
    public BoundsUninitializedException(string message) : base(message) { }
}
