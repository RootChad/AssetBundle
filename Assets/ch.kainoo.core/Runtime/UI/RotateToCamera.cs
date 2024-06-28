using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RotateToCamera : MonoBehaviour
{
    [SerializeField] private Vector3 m_rotationOffset;

    private void Awake()
    {
        RenderPipelineManager.beginCameraRendering += RotateCameraOnBeforeRender;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= RotateCameraOnBeforeRender;
    }

    private void RotateCameraOnBeforeRender(ScriptableRenderContext ctx, Camera camera)
    {
        //#if UNITY_EDITOR
        //        // The scene view is a camera for unity, so we want to ignore it
        //        if (camera == UnityEditor.SceneView.currentDrawingSceneView)
        //            return;
        //#endif

        var cameraForward = camera.transform.rotation;

        var rotationValue = cameraForward;
        rotationValue = Quaternion.Euler(m_rotationOffset) * rotationValue;
        
        transform.rotation = rotationValue;
    }
}
