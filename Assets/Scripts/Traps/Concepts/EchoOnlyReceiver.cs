using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class EchoOnlyReceiver : MonoBehaviour
{
    public SpriteRenderer visual;
    public Color inactiveColor = new Color(0.35f, 0.65f, 0.9f, 1f);
    public Color activeColor = new Color(0.4f, 1f, 0.85f, 1f);
    public UnityEvent onActivated;

    public bool IsActive { get; private set; }

    void Awake()
    {
        if (visual == null)
            visual = GetComponent<SpriteRenderer>();

        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        if (visual != null)
            visual.color = inactiveColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Bullet bullet = other.GetComponentInParent<Bullet>();
        if (bullet == null || !bullet.isGhostBullet)
            return;

        Activate();
        Destroy(bullet.gameObject);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;

        if (visual != null)
            visual.color = activeColor;

        onActivated?.Invoke();
    }

    public void ResetReceiver()
    {
        IsActive = false;

        if (visual != null)
            visual.color = inactiveColor;
    }
}
