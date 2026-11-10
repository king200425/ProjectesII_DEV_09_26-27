using UnityEngine;

public class TimeSwitch : MonoBehaviour
{
    public float activeWindow = 3f;
    public Color activeColor = Color.cyan;

    public bool IsActive { get; private set; }

    private float timer;
    private SpriteRenderer spriteRenderer;
    private Color startColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            startColor = spriteRenderer.color;
    }

    public void Activate()
    {
        timer = activeWindow;
        IsActive = true;

        if (spriteRenderer != null)
            spriteRenderer.color = activeColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() == null)
            return;

        Activate();
        Destroy(other.gameObject);
    }

    void Update()
    {
        if (!IsActive)
            return;

        timer -= Time.deltaTime * TimeSlowPrototype.WorldScale;

        if (timer > 0f)
            return;

        IsActive = false;

        if (spriteRenderer != null)
            spriteRenderer.color = startColor;
    }
}
