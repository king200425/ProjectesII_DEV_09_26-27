using UnityEngine;

public class LaserReceiver : MonoBehaviour
{
    public DualLaserGate gate;
    public bool echoOnly;
    public Color activeColor = Color.white;
    private SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void OnTriggerEnter2D(Collider2D other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet == null || bullet.isGhostBullet != echoOnly) return;
        if (gate != null) gate.Register(this);
        if (sr != null) sr.color = activeColor;
        Destroy(bullet.gameObject);
    }
}

