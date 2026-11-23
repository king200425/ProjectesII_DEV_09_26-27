using UnityEngine;

public class SpikeRowMotion : MonoBehaviour
{
    public Transform[] spikes;
    public Collider2D hazard;
    public float amplitude = 0.6f;
    public float speed = 2.2f;
    public float baseY = 0f;

    private float[] startY;
    private float phase;

    void Awake()
    {
        startY = new float[spikes.Length];
        for (int i = 0; i < spikes.Length; i++)
            if (spikes[i] != null) startY[i] = spikes[i].localPosition.y;
    }

    void Update()
    {
        phase += Time.deltaTime * TimeSlowPrototype.WorldScale * speed;
        float rise = (Mathf.Sin(phase) + 1f) * 0.5f;
        for (int i = 0; i < spikes.Length; i++)
        {
            if (spikes[i] == null) continue;
            Vector3 p = spikes[i].localPosition;
            p.y = startY[i] - amplitude + rise * amplitude * 2f;
            spikes[i].localPosition = p;
        }

        if (hazard != null)
            hazard.enabled = rise > 0.72f;
    }
}
