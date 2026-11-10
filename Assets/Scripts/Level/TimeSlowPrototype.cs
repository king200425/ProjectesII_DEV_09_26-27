using UnityEngine;

public class TimeSlowPrototype : MonoBehaviour
{
    public static float WorldScale { get; private set; } = 1f;
    public static bool IsSlowed { get; private set; }

    public float slowScale = 0.15f;
    public float maxDuration = 8f;
    public float cooldown = 1.5f;

    private float remaining;
    private float cooldownRemaining;

    void Awake()
    {
        WorldScale = 1f;
        IsSlowed = false;
    }

    void Update()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.unscaledDeltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && cooldownRemaining <= 0f)
        {
            if (IsSlowed)
                StopSlow();
            else
                StartSlow();
        }

        if (!IsSlowed)
            return;

        remaining -= Time.unscaledDeltaTime;

        if (remaining <= 0f)
            StopSlow();
    }

    public void StartSlow()
    {
        IsSlowed = true;
        WorldScale = slowScale;
        remaining = maxDuration;
    }

    public void StopSlow()
    {
        IsSlowed = false;
        WorldScale = 1f;
        cooldownRemaining = cooldown;
    }

    void OnDisable()
    {
        IsSlowed = false;
        WorldScale = 1f;
    }

    void OnGUI()
    {
        if (!IsSlowed)
            return;

        GUI.color = new Color(0.62f, 0.72f, 0.82f, 0.22f);
        GUI.DrawTexture(
            new Rect(0f, 0f, Screen.width, Screen.height),
            Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
