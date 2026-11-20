using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class ChronoSceneTransition : MonoBehaviour
{
    private Material curtain;
    private Texture2D departure;
    private float travel;
    private Material particleMaterial;
    private ChronoTunnelParticles particles;
    private Vector2 departureUV;

    public static ChronoSceneTransition Create(Material source, Texture2D snapshot, RectTransform clock)
    {
        var root = new GameObject("Chrono Scene Transition", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        DontDestroyOnLoad(root);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;
        var imageObject = new GameObject("Time Tunnel", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        var rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(root.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var transition = root.AddComponent<ChronoSceneTransition>();
        transition.departure = snapshot;
        transition.curtain = new Material(source);
        transition.curtain.SetFloat("_Mode", 2f);
        transition.curtain.SetFloat("_Cover", 0f);
        transition.curtain.SetFloat("_Reveal", 0f);
        transition.curtain.SetFloat("_Travel", 0f);
        Vector2 clockScreen = RectTransformUtility.WorldToScreenPoint(null, clock.position);
        transition.departureUV = new Vector2(clockScreen.x / Screen.width, clockScreen.y / Screen.height);
        transition.curtain.SetVector("_ClockUV", new Vector4(transition.departureUV.x, transition.departureUV.y, 0f, 0f));
        var image = imageObject.GetComponent<RawImage>();
        image.texture = snapshot;
        image.material = transition.curtain;
        var particleObject = new GameObject("Temporal Particles", typeof(RectTransform), typeof(CanvasRenderer), typeof(ChronoTunnelParticles));
        var particleRect = particleObject.GetComponent<RectTransform>();
        particleRect.SetParent(root.transform, false);
        particleRect.anchorMin = Vector2.zero;
        particleRect.anchorMax = Vector2.one;
        particleRect.offsetMin = particleRect.offsetMax = Vector2.zero;
        transition.particleMaterial = new Material(source);
        transition.particleMaterial.SetFloat("_Mode", 3f);
        transition.particleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        transition.particles = particleObject.GetComponent<ChronoTunnelParticles>();
        transition.particles.material = transition.particleMaterial;
        transition.SyncParticles();
        return transition;
    }

    private void Update()
    {
        float cover = curtain.GetFloat("_Cover");
        float speed = curtain.GetFloat("_Reveal") > .5f ? 3.8f : Mathf.Lerp(.4f, 3.8f, cover);
        travel += Time.unscaledDeltaTime * speed;
        curtain.SetFloat("_Travel", travel);
        curtain.SetFloat("_Aspect", (float)Screen.width / Screen.height);
        SyncParticles();
    }

    private void SyncParticles()
    {
        particles.SetPhase(curtain.GetFloat("_Cover"), curtain.GetFloat("_Reveal") > .5f, travel, departureUV);
    }

    public IEnumerator Cover()
    {
        yield return Animate(0f, 1f, 1.1f);
        // Hold the fully opaque tunnel before exchanging scenes.
        yield return new WaitForSecondsRealtime(.18f);
    }

    public void Activate(AsyncOperation loading)
    {
        StartCoroutine(RevealAfterLoad(loading));
    }

    private IEnumerator RevealAfterLoad(AsyncOperation loading)
    {
        loading.allowSceneActivation = true;
        while (!loading.isDone)
            yield return null;
        // Keep the curtain closed while the new scene lays out its first frame.
        yield return null;
        yield return new WaitForEndOfFrame();
        curtain.SetFloat("_Reveal", 1f);
        yield return Animate(1f, 0f, .7f);
        Destroy(gameObject);
    }

    private IEnumerator Animate(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            curtain.SetFloat("_Cover", Mathf.SmoothStep(from, to, Mathf.Clamp01(elapsed / duration)));
            SyncParticles();
            yield return null;
        }
        curtain.SetFloat("_Cover", to);
        SyncParticles();
    }

    private void OnDestroy()
    {
        Destroy(curtain);
        Destroy(departure);
        Destroy(particleMaterial);
    }
}
