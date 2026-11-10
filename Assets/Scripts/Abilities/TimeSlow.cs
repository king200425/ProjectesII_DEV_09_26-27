using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class TimeSlow : MonoBehaviour
{
    [Header("Time Slow Settings")]
    public float duration = 4f;
    public float slowMultiplier = 0.02f;
    public float cooldown = 10f;

    public bool IsTimeSlowed { get; private set; }
    public bool IsCooldown { get; private set; }

    public float CooldownRemaining { get; private set; }
    public float ActiveRemaining { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) &&
            !IsTimeSlowed &&
            !IsCooldown)
        {
            StartCoroutine(ActivateTimeSlow());
        }
    }

    IEnumerator ActivateTimeSlow()
    {
        IsTimeSlowed = true;
        ActiveRemaining = duration;

        UnityEngine.Debug.Log("Time Slow Activated!");

        while (ActiveRemaining > 0f)
        {
            ActiveRemaining -= Time.deltaTime;
            yield return null;
        }

        ActiveRemaining = 0f;
        IsTimeSlowed = false;

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
}
