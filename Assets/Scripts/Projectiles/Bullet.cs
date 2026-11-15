using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 35f;
    public float lifetime = 3f;
    public int damage = 1;

    [Header("Bullet Type")]
    public bool isGhostBullet = false;

    [Header("Laser Visual")]
    [SerializeField] private SpriteRenderer laserVisual;

    [Header("Sparks")]
    [SerializeField] private Material sparkMaterial;
    [SerializeField] private Color sparkColor = new(1f, 0.31f, 0.09f);
    [SerializeField] private Color sparkAccent = new(1f, 0.76f, 0.30f);

    private static readonly int LaserLaunch = Shader.PropertyToID("_LaserLaunch");
    private static MaterialPropertyBlock laserProperties;

    private Rigidbody2D rb;
    private Collider2D hitCollider;
    private bool hasHit;
    private int groundLayer;

    private TimeSlow timeSlow;
    private float lifeRemaining;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<Collider2D>();
        groundLayer = LayerMask.NameToLayer("Ground");

        if (laserVisual != null)
        {
            Vector3 muzzle = transform.position;
            Vector3 forward = transform.right;
            laserProperties ??= new MaterialPropertyBlock();
            laserVisual.GetPropertyBlock(laserProperties);
            laserProperties.SetVector(LaserLaunch, new Vector4(muzzle.x, muzzle.y, forward.x, forward.y));
            laserVisual.SetPropertyBlock(laserProperties);
        }

        // Both the player and the replay spawn this bullet at their gun's muzzle.
        if (sparkMaterial != null)
            ProjectileSparks.PlayMuzzle(transform.position, transform.right, sparkMaterial, sparkColor, sparkAccent);
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

        // Hit laser emitter
        LaserEmitter emitter =
            other.GetComponentInParent<LaserEmitter>();

        if (emitter != null)
        {
            FinishImpact(other);
            emitter.Hit(isGhostBullet);
            return;
        }

        // Hit enemy
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            FinishImpact(other);
            enemy.TakeDamage(damage);
            return;
        }

        // Hit ground
        if (other.gameObject.layer == groundLayer)
            FinishImpact(other);
    }

    private void FinishImpact(Collider2D target)
    {
        hasHit = true;
        if (sparkMaterial != null)
        {
            Vector2 travel = transform.right;
            Vector2 point = target.ClosestPoint(transform.position);
            Vector2 normal = -travel;
            if (hitCollider != null)
            {
                ColliderDistance2D distance = hitCollider.Distance(target);
                if (distance.isValid)
                {
                    point = distance.pointB;
                    normal = distance.normal;
                    if (Vector2.Dot(normal, travel) > 0f)
                        normal = -normal;
                }
            }

            ProjectileSparks.PlayImpact(new Vector3(point.x, point.y, transform.position.z),
                                        normal, sparkMaterial, sparkColor, sparkAccent);
        }
        Destroy(gameObject);
    }
}
