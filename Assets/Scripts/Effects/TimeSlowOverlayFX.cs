using UnityEngine;
using UnityEngine.UI;

public class TimeSlowOverlayFX : MonoBehaviour
{
    [Header("References")]
    public TimeSlow timeSlow;
    public Transform player;
    public Camera mainCamera;

    [Header("Settings")]
    public float expandDuration = 0.3f;
    public float overlayAlpha = 0.63f;
    public float restoreDuration = 0.3f;

    [Header("Warning Flicker")]
    public float warningDuration = 1f;
    public float minFlickerSpeed = 3f;
    public float maxFlickerSpeed = 15f;

    private RectTransform overlayRect;
    private RectTransform circleRect;
    private Image circleImage;

    private Texture2D circleTexture;
    private Sprite circleSprite;

    private enum EffectState
    {
        Idle,
        Expanding,
        Active,
        Restoring
    }

    private EffectState currentState = EffectState.Idle;

    private float progress;
    private bool wasTimeSlowed;
    private float flickerTimer;

    private Color grayColor = new Color(
        0.47f, 0.49f, 0.53f, 1f
    );

    void Start()
    {
        overlayRect = GetComponent<RectTransform>();

        Image background = GetComponent<Image>();

        if (background != null)
        {
            Color color = background.color;
            color.a = 0f;
            background.color = color;
            background.raycastTarget = false;
        }

        GameObject circle = new GameObject(
            "GrayCircle",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        circle.transform.SetParent(transform, false);

        circleRect = circle.GetComponent<RectTransform>();
        circleRect.anchorMin = new Vector2(0.5f, 0.5f);
        circleRect.anchorMax = new Vector2(0.5f, 0.5f);
        circleRect.pivot = new Vector2(0.5f, 0.5f);

        circleImage = circle.GetComponent<Image>();
        circleImage.sprite = CreateCircleSprite();
        circleImage.color = grayColor;
        circleImage.raycastTarget = false;

        circle.SetActive(false);
    }

    void Update()
    {
        if (timeSlow == null || player == null ||
            mainCamera == null || circleImage == null)
            return;

        bool active = timeSlow.IsTimeSlowed;

        // Q started
        if (active && !wasTimeSlowed)
        {
            currentState = EffectState.Expanding;
            progress = 0f;
            flickerTimer = 0f; // Reset flicker timer
            circleImage.gameObject.SetActive(true);
        }

        // Q ended
        if (!active && wasTimeSlowed)
        {
            currentState = EffectState.Restoring;
            progress = 0f;
        }

        if (currentState == EffectState.Expanding)
        {
            progress += Time.deltaTime /
                Mathf.Max(0.01f, expandDuration);

            UpdateCircle(Mathf.Clamp01(progress));

            SetCircleAlpha(overlayAlpha);

            if (progress >= 1f)
                currentState = EffectState.Active;
        }
        else if (currentState == EffectState.Active)
        {
            UpdateCircle(1f);

            float alpha = overlayAlpha;

            // Flicker during the final second
            if (timeSlow.ActiveRemaining <= warningDuration)
            {
                float remaining = Mathf.Clamp01(
                    timeSlow.ActiveRemaining / warningDuration
                );

                flickerTimer += Time.deltaTime;

                float flicker =
                    0.5f + 0.5f *
                    Mathf.Cos(flickerTimer * Mathf.PI * 2f / 0.50f);

                alpha = Mathf.Lerp(
                    overlayAlpha * 0.6f,
                    overlayAlpha,
                    flicker
                );
            }

            SetCircleAlpha(alpha);
        }
        else if (currentState == EffectState.Restoring)
        {
            progress += Time.deltaTime /
                Mathf.Max(0.01f, restoreDuration);

            // Temporary reverse expansion
            UpdateCircle(1f - Mathf.Clamp01(progress));

            SetCircleAlpha(overlayAlpha);

            if (progress >= 1f)
            {
                currentState = EffectState.Idle;
                circleImage.gameObject.SetActive(false);
            }
        }

        wasTimeSlowed = active;
    }

    void UpdateCircle(float amount)
    {
        Vector2 screenPosition =
            mainCamera.WorldToScreenPoint(player.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayRect,
            screenPosition,
            null,
            out Vector2 localPosition
        );

        circleRect.anchoredPosition = localPosition;

        Rect rect = overlayRect.rect;

        float maxX = Mathf.Max(
            Mathf.Abs(rect.xMin - localPosition.x),
            Mathf.Abs(rect.xMax - localPosition.x)
        );

        float maxY = Mathf.Max(
            Mathf.Abs(rect.yMin - localPosition.y),
            Mathf.Abs(rect.yMax - localPosition.y)
        );

        float radius = Mathf.Sqrt(maxX * maxX + maxY * maxY);

        float t = Mathf.SmoothStep(0f, 1f, amount);

        float size = radius * 2f * t;

        circleRect.sizeDelta = new Vector2(size, size);
    }

    void SetCircleAlpha(float alpha)
    {
        Color color = grayColor;
        color.a = alpha;
        circleImage.color = color;
    }

    Sprite CreateCircleSprite()
    {
        int size = 128;

        circleTexture = new Texture2D(
            size, size, TextureFormat.RGBA32, false
        );

        circleTexture.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float radius = center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(center, center)
                );

                float alpha = Mathf.Clamp01(radius - distance);

                circleTexture.SetPixel(
                    x, y,
                    new Color(1f, 1f, 1f, alpha)
                );
            }
        }

        circleTexture.Apply();

        circleSprite = Sprite.Create(
            circleTexture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f)
        );

        return circleSprite;
    }

    void OnDestroy()
    {
        if (circleSprite != null)
            Destroy(circleSprite);

        if (circleTexture != null)
            Destroy(circleTexture);
    }
}
