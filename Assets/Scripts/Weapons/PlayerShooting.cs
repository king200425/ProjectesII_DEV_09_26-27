using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    // Bullet prefab
    public GameObject bulletPrefab;

    // Bullet spawn position
    public Transform muzzlePoint;

    // Time between shots
    public float fireRate = 0.1f;
    [SerializeField, Range(0.01f, 1f)] private float slowFireScale = 0.25f;

    // Gun recoil component
    public GunRecoil gunRecoil;

    // Gun aiming pivot
    public Transform gunPivot;

    private float fireCooldown;
    private PlayerRecorder playerRecorder;

    void Start()
    {
        playerRecorder = GetComponent<PlayerRecorder>();
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0f)
            return;

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        // Hold left mouse button to shoot continuously
        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    void Shoot()
    {
        // Spawn bullet at the muzzle
        GameObject bullet = Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            gunPivot.rotation
        );

        Bullet projectile = bullet.GetComponent<Bullet>();
        if (projectile != null)
            projectile.isGhostBullet = false;

        // Record the shot during E recording
        if (playerRecorder != null)
        {
            playerRecorder.RecordShot(
                muzzlePoint.position,
                gunPivot.rotation
            );
        }

        // Play recoil when shooting
        if (gunRecoil != null)
        {
            gunRecoil.PlayRecoil();
        }
    }
}

