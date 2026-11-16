using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    // Bullet prefab
    public GameObject bulletPrefab;

    // Bullet spawn position
    public Transform muzzlePoint;

    // Time between shots
    public float fireRate = 0.1f;

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
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        if (Input.GetMouseButton(0) && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = fireRate;
        }
    }

    void Shoot()
    {
        // Spawn bullet at the muzzle
        Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            gunPivot.rotation
        );

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


