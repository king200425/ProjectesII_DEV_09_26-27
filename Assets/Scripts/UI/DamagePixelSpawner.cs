using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePixelSpawner : MonoBehaviour
{
    [Header("References")]
    public RectTransform spawnArea;
    public RectTransform pixelParent;
    public GameObject pixelPrefab;

    [Header("Spawn Settings")]
    public int maxPixels = 20;
    public float spawnInterval = 0.15f;
    public float pixelLifetime = 0.8f;

    [Header("Spawn Range")]
    public float edgeThickness = 120f;

    [Header("Pixel Size")]
    public float minSize = 4f;
    public float maxSize = 14f;

    [Header("Movement")]
    public float floatDistance = 10f;

    [Header("Pixel Colors")]
    public Color[] pixelColors =
    {
        new Color(1f, 0.15f, 0.15f, 1f),
        new Color(0.65f, 0.05f, 0.1f, 1f),
        new Color(1f, 0.45f, 0.45f, 1f),
        new Color(0.85f, 0.2f, 0.3f, 1f)
    };

    private Coroutine spawnCoroutine;
    private List<GameObject> spawnedPixels =
        new List<GameObject>();

    public void StartEffect()
    {
        StopEffect();
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopEffect()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        ClearAllPixels();
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (spawnedPixels.Count < maxPixels)
                SpawnPixel();

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPixel()
    {
        if (pixelPrefab == null || pixelParent == null ||
            spawnArea == null)
            return;

        GameObject pixel = Instantiate(pixelPrefab, pixelParent);
        RectTransform rect = pixel.GetComponent<RectTransform>();
        UnityEngine.UI.Image image =
            pixel.GetComponent<UnityEngine.UI.Image>();

        if (rect == null || image == null)
        {
            Destroy(pixel);
            return;
        }

        // Random edge position
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = GetRandomEdgePosition();

        // Random size and shape
        float width = Random.Range(minSize, maxSize);
        float height = Random.Range(minSize, maxSize);

        rect.sizeDelta = new Vector2(
            Mathf.Round(width),
            Mathf.Round(height)
        );

        // Random color
        if (pixelColors != null && pixelColors.Length > 0)
        {
            image.color =
                pixelColors[Random.Range(0, pixelColors.Length)];
        }

        image.raycastTarget = false;

        spawnedPixels.Add(pixel);
        StartCoroutine(AnimatePixel(pixel, rect, image));
    }

    Vector2 GetRandomEdgePosition()
    {
        float halfW = spawnArea.rect.width * 0.5f;
        float halfH = spawnArea.rect.height * 0.5f;

        float thickness = Mathf.Min(
            edgeThickness,
            halfW,
            halfH
        );

        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0: // Top
                return new Vector2(
                    Random.Range(-halfW, halfW),
                    Random.Range(halfH - thickness, halfH)
                );

            case 1: // Bottom
                return new Vector2(
                    Random.Range(-halfW, halfW),
                    Random.Range(-halfH, -halfH + thickness)
                );

            case 2: // Left
                return new Vector2(
                    Random.Range(-halfW, -halfW + thickness),
                    Random.Range(-halfH, halfH)
                );

            default: // Right
                return new Vector2(
                    Random.Range(halfW - thickness, halfW),
                    Random.Range(-halfH, halfH)
                );
        }
    }

    IEnumerator AnimatePixel(
        GameObject pixel,
        RectTransform rect,
        UnityEngine.UI.Image image)
    {
        Color originalColor = image.color;
        Vector2 startPosition = rect.anchoredPosition;

        Vector2 offset = new Vector2(
            Random.Range(-floatDistance, floatDistance),
            Random.Range(-floatDistance, floatDistance)
        );

        float elapsed = 0f;

        while (elapsed < pixelLifetime)
        {
            // Stop animation if the pixel was destroyed
            if (pixel == null || rect == null || image == null)
                yield break;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / pixelLifetime);

            // Small pixel movement
            rect.anchoredPosition = Vector2.Lerp(
                startPosition,
                startPosition + offset,
                t
            );

            // Random-looking flicker and fade
            float flicker = 0.75f +
                0.25f * Mathf.Sin(elapsed * 25f);

            Color color = originalColor;
            color.a = originalColor.a *
                (1f - t) * flicker;

            image.color = color;

            yield return null;
        }

        spawnedPixels.Remove(pixel);
        Destroy(pixel);
    }

    void ClearAllPixels()
    {
        // Stop all pixel animation coroutines
        StopAllCoroutines();

        for (int i = 0; i < spawnedPixels.Count; i++)
        {
            if (spawnedPixels[i] != null)
                Destroy(spawnedPixels[i]);
        }

        spawnedPixels.Clear();
        spawnCoroutine = null;
    }
}
