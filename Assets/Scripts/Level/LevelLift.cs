using UnityEngine;

public class LevelLift : MonoBehaviour
{
    public Transform target;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null || target == null)
            return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        player.transform.position = target.position;
    }
}
