using UnityEngine;

public class LaserCorePulse : MonoBehaviour
{
    public float pulseSpeed = 3f;
    public float pulseAmount = 0.06f;

    private Vector3 baseScale;
    private float visualTime;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        visualTime += Time.deltaTime * TimeSlow.WorldScale;
        float scale = 1f + Mathf.Sin(visualTime * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * scale;
    }
}
