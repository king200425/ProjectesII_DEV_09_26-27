
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Bullet movement speed
    public float speed = 15f;

    // Bullet lifetime in seconds
    public float lifetime = 3f;

    void Start()
    {
        // Destroy the bullet after its lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move forward in the bullet's local right direction
        transform.position += transform.right * speed * Time.deltaTime;
    }
}
