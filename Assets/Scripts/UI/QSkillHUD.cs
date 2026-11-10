
using System.Collections;
using UnityEngine;
using TMPro;

public class QSkillHUD : MonoBehaviour
{
    public TimeSlow timeSlow;
    public UnityEngine.UI.Image cooldownFill;
    public TextMeshProUGUI countdownText;

    [Header("Highlight Settings")]
    public UnityEngine.UI.Image skillImage;
    public Color highlightColor = new Color(0.5f, 0.9f, 1f, 1f);
    public float highlightDuration = 0.3f;

    private Color originalColor;
    private bool wasCooldown = false;

    void Start()
    {
        if (skillImage != null)
            originalColor = skillImage.color;
    }

    void Update()
    {
        if (timeSlow == null || cooldownFill == null ||
            countdownText == null)
            return;

        countdownText.gameObject.SetActive(false);

        // Q skill active
        if (timeSlow.IsTimeSlowed)
        {
            cooldownFill.fillAmount = 0f;
            countdownText.gameObject.SetActive(true);

            float remaining = timeSlow.ActiveRemaining;

            if (remaining > 1f)
                countdownText.text = Mathf.CeilToInt(remaining).ToString();
            else
                countdownText.text = Mathf.Max(0f, remaining).ToString("F2");
        }
        // Q cooling down
        else if (timeSlow.IsCooldown)
        {
            cooldownFill.fillAmount =
                Mathf.Clamp01(timeSlow.CooldownRemaining / timeSlow.cooldown);
        }
        // Q ready
        else
        {
            cooldownFill.fillAmount = 0f;
        }

        // Detect cooldown completion
        if (wasCooldown && !timeSlow.IsCooldown)
        {
            StartCoroutine(HighlightSkill());
        }

        wasCooldown = timeSlow.IsCooldown;
    }

    IEnumerator HighlightSkill()
    {
        if (skillImage == null)
            yield break;

        float timer = 0f;

        while (timer < highlightDuration)
        {
            timer += Time.deltaTime;

            float progress = timer / highlightDuration;

            // Bright at first, then return to normal
            skillImage.color = Color.Lerp(
                highlightColor,
                originalColor,
                progress
            );

            yield return null;
        }

        skillImage.color = originalColor;
    }
}
