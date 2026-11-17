using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    public Transform respawnPoint;
    public float cooldown = 0.4f;

    private float nextTime;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < nextTime)
            return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        if (respawnPoint != null)
            player.transform.position = respawnPoint.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        nextTime = Time.time + cooldown;
    }
}
