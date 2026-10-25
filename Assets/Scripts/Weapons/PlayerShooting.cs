
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

    private float nextFireTime;

    void Update()
    {
        // Hold left mouse button to shoot continuously
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        // Spawn bullet at the muzzle
        Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            muzzlePoint.rotation
        );

        // Play recoil when shooting
        if (gunRecoil != null)
        {
            gunRecoil.PlayRecoil();
        }
    }
}
