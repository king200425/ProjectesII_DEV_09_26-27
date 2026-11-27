using System.Collections;
using UnityEngine;

public class RoomCameraDirector : MonoBehaviour
{
    public static RoomCameraDirector Instance;

    public Camera targetCamera;
    public float blendTime = 0.25f;

    private Coroutine blend;

    void Awake()
    {
        Instance = this;
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
    }

    public void Focus(Transform point, float orthographicSize)
    {
        if (blend != null)
            StopCoroutine(blend);
        blend = StartCoroutine(BlendTo(point, orthographicSize));
    }

    IEnumerator BlendTo(Transform point, float size)
    {
        Vector3 start = transform.position;
        Vector3 end = new Vector3(point.position.x, point.position.y, -10f);
        float startSize = targetCamera.orthographicSize;

        float t = 0f;
        while (t < blendTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / blendTime);
            transform.position = Vector3.Lerp(start, end, k);
            targetCamera.orthographicSize = Mathf.Lerp(startSize, size, k);
            yield return null;
        }

        transform.position = end;
        targetCamera.orthographicSize = size;
    }
}
