using UnityEngine;

public class RoomCameraZone : MonoBehaviour
{
    public Transform cameraPoint;
    public float orthographicSize = 5.8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (RoomCameraDirector.Instance == null)
            return;

        RoomCameraDirector.Instance.Focus(cameraPoint, orthographicSize);
    }
}
