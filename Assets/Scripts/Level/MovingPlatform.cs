using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;
    public float speed = 2f;

    private float progress;
    private int direction = 1;
    private Transform rider;
    private Vector3 lastPosition;

    void Awake()
    {
        Rigidbody2D body = GetComponent<Rigidbody2D>();

        if (body == null)
            body = gameObject.AddComponent<Rigidbody2D>();

        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.simulated = true;
    }

    void Start()
    {
        transform.position = pointA;
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;

        if (rider != null)
            rider.position += delta;

        lastPosition = transform.position;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        PlayerHealth player = collision.collider.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        if (player.transform.position.y > transform.position.y + 0.2f)
            rider = player.transform;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        PlayerHealth player = collision.collider.GetComponentInParent<PlayerHealth>();

        if (player != null && rider == player.transform)
            rider = null;
    }

    void Update()
    {
        if (TimeSlowPrototype.IsSlowed)
            return;

        float distance = Vector3.Distance(pointA, pointB);

        if (distance < 0.01f)
            return;

        float scale = TimeSlowPrototype.WorldScale * TimeSlow.WorldScale;

        progress += direction * speed * scale * Time.deltaTime / distance;

        if (progress >= 1f)
        {
            progress = 1f;
            direction = -1;
        }
        else if (progress <= 0f)
        {
            progress = 0f;
            direction = 1;
        }

        transform.position = Vector3.Lerp(pointA, pointB, progress);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);
    }
}
