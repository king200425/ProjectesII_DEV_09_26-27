using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    // Recoil distance
    public float recoilDistance = 0.08f;

    // Return speed
    public float returnSpeed = 15f;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        // Smoothly return the gun to its original position
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            originalPosition,
            returnSpeed * Time.deltaTime
        );
    }

    public void PlayRecoil()
    {
        // Move the gun backward when shooting
        transform.localPosition =
            originalPosition + Vector3.left * recoilDistance;
    }
}
