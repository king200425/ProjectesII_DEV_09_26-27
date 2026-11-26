using UnityEngine;

public class FlameTrap : MonoBehaviour
{
    [Header("Timing")]
    public float interval = 3f;
    public float activeTime = 1f;

    [Header("References")]
    public GameObject fireVisual;
    public BoxCollider2D damageCollider;
    public SpriteRenderer fireRenderer;
    public Sprite[] frames;
    public float framesPerSecond = 12f;

    [Header("Damage")]
    public bool killInstantly = true;

    private float timer;
    private float frameTimer;
    private int frameIndex;
    private bool active;

    public bool IsActive => active;

    void Start()
    {
        if (fireVisual != null)
            fireVisual.SetActive(false);

        if (damageCollider != null)
            damageCollider.enabled = false;

        timer = interval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (!active && timer <= 0f)
            StartBurst();

        if (!active)
            return;

        AnimateFire();

        if (timer <= 0f)
            EndBurst();
    }

    void StartBurst()
    {
        active = true;
        timer = activeTime;
        frameIndex = 0;
        frameTimer = 0f;

        if (fireVisual != null)
            fireVisual.SetActive(true);

        if (damageCollider != null)
            damageCollider.enabled = true;

        ApplyFrame();
    }

    void EndBurst()
    {
        active = false;
        timer = Mathf.Max(0.01f, interval - activeTime);

        if (fireVisual != null)
            fireVisual.SetActive(false);

        if (damageCollider != null)
            damageCollider.enabled = false;
    }

    void AnimateFire()
    {
        if (fireRenderer == null || frames == null || frames.Length == 0)
            return;

        frameTimer += Time.deltaTime * SlowFactor();
        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % frames.Length;
        }

        ApplyFrame();
    }

    void ApplyFrame()
    {
        if (fireRenderer != null && frames != null && frames.Length > 0)
            fireRenderer.sprite = frames[frameIndex];
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!active)
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return;

        if (killInstantly)
            health.Die();
        else
            health.TakeDamage();
    }

    static float SlowFactor()
    {
        return Mathf.Min(TimeSlow.WorldScale, TimeSlowPrototype.WorldScale);
    }
}
