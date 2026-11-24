using UnityEngine;

public class LaserCorePulse : MonoBehaviour
{
    public float pulseSpeed = 3f;
    public float pulseAmount = 0.06f;

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * scale;
    }
}
