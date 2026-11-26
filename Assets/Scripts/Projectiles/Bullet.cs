using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 35f;
    public float lifetime = 3f;
    public int damage = 1;
    [SerializeField, Min(0f)] private float offScreenPadding = 0.08f;

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
    private Camera levelCamera;
    private readonly RaycastHit2D[] solidHits = new RaycastHit2D[8];
    private float lifeRemaining;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitCollider = GetComponent<Collider2D>();
        groundLayer = LayerMask.NameToLayer("Ground");

        if (rb != null)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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
        if (TimeSlowPrototype.IsPresent)
            levelCamera = Camera.main;
        lifeRemaining = lifetime;
    }

    private float SlowMultiplier => timeSlow != null && timeSlow.IsTimeSlowed
        ? timeSlow.slowMultiplier
        : 1f;

    void Update()
    {
        if (hasHit || Time.timeScale == 0f)
            return;

        // Slow down bullet lifetime too
        float multiplier = SlowMultiplier;
        lifeRemaining -= Time.deltaTime * multiplier;

        if (lifeRemaining <= 0f || IsOutsideLevelView())
        {
            Destroy(gameObject);
            return;
        }

        // Movement without Rigidbody2D
        if (rb == null)
        {
            float step = speed * multiplier * TimeSlowPrototype.WorldScale * Time.deltaTime;
            if (TryHitTimeSwitchAlongPath(step))
                return;

            transform.position +=
                transform.right * step;
        }
    }

    void FixedUpdate()
    {
        if (rb == null || hasHit)
            return;

        Vector2 direction = transform.right;
        float currentSpeed = speed * SlowMultiplier * TimeSlowPrototype.WorldScale;

        if (TryHitTimeSwitchAlongPath(currentSpeed * Time.fixedDeltaTime))
            return;

        // Sweep fast shots in every level so a thin moving switch head cannot be skipped.
        if (hitCollider != null)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            int count = hitCollider.Cast(direction, filter, solidHits, currentSpeed * Time.fixedDeltaTime);
            for (int i = 0; i < count; i++)
            {
                RaycastHit2D hit = solidHits[i];
                if (hit.collider.GetComponentInParent<PlayerHealth>() != null)
                    continue;

                if (TryImpact(hit.collider, hit))
                    return;
            }
        }

        // Movement with Rigidbody2D
        rb.linearVelocity = direction * currentSpeed;
    }

    private bool TryHitTimeSwitchAlongPath(float distance)
    {
        if (distance <= 0f)
            return false;

        Vector2 next = (Vector2)transform.position + (Vector2)transform.right * distance;
        Collider2D[] hits = Physics2D.OverlapCircleAll(next, 0.35f);

        foreach (Collider2D hit in hits)
        {
            TimeSwitch timeSwitch = hit.GetComponentInParent<TimeSwitch>();

            if (timeSwitch == null)
                continue;

            timeSwitch.Activate();
            FinishImpact(hit, default);
            return true;
        }

        return false;
    }

    private bool IsOutsideLevelView()
    {
        if (levelCamera == null)
            return false;

        Vector3 position = levelCamera.WorldToViewportPoint(transform.position);
        return position.x < -offScreenPadding || position.x > 1f + offScreenPadding ||
               position.y < -offScreenPadding || position.y > 1f + offScreenPadding;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasHit)
            TryImpact(other);
    }

    private bool TryImpact(Collider2D other, RaycastHit2D hit = default)
    {
        // Hit laser emitter
        LaserEmitter emitter =
            other.GetComponentInParent<LaserEmitter>();

        if (emitter != null)
        {
            FinishImpact(other, hit);
            emitter.Hit(isGhostBullet);
            return true;
        }

        // Hit rotating/turret trap
        TurretHealth turret =
            other.GetComponentInParent<TurretHealth>();

        if (turret != null)
        {
            FinishImpact(other, hit);
            turret.Hit(isGhostBullet);
            return true;
        }

        // Hit enemy
        EnemyHealth enemy =
            other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            FinishImpact(other, hit);
            enemy.TakeDamage(damage);
            return true;
        }

        ShotPressureSwitch pressureSwitch = other.GetComponentInParent<ShotPressureSwitch>();
        if (pressureSwitch != null)
        {
            FinishImpact(other, hit);
            pressureSwitch.ReceiveShot(isGhostBullet);
            return true;
        }

        ShootSwitch shootSwitch = other.GetComponentInParent<ShootSwitch>();
        if (shootSwitch != null)
        {
            FinishImpact(other, hit);
            shootSwitch.Activate();
            return true;
        }

        TimeSwitch timeSwitch = other.GetComponentInParent<TimeSwitch>();
        if (timeSwitch != null)
        {
            FinishImpact(other, hit);
            timeSwitch.Activate();
            return true;
        }

        // Level doors can block shots on layers other than Ground.
        if (other.gameObject.layer != groundLayer &&
            (!TimeSlowPrototype.IsPresent || other.isTrigger || other.GetComponentInParent<PlayerHealth>() != null))
            return false;

        FinishImpact(other, hit);
        return true;
    }

    private void FinishImpact(Collider2D target, RaycastHit2D hit)
    {
        hasHit = true;
        if (sparkMaterial != null)
        {
            Vector2 travel = transform.right;
            Vector2 point = target.ClosestPoint(transform.position);
            Vector2 normal = -travel;
            if (hit.collider != null)
            {
                point = hit.point;
                normal = hit.normal;
            }
            else if (hitCollider != null)
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
