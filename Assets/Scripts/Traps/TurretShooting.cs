using UnityEngine;

public class TurretShooting : MonoBehaviour
{
    public Transform aim;
    public Transform muzzle;
    public GameObject boltPrefab;
    public float fireInterval = 1.4f;
    public float range = 24f;

    private Transform player;
    private float nextShot;

    void Start()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            player = health.transform;

        nextShot = 1.2f;
    }

    void Update()
    {
        if (player == null)
            return;

        Vector2 toPlayer = player.position - transform.position;
        if (aim != null)
        {
            float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            bool facingLeft = Mathf.Abs(angle) > 90f;
            float localAngle = facingLeft ? angle - 180f : angle;
            aim.rotation = Quaternion.Euler(0f, 0f, localAngle);
            aim.localScale = new Vector3(facingLeft ? -1f : 1f, 1f, 1f);
        }

        nextShot -= Time.deltaTime * TimeSlow.WorldScale;
        if (nextShot <= 0f && toPlayer.magnitude <= range)
        {
            Fire(toPlayer.normalized);
            nextShot = fireInterval;
        }
    }

    void Fire(Vector2 direction)
    {
        if (boltPrefab == null || muzzle == null)
            return;

        GameObject bolt = Instantiate(boltPrefab, muzzle.position, Quaternion.identity);
        bolt.transform.right = direction;
        TurretLaserBolt laser = bolt.GetComponent<TurretLaserBolt>();
        if (laser != null)
            laser.direction = direction;
    }
}
