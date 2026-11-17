using UnityEngine;

public class CameraBackdrop : MonoBehaviour
{
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Fit();
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
    }

    void Fit()
    {
        Camera cam = Camera.main;
        if (cam == null || sr == null || sr.sprite == null) return;

        Vector2 size = sr.sprite.bounds.size;
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;
        float scale = Mathf.Max(viewHeight / size.y, viewWidth / size.x);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
