using UnityEngine;

public class ShootLock : MonoBehaviour
{
    public Sprite activeSprite;
    public Color activeColor = Color.white;

    public bool IsActive { get; private set; }

    private SpriteRenderer visual;
    private Color startColor;
    private Sprite startSprite;

    void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        if (visual != null)
        {
            startColor = visual.color;
            startSprite = visual.sprite;
        }
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;

        if (visual != null)
        {
            if (activeSprite != null)
                visual.sprite = activeSprite;

            visual.color = activeColor;
        }
    }

    public void ResetLock()
    {
        IsActive = false;

        if (visual != null)
        {
            visual.sprite = startSprite;
            visual.color = startColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Bullet>() == null)
            return;

        Activate();
        Destroy(other.gameObject);
    }
}
