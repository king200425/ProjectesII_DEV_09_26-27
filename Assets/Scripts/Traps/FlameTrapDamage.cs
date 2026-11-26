using UnityEngine;

public class FlameTrapDamage : MonoBehaviour
{
    public FlameTrap trap;

    void OnTriggerStay2D(Collider2D other)
    {
        if (trap == null || !trap.IsActive)
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.Die();
    }
}
