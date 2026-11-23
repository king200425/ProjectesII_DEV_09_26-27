using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0f, 4f, 0f);
    public float moveSpeed = 5f;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool opened;
    private Collider2D doorCollider;

    void Awake()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
        doorCollider = GetComponent<Collider2D>();
    }

    public void Open()
    {
        opened = true;
        if (doorCollider != null)
            doorCollider.enabled = false;
    }

    void Update()
    {
        Vector3 target = opened ? openPosition : closedPosition;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }
}
