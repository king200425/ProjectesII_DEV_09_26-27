using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 两个菜单共用这一份转场：等关卡就绪、盖住旧画面、换场景、显示新画面。
public sealed class ChronoSceneTransition : MonoBehaviour
{
    private const float EnterDuration = 1.1f;
    private const float ExitDuration = 0.7f;
    private Material curtain;
    private Material particleMaterial;
    private Texture2D departure;
    private ChronoTunnelParticles particles;
    private Vector2 departureUV;
    private float coverage;
    private float travel;
    private bool revealing;

    public static IEnumerator Play(AsyncOperation loading, Material source, RectTransform startPoint)
    {
        // Unity 加载到 90% 后会等待，直到我们允许它显示新场景。
        while (loading.progress < 0.9f)
            yield return null;
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        ChronoSceneTransition transition = Create(source, screenshot, startPoint);
        yield return transition.AnimateCoverage(0f, 1f, EnterDuration);
        // Hold the fully opaque tunnel before exchanging scenes.
        yield return new WaitForSecondsRealtime(0.18f);

        // 后半段由转场自己播放，原菜单销毁后也不会中断。
        transition.StartCoroutine(transition.RevealAfterLoad(loading));
    }

    private static ChronoSceneTransition Create(Material source, Texture2D screenshot, RectTransform startPoint)
    {
        GameObject root = new GameObject("Chrono Scene Transition", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        DontDestroyOnLoad(root);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;

        ChronoSceneTransition transition = root.AddComponent<ChronoSceneTransition>();
        transition.departure = screenshot;
        transition.curtain = new Material(source);
        transition.curtain.SetFloat("_Mode", 2f);

        // 直接取 UI 中心，钟表和存档卡片都可以作为转场起点。
        Vector3 center = startPoint.TransformPoint(startPoint.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, center);
        transition.departureUV = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
        transition.curtain.SetVector("_ClockUV", new Vector4(transition.departureUV.x, transition.departureUV.y, 0f, 0f));

        GameObject imageObject = CreateFullScreenLayer(root.transform, "Time Tunnel");
        RawImage image = imageObject.AddComponent<RawImage>();
        image.texture = screenshot;
        image.material = transition.curtain;

        GameObject particleObject = CreateFullScreenLayer(root.transform, "Temporal Particles");
        transition.particles = particleObject.AddComponent<ChronoTunnelParticles>();
        transition.particleMaterial = new Material(source);
        transition.particleMaterial.SetFloat("_Mode", 3f);
        transition.particleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        transition.particles.material = transition.particleMaterial;
        transition.UpdateEffects();
        return transition;
    }

    private static GameObject CreateFullScreenLayer(Transform parent, string name)
    {
        GameObject layer = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rect = layer.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return layer;
    }

    private void Update()
    {
        float speed = 3.8f;
        if (!revealing)
            speed = Mathf.Lerp(0.4f, 3.8f, coverage);
        travel += Time.unscaledDeltaTime * speed;
        UpdateEffects();
    }

    private void UpdateEffects()
    {
        // coverage：画面覆盖程度；travel：光轨前进距离；revealing：是否正在显示新场景。
        curtain.SetFloat("_Cover", coverage);
        curtain.SetFloat("_Reveal", revealing ? 1f : 0f);
        curtain.SetFloat("_Travel", travel);
        curtain.SetFloat("_Aspect", (float)Screen.width / Screen.height);
        particles.SetPhase(coverage, revealing, travel, departureUV);
    }

    private IEnumerator RevealAfterLoad(AsyncOperation loading)
    {
        revealing = true;
        loading.allowSceneActivation = true;
        while (!loading.isDone)
            yield return null;
        // Keep the curtain closed while the new scene lays out its first frame.
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return AnimateCoverage(1f, 0f, ExitDuration);
        Destroy(gameObject);
    }

    private IEnumerator AnimateCoverage(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            coverage = Mathf.SmoothStep(from, to, Mathf.Clamp01(elapsed / duration));
            UpdateEffects();
            yield return null;
        }
        coverage = to;
        UpdateEffects();
    }

    private void OnDestroy()
    {
        // 运行时创建的材质和截图需要一起释放。
        Destroy(curtain);
        Destroy(particleMaterial);
        Destroy(departure);
    }
}
