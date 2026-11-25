using UnityEngine;

public class DoorTrapGate : MonoBehaviour
{
    public ShootLock[] locks;
    public ShotPressureSwitch[] pressureSwitches;
    public Transform[] doorLeaves;
    public Vector3 openOffset = new Vector3(0f, 4f, 0f);
    public float moveSpeed = 3.5f;

    [Header("Auto Close After Passing")]
    public float closeDelay = 0.35f;

    private Vector3[] closedPositions;
    private Vector3[] openPositions;
    private Transform player;
    private bool playerXInitialized;
    private bool consumed;
    private float lastPlayerX;
    private float closeAt;

    void Awake()
    {
        if (doorLeaves == null || doorLeaves.Length == 0)
            return;

        closedPositions = new Vector3[doorLeaves.Length];
        openPositions = new Vector3[doorLeaves.Length];

        for (int i = 0; i < doorLeaves.Length; i++)
        {
            if (doorLeaves[i] == null)
                continue;

            closedPositions[i] = doorLeaves[i].position;
            openPositions[i] = closedPositions[i] + openOffset;
        }
    }

    void Start()
    {
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
            player = health.transform;
    }

    void Update()
    {
        if (doorLeaves == null || closedPositions == null)
            return;

        bool allActive = AllLocksActive();

        if (!consumed && allActive)
        {
            if (!playerXInitialized && player != null)
            {
                lastPlayerX = player.position.x;
                playerXInitialized = true;
            }

            if (HasCrossedDoor())
            {
                consumed = true;
                closeAt = Time.time + closeDelay;
            }

            if (player != null)
                lastPlayerX = player.position.x;
        }

        bool open = allActive && !consumed;
        if (consumed && Time.time < closeAt)
            open = true;

        for (int i = 0; i < doorLeaves.Length; i++)
        {
            if (doorLeaves[i] == null)
                continue;

            doorLeaves[i].position = Vector3.MoveTowards(
                doorLeaves[i].position,
                open ? openPositions[i] : closedPositions[i],
                moveSpeed * TimeSlow.WorldScale * Time.deltaTime);
        }

        if (consumed && DoorFullyClosed())
        {
            ResetSwitches();
            consumed = false;
            playerXInitialized = false;
        }
    }

    void ResetSwitches()
    {
        if (locks != null)
        {
            foreach (ShootLock item in locks)
            {
                if (item != null)
                    item.ResetLock();
            }
        }

        if (pressureSwitches != null)
        {
            foreach (ShotPressureSwitch item in pressureSwitches)
            {
                if (item != null)
                    item.ResetSwitch();
            }
        }
    }

    bool DoorFullyClosed()
    {
        return Vector3.Distance(
            doorLeaves[0].position,
            closedPositions[0]) <= 0.05f;
    }

    bool HasCrossedDoor()
    {
        if (player == null)
            return false;

        float doorX = closedPositions[0].x;
        float previousSide = lastPlayerX - doorX;
        float currentSide = player.position.x - doorX;

        return previousSide * currentSide < 0f;
    }

    bool AllLocksActive()
    {
        if (locks != null)
        {
            foreach (ShootLock item in locks)
            {
                if (item == null || !item.IsActive)
                    return false;
            }
        }

        if (pressureSwitches != null)
        {
            foreach (ShotPressureSwitch item in pressureSwitches)
            {
                if (item == null || !item.IsActive)
                    return false;
            }
        }

        return (locks != null && locks.Length > 0) ||
               (pressureSwitches != null && pressureSwitches.Length > 0);
    }
}
