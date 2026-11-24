using UnityEngine;

public class RotatingLaserTrap : MonoBehaviour
{
    [Header("Rotation")]
    public Transform rotator;
    public float degreesPerSecond = 160f;
    public bool clockwise = true;

    void Update()
    {
        if (rotator == null)
            return;

        float direction = clockwise ? 1f : -1f;
        rotator.Rotate(0f, 0f, direction * degreesPerSecond * Time.deltaTime);
    }
}
