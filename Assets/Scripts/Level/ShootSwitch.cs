using UnityEngine;

public class ShootSwitch : MonoBehaviour
{
    public Color activeColor = Color.green;
    public BridgeDoor bridge;

    public bool IsActive { get; private set; }

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
        if (IsActive)
            return;

        IsActive = true;

        if (spriteRenderer != null)
            spriteRenderer.color = activeColor;

        if (bridge != null)
            bridge.CheckSwitches();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() == null)
            return;

        Activate();
        Destroy(other.gameObject);
    }
}
