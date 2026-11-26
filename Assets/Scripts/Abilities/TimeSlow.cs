using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class TimeSlow : MonoBehaviour
{
    private PlayerRecorder playerRecorder;

    void Start()
    {
        playerRecorder = GetComponent<PlayerRecorder>();
    }

    [Header("Time Slow Settings")]
    public float duration = 4f;
    public float slowMultiplier = 0.02f;
    public float cooldown = 10f;

    public static float WorldScale { get; private set; } = 1f;

    public bool IsTimeSlowed { get; private set; }
    public bool IsCooldown { get; private set; }

    public float CooldownRemaining { get; private set; }
    public float ActiveRemaining { get; private set; }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (Input.GetKeyDown(KeyCode.Q) &&
            !IsTimeSlowed &&
            !IsCooldown)
        {
            // Cannot use Q while E is recording or replaying
            if (playerRecorder != null &&
                (playerRecorder.IsRecording || playerRecorder.IsReplaying))
            {
                return;
            }

            StartCoroutine(ActivateTimeSlow());
        }
    }

    IEnumerator ActivateTimeSlow()
    {
        IsTimeSlowed = true;
        ActiveRemaining = duration;
        WorldScale = slowMultiplier;

        UnityEngine.Debug.Log("Time Slow Activated!");

        while (ActiveRemaining > 0f)
        {
            ActiveRemaining -= Time.deltaTime;
            yield return null;
        }

        ActiveRemaining = 0f;
        IsTimeSlowed = false;
        WorldScale = 1f;

        UnityEngine.Debug.Log("Time Slow Ended!");

        IsCooldown = true;
        CooldownRemaining = cooldown;

        while (CooldownRemaining > 0f)
        {
            CooldownRemaining -= Time.deltaTime;
            yield return null;
        }

        CooldownRemaining = 0f;
        IsCooldown = false;

        UnityEngine.Debug.Log("Time Slow Ready!");
    }

    void OnDisable()
    {
        WorldScale = 1f;
    }
}
