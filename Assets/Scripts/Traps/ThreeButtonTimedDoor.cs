using UnityEngine;

public class ThreeButtonTimedDoor : MonoBehaviour
{
    [Header("Buttons")]
    public CountdownDoorButton[] buttons;

    [Header("Door")]
    public Transform doorLeaf;
    public Vector3 openOffset = new Vector3(0f, 4f, 0f);
    public float moveSpeed = 5f;

    [Header("Window")]
    [Min(0.05f)] public float windowSeconds = 0.8f;

    private float[] pressTimes;
    private Vector3 closedPosition;

    void Awake()
    {
        pressTimes = new float[3];
        for (int i = 0; i < pressTimes.Length; i++)
            pressTimes[i] = -1f;

        if (doorLeaf != null)
            closedPosition = doorLeaf.position;
    }

    public void PressButton(int index)
    {
        if (index < 0 || index >= pressTimes.Length)
            return;

        pressTimes[index] = Time.time;

        if (AllPressed() && Latest() - Earliest() > windowSeconds)
            ResetButtons();
    }

    void Update()
    {
        if (doorLeaf == null)
            return;

        bool shouldOpen = AllPressed() && Latest() - Earliest() <= windowSeconds;
        Vector3 target = shouldOpen ? closedPosition + openOffset : closedPosition;
        doorLeaf.position = Vector3.MoveTowards(
            doorLeaf.position, target, moveSpeed * Time.deltaTime);

        if (!shouldOpen && AnyPressed() && Time.time - Earliest() > windowSeconds)
            ResetButtons();
    }

    bool AllPressed()
    {
        if (buttons == null || buttons.Length < 3)
            return false;

        for (int i = 0; i < 3; i++)
        {
            if (buttons[i] == null || !buttons[i].IsPressed || pressTimes[i] < 0f)
                return false;
        }

        return true;
    }

    bool AnyPressed()
    {
        foreach (float t in pressTimes)
        {
            if (t >= 0f)
                return true;
        }
        return false;
    }

    float Earliest()
    {
        float value = float.MaxValue;
        foreach (float t in pressTimes)
        {
            if (t >= 0f)
                value = Mathf.Min(value, t);
        }
        return value;
    }

    float Latest()
    {
        float value = float.MinValue;
        foreach (float t in pressTimes)
        {
            if (t >= 0f)
                value = Mathf.Max(value, t);
        }
        return value;
    }

    void ResetButtons()
    {
        for (int i = 0; i < pressTimes.Length; i++)
            pressTimes[i] = -1f;

        if (buttons == null)
            return;

        foreach (CountdownDoorButton button in buttons)
        {
            if (button != null)
                button.ResetButton();
        }
    }
}
