using System.Collections;
using UnityEngine;

public class TimeSlow : MonoBehaviour
{
    [Header("Time Slow Settings")]
    public float duration = 4f;
    public float slowMultiplier = 0.02f;
    public float cooldown = 10f;

    public bool IsTimeSlowed { get; private set; }

    private bool isCooldown = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) &&
            !IsTimeSlowed &&
            !isCooldown)
        {
            StartCoroutine(ActivateTimeSlow());
        }
    }

    IEnumerator ActivateTimeSlow()
    {
        IsTimeSlowed = true;

        UnityEngine.Debug.Log("Time Slow Activated!");

        // Skill duration
        yield return new WaitForSeconds(duration);

        IsTimeSlowed = false;

        UnityEngine.Debug.Log("Time Slow Ended!");

        // Start cooldown
        isCooldown = true;

        yield return new WaitForSeconds(cooldown);

        isCooldown = false;

        UnityEngine.Debug.Log("Time Slow Ready!");
    }
}
