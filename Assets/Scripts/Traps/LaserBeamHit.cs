using UnityEngine;

public class LaserBeamHit : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Hit(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Hit(other);
    }

    void Hit(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.Die();
    }
}
