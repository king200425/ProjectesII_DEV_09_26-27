using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public Vector2 offset = new Vector2(0f, 1f);
    public Vector2 minBounds = new Vector2(-2f, 1f);
    public Vector2 maxBounds = new Vector2(24f, 27f);
    public float orthographicSize = 5f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null)
            return;

        float x = Mathf.Clamp(
            target.position.x + offset.x,
            minBounds.x,
            maxBounds.x);

        float y = Mathf.Clamp(
            target.position.y + offset.y,
            minBounds.y,
            maxBounds.y);

        Vector3 desired = new Vector3(x, y, -10f);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref velocity,
            smoothTime);
    }
}
