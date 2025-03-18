using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UICamera : MonoBehaviour
{
    [SerializeField] RenderTexture targetTexture;
    Camera camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        camera = GetComponent<Camera>();
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Base;

        camera.targetTexture = targetTexture;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0, 0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
    }
}