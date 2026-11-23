using UnityEngine;

public class StyleBackdrop : MonoBehaviour
{
    public float zOffset = 10f;

    private SpriteRenderer sr;
    private Camera cam;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        Fit();
    }

    void LateUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, zOffset);
    }

    void Fit()
    {
        if (cam == null || sr == null || sr.sprite == null) return;
        float viewH = cam.orthographicSize * 2f;
        float viewW = viewH * cam.aspect;
        Vector2 size = sr.sprite.bounds.size;
        float scale = Mathf.Max(viewH / size.y, viewW / size.x);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
