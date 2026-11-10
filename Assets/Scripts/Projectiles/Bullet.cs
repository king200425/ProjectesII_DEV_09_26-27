using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 35f;

    public float lifetime = 3f;

    public int damage = 1;

    private Rigidbody2D rb;
    private bool hasHit;
    private int groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rb == null)
        {
            transform.position +=
                transform.right * speed * TimeSlowPrototype.WorldScale * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
            rb.linearVelocity = transform.right * speed * TimeSlowPrototype.WorldScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

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
