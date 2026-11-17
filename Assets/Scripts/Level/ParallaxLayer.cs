using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float factor = 0.5f;
    public float baseX = 200f;
    private Transform cam;

    void Start()
    {
        if (Camera.main != null)
            cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null)
            return;

        transform.position = new Vector3(
            baseX + cam.position.x * (1f - factor),
            cam.position.y,
            transform.position.z);
    }
}

