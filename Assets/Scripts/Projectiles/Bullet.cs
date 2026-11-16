using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 35f;
    public float lifetime = 3f;
    public int damage = 1;
    public float offScreenPadding = 0.08f;

    private Rigidbody2D rb;
    private bool hasHit;
    private int groundLayer;
    private Camera viewCamera;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.NameToLayer("Ground");

        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        viewCamera = Camera.main;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (IsOutsideView(transform.position))
        {
            Destroy(gameObject);
            return;
        }

        if (rb == null)
        {
            transform.position +=
                transform.right * speed * TimeSlowPrototype.WorldScale * Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (IsOutsideView(transform.position))
        {
            Destroy(gameObject);
            return;
        }

        Vector3 nextPosition = transform.position +
            transform.right * speed * TimeSlowPrototype.WorldScale * Time.fixedDeltaTime;

        if (IsOutsideView(nextPosition))
        {
            Destroy(gameObject);
            return;
        }

        float distance = speed * TimeSlowPrototype.WorldScale * Time.fixedDeltaTime + 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            transform.right,
            distance);

        if (hit.collider != null &&
            !hit.collider.isTrigger &&
            hit.collider.GetComponentInParent<PlayerHealth>() == null &&
            hit.collider.GetComponentInParent<EnemyHealth>() == null)
        {
            Destroy(gameObject);
            return;
        }

        if (rb != null)
            rb.linearVelocity = transform.right * speed * TimeSlowPrototype.WorldScale;
    }

    bool IsOutsideView(Vector3 position)
    {
        if (viewCamera == null)
            return false;

        Vector3 viewport = viewCamera.WorldToViewportPoint(position);

        return viewport.x < -offScreenPadding ||
               viewport.x > 1f + offScreenPadding ||
               viewport.y < -offScreenPadding ||
               viewport.y > 1f + offScreenPadding;
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




