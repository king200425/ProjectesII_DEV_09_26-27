using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ShotPressureSwitchFeedback : MonoBehaviour
{
    public Material sparkMaterial;
    public bool screenFlash = true;
    [Range(0f, 0.2f)] public float flashOpacity = 0.055f;
    [Min(0.05f)] public float flashSeconds = 0.25f;
    public int PlayCount { get; private set; }
    GameObject overlay;

    public void Play(Vector3 position)
    {
        PlayCount++;
        if (sparkMaterial != null)
            ProjectileSparks.PlayImpact(position, transform.up, sparkMaterial,
                new Color(0.18f, 1f, 0.35f), new Color(0.76f, 1f, 0.85f));
        if (screenFlash && isActiveAndEnabled) StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        if (overlay != null) Destroy(overlay);
        overlay = new GameObject("Pressure Switch Unlock Flash", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        GameObject wash = new GameObject("Green Pulse", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        wash.transform.SetParent(overlay.transform, false);
        RectTransform rect = wash.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        Image image = wash.GetComponent<Image>();
        image.raycastTarget = false;
        float elapsed = 0;
        while (elapsed < flashSeconds)
        {
            image.color = new Color(0.2f, 1f, 0.42f,
                flashOpacity * Mathf.Pow(1f - elapsed / flashSeconds, 2f));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        Destroy(overlay);
        overlay = null;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (overlay != null) Destroy(overlay);
        overlay = null;
    }
}
