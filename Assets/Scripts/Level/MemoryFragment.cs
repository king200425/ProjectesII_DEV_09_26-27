using UnityEngine;

public class MemoryFragment : MonoBehaviour
{
    public static int Collected;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        Collected++;
        Destroy(gameObject);
    }
}
