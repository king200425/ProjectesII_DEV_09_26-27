using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed = 8f;

    void Update()
    {
        transform.Rotate(0f, 0f, speed * Time.deltaTime);
    }
}
