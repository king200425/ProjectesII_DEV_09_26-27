using UnityEngine;

public class BridgeDoor : MonoBehaviour
{
    public ShootSwitch[] switches;
    public Transform bridge;
    public Vector3 hiddenOffset = new Vector3(0f, -3f, 0f);
    public float moveSpeed = 6f;

    private Vector3 openPosition;
    private Vector3 hiddenPosition;

    void Awake()
    {
        if (bridge == null)
            return;

        openPosition = bridge.position;
        hiddenPosition = openPosition + hiddenOffset;
        bridge.position = hiddenPosition;
    }

    public void CheckSwitches()
    {
        if (bridge == null)
            return;

        if (AllActive())
            bridge.position = openPosition;
    }

    void Update()
    {
        if (bridge == null)
            return;

        Vector3 target = AllActive() ? openPosition : hiddenPosition;

        bridge.position = Vector3.MoveTowards(
            bridge.position,
            target,
            moveSpeed * Time.deltaTime);
    }

    bool AllActive()
    {
        if (switches == null || switches.Length == 0)
            return false;

        foreach (ShootSwitch item in switches)
        {
            if (item == null || !item.IsActive)
                return false;
        }

        return true;
    }
}
