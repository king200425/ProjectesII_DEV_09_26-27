using UnityEngine;

public class CountdownDoorButton : MonoBehaviour
{
    public ThreeButtonTimedDoor door;
    public int buttonIndex;
    public float countdownSeconds = 0.8f;

    private bool pressed;
    private float remaining;
    private Quaternion startRotation;

    public bool IsPressed => pressed;

    void Awake()
    {
        startRotation = transform.localRotation;
    }

    public void Hit()
    {
        if (pressed)
            return;

        pressed = true;
        remaining = countdownSeconds;

        if (door != null)
            door.PressButton(buttonIndex);
    }

    void Update()
    {
        if (!pressed)
            return;

        remaining -= Time.deltaTime;

        float progress = 1f - Mathf.Clamp01(remaining / Mathf.Max(0.01f, countdownSeconds));
        transform.localRotation = startRotation * Quaternion.Euler(0f, 0f, -360f * progress);

        if (remaining <= 0f)
            ResetButton();
    }

    public void ResetButton()
    {
        pressed = false;
        remaining = 0f;
        transform.localRotation = startRotation;
    }
}
