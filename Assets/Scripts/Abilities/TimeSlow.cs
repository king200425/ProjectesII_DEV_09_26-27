using UnityEngine;

public class TimeSlow : MonoBehaviour
{
    [Header("Q 时间减速 · 能量")]
    [InspectorName("满能量持续秒数"), Min(0.1f)]
    public float duration = 4f;
    [InspectorName("世界速度倍率"), Range(0.001f, 1f)]
    public float slowMultiplier = 0.02f;
    [InspectorName("空能量回满秒数"), Min(0.1f)]
    public float rechargeDuration = 5f;
    [InspectorName("低能量提醒比例"), Range(0.01f, 0.9f)]
    public float lowEnergyThreshold = 0.25f;

    public static float WorldScale { get; private set; } = 1f;
    public bool IsTimeSlowed { get; private set; }
    public float EnergyPercent => energy;
    public bool IsLowEnergy => energy <= lowEnergyThreshold;
    public bool IsRecoveringFromEmpty { get; private set; }
    public const float ExhaustionRecoveryThreshold = 1f / 3f;
    public float ActiveRemaining => energy * Mathf.Max(0.1f, duration);
    public bool CanActivate => isActiveAndEnabled && Time.timeScale > 0f && energy > 0f && !IsRecoveringFromEmpty &&
        (playerRecorder == null || (!playerRecorder.IsRecording && !playerRecorder.IsReplaying));

    private PlayerRecorder playerRecorder;
    private float energy = 1f;

    void Awake()
    {
        playerRecorder = GetComponent<PlayerRecorder>();
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            ToggleTimeSlow();

        if (IsTimeSlowed)
        {
            energy = Mathf.Max(0f, energy - Time.deltaTime / Mathf.Max(0.1f, duration));
            if (energy <= 0f)
            {
                IsRecoveringFromEmpty = true;
                StopTimeSlow();
            }
        }
        else
        {
            energy = Mathf.Min(1f, energy + Time.deltaTime / Mathf.Max(0.1f, rechargeDuration));
            if (IsRecoveringFromEmpty && energy >= ExhaustionRecoveryThreshold)
                IsRecoveringFromEmpty = false;
        }
    }

    public bool ToggleTimeSlow()
    {
        if (!isActiveAndEnabled || Time.timeScale <= 0f)
            return false;

        // Closing Q is always allowed; the E interlock only prevents opening it.
        if (IsTimeSlowed)
        {
            StopTimeSlow();
            return true;
        }

        if (!CanActivate)
            return false;

        IsTimeSlowed = true;
        WorldScale = Mathf.Clamp(slowMultiplier, 0.001f, 1f);
        return true;
    }

    public void StopTimeSlow()
    {
        IsTimeSlowed = false;
        WorldScale = 1f;
    }

    void OnDisable()
    {
        StopTimeSlow();
    }
}
