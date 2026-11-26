using UnityEngine;

public class PortalAnimator : MonoBehaviour
{
    public SpriteRenderer portalRenderer;
    public Sprite[] frames;
    public float framesPerSecond = 10f;
    public bool loop = true;

    private int frameIndex;
    private float timer;

    void Start()
    {
        if (portalRenderer == null)
            portalRenderer = GetComponent<SpriteRenderer>();

        ApplyFrame();
    }

    void Update()
    {
        if (portalRenderer == null || frames == null || frames.Length == 0)
            return;

        timer += Time.deltaTime * SlowFactor();
        float duration = 1f / Mathf.Max(1f, framesPerSecond);

        while (timer >= duration)
        {
            timer -= duration;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (loop)
                    frameIndex = 0;
                else
                    frameIndex = frames.Length - 1;
            }

            ApplyFrame();
        }
    }

    void ApplyFrame()
    {
        if (portalRenderer != null && frames != null && frames.Length > 0)
            portalRenderer.sprite = frames[frameIndex];
    }

    static float SlowFactor()
    {
        return Mathf.Min(TimeSlow.WorldScale, TimeSlowPrototype.WorldScale);
    }
}
