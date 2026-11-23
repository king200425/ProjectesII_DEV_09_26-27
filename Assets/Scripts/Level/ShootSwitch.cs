using UnityEngine;

public class ShootSwitch : MonoBehaviour
{
    public Color activeColor = Color.green;
    public Sprite offSprite;
    public Sprite onSprite;
    public BridgeDoor bridge;
    public PuzzleDoor puzzleDoor;

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
        {
            if (onSprite != null)
            {
                spriteRenderer.sprite = onSprite;
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = activeColor;
            }
        }

        if (bridge != null)
            bridge.CheckSwitches();

        if (puzzleDoor != null)
            puzzleDoor.Open();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() == null)
            return;

        Activate();
        Destroy(other.gameObject);
    }
}
