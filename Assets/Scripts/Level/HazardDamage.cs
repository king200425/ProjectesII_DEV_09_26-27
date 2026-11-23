using UnityEngine;

public class HazardDamage : MonoBehaviour
{
    public int damage = 1;
    public float cooldown = 0.6f;
    private float nextHit;

    void OnTriggerEnter2D(Collider2D other) { Hit(other); }
    void OnTriggerStay2D(Collider2D other) { Hit(other); }

    void Hit(Collider2D other)
    {
        if (Time.time < nextHit) return;
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;
        player.TakeDamage();
        nextHit = Time.time + cooldown;
    }
}
