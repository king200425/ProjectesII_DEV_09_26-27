using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecordingHUD : MonoBehaviour
{
    [Header("HUD Root")]
    public GameObject recordingHUDRoot;

    [Header("UI References")]
    public Image statusBorder;
    public Image statusFrame;
    public Image statusIcon;
    public TMP_Text statusText;
    public TMP_Text timerText;

    [Header("Border Sprites")]
    public Sprite borderRed;
    public Sprite borderYellow;
    public Sprite borderGreen;

    [Header("Frame Sprites")]
    public Sprite frameRed;
    public Sprite frameYellow;
    public Sprite frameGreen;

    [Header("Icon Sprites")]
    public Sprite recordDot;
    public Sprite readySquare;
    public Sprite playTriangle;

    [Header("Settings")]
    public float duration = 4f;
    public float blinkPeriod = 0.5f;
    public float minimumAlpha = 0.6f;

    [Header("Camera Focus Effect")]
    public float focusInDuration = 0.25f;
    public float focusOutDuration = 0.35f;
    public float startScale = 1.12f;
    public float endScale = 0.82f;

    private enum HUDState
    {
        Hidden,
        Recording,
        Ready,
        Playing
    }

    private HUDState currentState = HUDState.Hidden;

    private float blinkTimer;

    private CanvasGroup hudCanvasGroup;
    private Coroutine transitionCoroutine;

    void Awake()
    {
        // Add CanvasGroup to the HUD root
        if (recordingHUDRoot != null)
        {
            hudCanvasGroup =
                recordingHUDRoot.GetComponent<CanvasGroup>();

            if (hudCanvasGroup == null)
            {
                hudCanvasGroup =
                    recordingHUDRoot.AddComponent<CanvasGroup>();
            }
        }
    }

    void Start()
    {
        HideImmediately();
    }

    void Update()
    {
        // Blink only during recording and playback
        if (currentState != HUDState.Recording &&
            currentState != HUDState.Playing)
        {
            return;
        }

        if (statusIcon == null)
            return;

        blinkTimer += Time.unscaledDeltaTime;

        float blink =
            0.5f + 0.5f *
            Mathf.Cos(
                blinkTimer * Mathf.PI * 2f /
                Mathf.Max(0.1f, blinkPeriod)
            );

        Color color = statusIcon.color;

        color.a = Mathf.Lerp(
            minimumAlpha,
            1f,
            blink
        );

        statusIcon.color = color;
    }

    public void ShowRecording(float elapsed)
    {
        if (currentState != HUDState.Recording)
        {
            bool wasHidden =
                currentState == HUDState.Hidden;

            SetState(
                HUDState.Recording,
                borderRed,
                frameRed,
                recordDot,
                "REC"
            );

            // Camera focuses when recording starts
            if (wasHidden)
            {
                StartFocusIn();
            }
        }

        timerText.text =
            FormatTime(elapsed) + " / 04:00";
    }

    public void ShowReady()
    {
        if (currentState == HUDState.Ready)
            return;

        SetState(
            HUDState.Ready,
            borderYellow,
            frameYellow,
            readySquare,
            "READY"
        );

        timerText.text = "04.0s ECHO BUFFER";
    }

    public void ShowPlaying(float elapsed)
    {
        if (currentState != HUDState.Playing)
        {
            SetState(
                HUDState.Playing,
                borderGreen,
                frameGreen,
                playTriangle,
                "PLAY"
            );
        }

        timerText.text =
            FormatTime(elapsed) + " / 04:00";
    }

    public void HideHUD()
    {
        if (currentState == HUDState.Hidden)
            return;

        currentState = HUDState.Hidden;
        blinkTimer = 0f;

        StopTransition();

        if (recordingHUDRoot != null &&
            recordingHUDRoot.activeSelf)
        {
            transitionCoroutine =
                StartCoroutine(FocusOut());
        }
        else
        {
            HideImmediately();
        }
    }

    private void SetState(
        HUDState newState,
        Sprite border,
        Sprite frame,
        Sprite icon,
        string text)
    {
        StopTransition();

        currentState = newState;
        blinkTimer = 0f;

        // Show the whole HUD
        if (recordingHUDRoot != null)
        {
            recordingHUDRoot.SetActive(true);
        }

        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 1f;
        }

        if (statusBorder != null)
        {
            statusBorder.gameObject.SetActive(true);
            statusBorder.sprite = border;
            statusBorder.rectTransform.localScale =
                Vector3.one;
        }

        if (statusFrame != null)
        {
            statusFrame.gameObject.SetActive(true);
            statusFrame.sprite = frame;
        }

        if (statusIcon != null)
        {
            statusIcon.sprite = icon;
            statusIcon.color = Color.white;
        }

        if (statusText != null)
        {
            statusText.text = text;

            if (newState == HUDState.Recording)
            {
                statusText.color =
                    new Color32(255, 82, 100, 255);
            }
            else if (newState == HUDState.Ready)
            {
                statusText.color =
                    new Color32(255, 193, 7, 255);
            }
            else if (newState == HUDState.Playing)
            {
                statusText.color =
                    new Color32(57, 255, 153, 255);
            }
        }
    }

    // Start camera focus animation
    private void StartFocusIn()
    {
        StopTransition();

        transitionCoroutine =
            StartCoroutine(FocusIn());
    }

    private IEnumerator FocusIn()
    {
        if (hudCanvasGroup == null ||
            statusBorder == null)
        {
            transitionCoroutine = null;
            yield break;
        }

        RectTransform border =
            statusBorder.rectTransform;

        float timer = 0f;

        hudCanvasGroup.alpha = 0f;

        border.localScale =
            Vector3.one * startScale;

        while (timer < focusInDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(0.01f, focusInDuration)
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            // Fade in
            hudCanvasGroup.alpha = t;

            // Camera frame moves into focus
            border.localScale = Vector3.Lerp(
                Vector3.one * startScale,
                Vector3.one,
                t
            );

            yield return null;
        }

        hudCanvasGroup.alpha = 1f;
        border.localScale = Vector3.one;

        transitionCoroutine = null;
    }

    // Camera frame closes and fades out
    private IEnumerator FocusOut()
    {
        if (hudCanvasGroup == null ||
            statusBorder == null)
        {
            HideImmediately();
            yield break;
        }

        RectTransform border =
            statusBorder.rectTransform;

        float timer = 0f;

        float startAlpha =
            hudCanvasGroup.alpha;

        Vector3 originalScale =
            border.localScale;

        while (timer < focusOutDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(0.01f, focusOutDuration)
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            // Gradually disappear
            hudCanvasGroup.alpha =
                Mathf.Lerp(startAlpha, 0f, t);

            // Focus frame shrinks inward
            border.localScale = Vector3.Lerp(
                originalScale,
                Vector3.one * endScale,
                t
            );

            yield return null;
        }

        HideImmediately();
    }

    // Hide without animation (used at game start)
    private void HideImmediately()
    {
        currentState = HUDState.Hidden;
        blinkTimer = 0f;

        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 1f;
        }

        if (statusBorder != null)
        {
            statusBorder.rectTransform.localScale =
                Vector3.one;
        }

        if (recordingHUDRoot != null)
        {
            recordingHUDRoot.SetActive(false);
        }

        transitionCoroutine = null;
    }

    private void StopTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }

    private string FormatTime(float seconds)
    {
        seconds = Mathf.Clamp(
            seconds,
            0f,
            duration
        );

        int totalTenths =
            Mathf.FloorToInt(seconds * 10f);

        int wholeSeconds =
            totalTenths / 10;

        int tenths =
            totalTenths % 10;

        return $"00:{wholeSeconds:00}.{tenths}";
    }

    void OnDisable()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }
}
