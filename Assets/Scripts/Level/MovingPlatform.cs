using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;
    public float speed = 2f;

    private float progress;
    private int direction = 1;

    void Start()
    {
        transform.position = pointA;
    }

    void Update()
    {
        float distance = Vector3.Distance(pointA, pointB);

        if (distance < 0.01f)
            return;

        float scale = TimeSlowPrototype.WorldScale;

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
