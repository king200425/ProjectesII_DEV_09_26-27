using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSync : MonoBehaviour
{
    public Camera mainCamera;

    private Camera colorCamera;

    void Awake()
    {
        colorCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;

        colorCamera.orthographic = mainCamera.orthographic;
        colorCamera.orthographicSize = mainCamera.orthographicSize;
        colorCamera.aspect = mainCamera.aspect;
    }
}
