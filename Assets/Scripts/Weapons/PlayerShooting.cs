
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

    private float nextFireTime;

    void LateUpdate()
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
            gunPivot.rotation
        );

        // Play recoil when shooting
        if (gunRecoil != null)
        {
            gunRecoil.PlayRecoil();
        }
    }
}
