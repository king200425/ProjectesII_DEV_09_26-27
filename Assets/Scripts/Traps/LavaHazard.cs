using UnityEngine;

public class LavaHazard : MonoBehaviour
{
    void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null)
            health.Die();
    }
}
