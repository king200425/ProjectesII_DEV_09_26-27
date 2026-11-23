using UnityEngine;

public class PlayerShotSwitch : MonoBehaviour
{
    public PuzzleDoor door;
    public Color activeColor = new Color(0.3f, 1f, 0.85f);
    private SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void OnTriggerEnter2D(Collider2D other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet == null || bullet.isGhostBullet) return;
        if (door != null) door.Open();
        if (sr != null) sr.color = activeColor;
        Destroy(bullet.gameObject);
    }
}

