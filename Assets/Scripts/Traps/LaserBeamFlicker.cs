using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LaserBeamFlicker : MonoBehaviour
{
    [Header("Pulse")]
    public float pulseSpeed = 16f;
    public float minAlpha = 0.72f;
    public float maxAlpha = 1f;
    public float widthPulse = 0.07f;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private float phase;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float wave = (Mathf.Sin(Time.time * pulseSpeed + phase) + 1f) * 0.5f;

        Color c = sr.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, wave);
        sr.color = c;

        transform.localScale = new Vector3(
            baseScale.x,
            baseScale.y * (1f + widthPulse * Mathf.Sin(Time.time * pulseSpeed * 0.7f + phase)),
            baseScale.z);
    }
}
