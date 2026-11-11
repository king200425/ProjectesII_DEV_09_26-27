using System.Collections;
using UnityEngine;

public class DamageScreenFX : MonoBehaviour
{
    [Header("References")]
    public UnityEngine.UI.Image damagePulse;
    public UnityEngine.UI.Image damageBorder;
    public DamagePixelSpawner pixelSpawner;

    [Header("Settings")]
    public float borderAlpha = 0.6f;
    public float basePulseAlpha = 0.15f;
    public float pulseAlpha = 0.45f;
    public float fadeDuration = 0.8f;

    [Header("Heartbeat Scale")]
    public float beatScale = 1.05f;
    public float beatDuration = 0.25f;
    public float beatInterval = 0.8f;

    private Coroutine effectCoroutine;

    private RectTransform damageRect;
    private Vector3 originalScale;

    void Awake()
    {
        damageRect = GetComponent<RectTransform>();
        originalScale = damageRect.localScale;
    }

    void Start()
    {
        SetAlpha(damagePulse, 0f);
        SetAlpha(damageBorder, 0f);
    }

    public void StartDamage()
    {
        StopEffect();
        ResetScale();

        SetAlpha(damageBorder, borderAlpha);
        SetAlpha(damagePulse, basePulseAlpha);

        // Start random damage pixels
        if (pixelSpawner != null)
        {
            pixelSpawner.StartEffect();
        }

        effectCoroutine = StartCoroutine(Heartbeat());
    }

    public void Recover()
    {
        StopEffect();
        ResetScale();

        // Stop random damage pixels
        if (pixelSpawner != null)
        {
            pixelSpawner.StopEffect();
        }

        effectCoroutine = StartCoroutine(FadeOut());
    }

    public void StopEffects()
    {
        StopEffect();
        ResetScale();

        // Remove random damage pixels
        if (pixelSpawner != null)
        {
            pixelSpawner.StopEffect();
        }

        SetAlpha(damagePulse, 0f);
        SetAlpha(damageBorder, 0f);
    }

    void StopEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }
    }

    IEnumerator Heartbeat()
    {
        while (true)
        {
            // One heartbeat: expand and return
            yield return StartCoroutine(Pulse());

            // Pause before next heartbeat
            yield return new WaitForSeconds(beatInterval);
        }
    }

    IEnumerator Pulse()
    {
        float elapsed = 0f;

        while (elapsed < beatDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / beatDuration);
            float beat = Mathf.Sin(t * Mathf.PI);

            // Red becomes stronger during heartbeat
            float alpha = Mathf.Lerp(
                basePulseAlpha,
                pulseAlpha,
                beat
            );

            SetAlpha(damagePulse, alpha);

            // Expand the entire DamageFX
            float scale = Mathf.Lerp(1f, beatScale, beat);
            damageRect.localScale = originalScale * scale;

            yield return null;
        }

        SetAlpha(damagePulse, basePulseAlpha);
        ResetScale();
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;

        float startBorder = damageBorder.color.a;
        float startPulse = damagePulse.color.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);

            SetAlpha(
                damageBorder,
                Mathf.Lerp(startBorder, 0f, t)
            );

            SetAlpha(
                damagePulse,
                Mathf.Lerp(startPulse, 0f, t)
            );

            yield return null;
        }

        SetAlpha(damageBorder, 0f);
        SetAlpha(damagePulse, 0f);

        ResetScale();
        effectCoroutine = null;
    }

    void ResetScale()
    {
        if (damageRect != null)
        {
            damageRect.localScale = originalScale;
        }
    }

    void SetAlpha(UnityEngine.UI.Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
