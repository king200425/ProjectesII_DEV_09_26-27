using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QSkillHUD : MonoBehaviour
{
    [Header("References")]
    public TimeSlow timeSlow;
    // Reuse the existing reference instead of requiring every HUD to be rebound.
    public Image cooldownFill;
    public TextMeshProUGUI countdownText;
    public Image skillImage;

    [Header("Energy Colors")]
    public Color normalColor = new Color(0.1f, 0.8f, 1f, 1f);
    public Color lowEnergyColor = new Color(1f, 0.3f, 0.2f, 1f);
    public float blinkSpeed = 8f;

    [Header("Highlight Settings")]
    public Color highlightColor = new Color(0.5f, 0.9f, 1f, 1f);
    public float highlightDuration = 0.3f;

    private Color originalColor;
    private bool wasFull;
    private bool wasRecovering;
    private float highlightRemaining;
    private Sprite fallbackSprite;
    private bool initialized;

    void Start()
    {
        if (timeSlow == null)
            timeSlow = FindFirstObjectByType<TimeSlow>();
        if (skillImage != null)
            originalColor = skillImage.color;
        initialized = true;
        wasFull = timeSlow != null && timeSlow.EnergyPercent >= 1f;
        wasRecovering = timeSlow != null && timeSlow.IsRecoveringFromEmpty;
        ConfigureEnergyBar();
        Refresh();
    }

    void ConfigureEnergyBar()
    {
        if (cooldownFill != null)
        {
            // The old fill covered the Q icon. Move it into a narrow track inside the slot.
            var trackObject = new GameObject("QEnergyTrack", typeof(RectTransform), typeof(Image), typeof(Outline));
            trackObject.layer = cooldownFill.gameObject.layer;
            var track = trackObject.GetComponent<RectTransform>();
            track.SetParent(transform, false);
            track.anchorMin = new Vector2(0f, 0f);
            track.anchorMax = new Vector2(1f, 0f);
            track.pivot = new Vector2(0.5f, 0f);
            track.anchoredPosition = new Vector2(0f, 7f);
            track.sizeDelta = new Vector2(-16f, 12f);
            var background = trackObject.GetComponent<Image>();
            background.color = new Color(0.015f, 0.035f, 0.065f, 0.95f);
            background.raycastTarget = false;
            var outline = trackObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.18f, 0.6f, 0.75f, 0.9f);
            outline.effectDistance = Vector2.one;

            var fillRect = cooldownFill.rectTransform;
            fillRect.SetParent(track, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            if (cooldownFill.sprite == null)
            {
                fallbackSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
                cooldownFill.sprite = fallbackSprite;
            }
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Horizontal;
            cooldownFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            cooldownFill.raycastTarget = false;
        }

        if (countdownText != null)
        {
            var textRect = countdownText.rectTransform;
            textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0f);
            textRect.pivot = new Vector2(0.5f, 0f);
            textRect.anchoredPosition = new Vector2(0f, 22f);
            textRect.sizeDelta = new Vector2(108f, 24f);
            countdownText.margin = Vector4.zero;
            countdownText.fontSize = 18f;
            countdownText.enableAutoSizing = false;
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.raycastTarget = false;
            countdownText.transform.SetAsLastSibling();
        }
    }

    void Update() => Refresh();

    void Refresh()
    {
        if (timeSlow == null)
            return;

        float energy = timeSlow.EnergyPercent;
        Color energyColor = normalColor;
        if (timeSlow.IsRecoveringFromEmpty)
        {
            energyColor = new Color(0.45f, 0.52f, 0.6f, 1f);
        }
        else if (timeSlow.IsLowEnergy)
        {
            float pulse = timeSlow.IsTimeSlowed ? Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f) : 0f;
            energyColor = Color.Lerp(lowEnergyColor, normalColor, pulse);
        }
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = energy;
            cooldownFill.color = energyColor;
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            // A non-empty bar should never claim 0%; an incomplete bar should never claim 100%.
            int percent = energy >= 1f ? 100 : energy <= 0f ? 0 : Mathf.Clamp(Mathf.FloorToInt(energy * 100f), 1, 99);
            countdownText.text = percent + "%";
            countdownText.color = timeSlow.IsLowEnergy || timeSlow.IsRecoveringFromEmpty ? energyColor : Color.white;
        }

        bool full = energy >= 1f;
        if ((full && !wasFull) || (wasRecovering && !timeSlow.IsRecoveringFromEmpty))
            highlightRemaining = Mathf.Max(0f, highlightDuration);
        wasFull = full;
        wasRecovering = timeSlow.IsRecoveringFromEmpty;
        if (skillImage != null)
        {
            Color resting = timeSlow.IsTimeSlowed ? highlightColor : originalColor;
            if (timeSlow.IsRecoveringFromEmpty)
                resting = Color.Lerp(originalColor, Color.gray, 0.75f);
            skillImage.color = highlightRemaining > 0f ? Color.Lerp(resting, highlightColor, highlightRemaining / Mathf.Max(0.01f, highlightDuration)) : resting;
        }
        highlightRemaining = Mathf.Max(0f, highlightRemaining - Time.unscaledDeltaTime);
    }

    void OnDisable()
    {
        highlightRemaining = 0f;
        if (initialized && skillImage != null)
            skillImage.color = originalColor;
    }

    void OnDestroy()
    {
        if (fallbackSprite != null)
            Destroy(fallbackSprite);
    }
}
