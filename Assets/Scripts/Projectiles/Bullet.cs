using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;

    private TimeSlow timeSlow;
    private float lifeTimer = 0f;

    void Start()
    {
        // Find the player's time slow component
        timeSlow = FindFirstObjectByType<TimeSlow>();
    }

    void Update()
    {
        float multiplier = 1f;

        // Slow down the bullet when Q is active
        if (timeSlow != null && timeSlow.IsTimeSlowed)
        {
            multiplier = timeSlow.slowMultiplier;
        }

        float delta = Time.deltaTime * multiplier;

        // Move the bullet
        transform.position += transform.right * speed * delta;

        // Count bullet lifetime using the same speed
        lifeTimer += delta;

        if (lifeTimer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
