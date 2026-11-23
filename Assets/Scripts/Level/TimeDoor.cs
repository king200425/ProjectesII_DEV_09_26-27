using UnityEngine;

public class TimeDoor : MonoBehaviour
{
    public TimeSwitch[] switches;
    public Transform door;
    public Collider2D doorCollider;
    public Vector3 openOffset = new Vector3(0f, 3.5f, 0f);
    public float moveSpeed = 4f;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Awake()
    {
        if (door == null)
            door = transform;

        closedPosition = door.position;
        openPosition = closedPosition + openOffset;

        if (doorCollider == null)
            doorCollider = door.GetComponent<Collider2D>();
    }

    void Update()
    {
        if (door == null)
            return;

        Vector3 target = AllActive() ? openPosition : closedPosition;

        door.position = Vector3.MoveTowards(
            door.position,
            target,
            moveSpeed * Time.deltaTime);

        if (doorCollider != null)
            doorCollider.enabled =
                Vector3.Distance(door.position, openPosition) > 0.05f;
    }

    bool AllActive()
    {
        if (switches == null || switches.Length == 0)
            return false;

        foreach (TimeSwitch item in switches)
        {
            if (item == null || !item.IsActive)
                return false;
        }

        return true;
    }
}
