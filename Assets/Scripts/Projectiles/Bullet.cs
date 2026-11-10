
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 35f;
    public float lifetime = 3f;
    public int damage = 1;

    private Rigidbody2D rb;
    private bool hasHit;
    private int groundLayer;

    private TimeSlow timeSlow;
    private float lifeRemaining;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    void Start()
    {
        timeSlow = FindFirstObjectByType<TimeSlow>();
        lifeRemaining = lifetime;
    }

    void Update()
    {
        float multiplier = 1f;

        if (timeSlow != null && timeSlow.IsTimeSlowed)
        {
            multiplier = timeSlow.slowMultiplier;
        }

        // Slow down bullet lifetime too
        lifeRemaining -= Time.deltaTime * multiplier;

        if (lifeRemaining <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // Movement without Rigidbody2D
        if (rb == null)
        {
            transform.position +=
                transform.right * speed * multiplier * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        float multiplier = 1f;

        if (timeSlow != null && timeSlow.IsTimeSlowed)
        {
            multiplier = timeSlow.slowMultiplier;
        }

        // Movement with Rigidbody2D
        if (rb != null)
        {
            rb.linearVelocity =
                transform.right * speed * multiplier;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            hasHit = true;
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == groundLayer)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
