using UnityEngine;

public class EchoShotSwitch : MonoBehaviour
{
    public PuzzleDoor door;
    public Color activeColor = new Color(0.9f, 0.55f, 1f);
    private SpriteRenderer sr;
    private bool armed;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet == null) return;

        if (bullet.isGhostBullet)
        {
            ActivateDoor();
            Destroy(bullet.gameObject);
        }
    }

    void ActivateDoor()
    {
        if (door != null) door.Open();
        if (sr != null) sr.color = activeColor;
    }
}

