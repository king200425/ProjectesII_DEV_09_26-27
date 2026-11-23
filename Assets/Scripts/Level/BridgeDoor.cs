using UnityEngine;

public class BridgeDoor : MonoBehaviour
{
    public ShootSwitch[] switches;
    public Transform bridge;
    public SpriteRenderer bridgeRenderer;
    public GameObject bridgeColliderRoot;
    public Vector3 hiddenOffset = new Vector3(0f, -3f, 0f);
    public float moveSpeed = 6f;

    public bool IsOpen { get; private set; }

    private Vector3 openPosition;
    private Vector3 hiddenPosition;

    void Awake()
    {
        if (bridge == null)
            return;

        openPosition = bridge.position;
        hiddenPosition = openPosition + hiddenOffset;
        bridge.position = hiddenPosition;
        SetVisible(false);
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

        bool active = AllActive();
        IsOpen = active;
        SetVisible(active);

        Vector3 target = active ? openPosition : hiddenPosition;

        bridge.position = Vector3.MoveTowards(
            bridge.position,
            target,
            moveSpeed * Time.deltaTime);
    }

    void SetVisible(bool visible)
    {
        if (bridgeRenderer != null)
            bridgeRenderer.enabled = visible;

        if (bridgeColliderRoot != null)
            bridgeColliderRoot.SetActive(visible);
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
