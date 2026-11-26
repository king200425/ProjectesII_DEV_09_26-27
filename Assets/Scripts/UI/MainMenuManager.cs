using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 主菜单的控制入口：按钮、钟表、标题、角色和文字提示。
public class MainMenuManager : MonoBehaviour
{
    [Header("菜单按钮")]
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup mainButtons;

    [Header("钟表")]
    [SerializeField] private RectTransform outerDial;
    [SerializeField] private RectTransform innerDial;
    [SerializeField] private RectTransform sweepHand;
    [SerializeField] private RectTransform hourHand;
    [SerializeField] private RectTransform clockCore;

    [Header("标题、角色和粒子")]
    [SerializeField] private RawImage pinkGhost;
    [SerializeField] private RawImage effects;
    [SerializeField] private Text cyanTitleEcho;
    [SerializeField] private Text pinkTitleEcho;
    [SerializeField] private ChronoMenuParticles timeParticles;

    [Header("主菜单上的文字")]
    [SerializeField] private Text frameCounter;
    [SerializeField] private Text frameDetails;
    [SerializeField] private Text actionStatus;
    [SerializeField] private Text timelineStatus;
    [SerializeField] private Text phaseStatus;

    [Header("开始游戏")]
    [SerializeField] private string destinationScene = "SaveSelect";
    [SerializeField, Min(0.5f)] private float ignitionDuration = 2.2f;

    private Material effectsMaterial;
    private Material ghostMaterial;
    private readonly System.Random glitchTiming = new System.Random(177);
    private Vector2 ghostPosition;
    private Vector2 cyanEchoPosition;
    private Vector2 pinkEchoPosition;
    private float nextGlitch;
    private float glitchEnd;
    private float ignition;
    private float elapsedIgnition;
    private float elapsedFrames;
    private float feedbackUntil;
    private int frames;
    private bool starting;
    private float burstStarted = -1f;

    private void Awake()
    {
        // 每次运行使用自己的材质副本，避免修改项目里的原材质。
        effectsMaterial = new Material(effects.material);
        effects.material = effectsMaterial;
        ghostMaterial = new Material(pinkGhost.material);
        pinkGhost.material = ghostMaterial;

        ghostPosition = pinkGhost.rectTransform.anchoredPosition;
        cyanEchoPosition = cyanTitleEcho.rectTransform.anchoredPosition;
        pinkEchoPosition = pinkTitleEcho.rectTransform.anchoredPosition;
        nextGlitch = Time.unscaledTime + 1.4f;
        startButton.Select();
    }

    private void Update()
    {
        // unscaledTime 不受暂停影响，菜单特效仍能正常播放。
        float time = Time.unscaledTime;
        float delta = Time.unscaledDeltaTime;
        UpdateClock(time, delta);
        UpdateTitle(time);
        UpdateGhost(time);
        UpdateFrameCounter(delta);
        UpdateMenuHint(time);
    }

    private void UpdateClock(float time, float delta)
    {
        float charge = 0f;
        float burst = 0f;
        if (starting)
        {
            charge = Mathf.Clamp01(elapsedIgnition / ignitionDuration);
            elapsedIgnition += delta;
            ignition = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedIgnition / 1.15f));
        }
        if (burstStarted >= 0f)
            burst = Mathf.Clamp01((time - burstStarted) / 0.75f);

        // ignition 从 0 变到 1，钟表就从平时的慢转过渡到高速旋转。
        outerDial.Rotate(0f, 0f, -Mathf.Lerp(9f, 800f, ignition) * delta);
        innerDial.Rotate(0f, 0f, Mathf.Lerp(14.4f, 1300f, ignition) * delta);
        sweepHand.Rotate(0f, 0f, -Mathf.Lerp(45f, 2200f, ignition) * delta);
        hourHand.Rotate(0f, 0f, -Mathf.Lerp(5f, 780f, ignition) * delta);

        float coreSize = Mathf.Lerp(1f, 0.72f, charge);
        coreSize += Mathf.Sin(Mathf.Min(1f, burst * 4f) * Mathf.PI) * 0.45f;
        float pulse = Mathf.Sin(time * (4f + 12f * ignition)) * (0.025f + 0.09f * ignition);
        clockCore.localScale = Vector3.one * coreSize * (1f + pulse);

        // 把加速、蓄能和爆发进度交给画面特效。
        effectsMaterial.SetFloat("_Ignition", ignition);
        effectsMaterial.SetFloat("_Charge", charge);
        effectsMaterial.SetFloat("_Burst", burst);
        timeParticles.SetIgnition(ignition);
    }

    private void UpdateTitle(float time)
    {
        // Sin 产生周期变化，Pow 让拖影只在短时间内变明显。
        float pulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(time * 1.65f)), 12f);
        float offset = Mathf.Round(pulse * 9f + ignition * 3f);
        cyanTitleEcho.rectTransform.anchoredPosition = cyanEchoPosition + new Vector2(offset, 0f);
        pinkTitleEcho.rectTransform.anchoredPosition = pinkEchoPosition - new Vector2(Mathf.Round(offset * 1.35f), 0f);
        cyanTitleEcho.color = new Color(0f, 0.94f, 1f, 0.35f + pulse * 0.24f);
        pinkTitleEcho.color = new Color(0.79f, 0.39f, 1f, 0.36f + pulse * 0.2f);
    }

    private void UpdateGhost(float time)
    {
        if (time >= nextGlitch)
        {
            glitchEnd = time + 0.12f + (float)glitchTiming.NextDouble() * 0.09f;
            nextGlitch = time + 2.5f + (float)glitchTiming.NextDouble() * 2.3f;
        }

        float shift = 0f;
        float strength = 0.025f;
        float opacity = 0.84f;
        if (time < glitchEnd)
        {
            shift = Mathf.Round(Mathf.Sin(time * 113f) * 9f);
            strength = 0.85f;
            opacity = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(time * 91f));
        }
        pinkGhost.rectTransform.anchoredPosition = ghostPosition + new Vector2(shift, 0f);
        ghostMaterial.SetFloat("_Glitch", strength);
        pinkGhost.color = new Color(1f, 1f, 1f, opacity);
    }

    private void UpdateFrameCounter(float delta)
    {
        elapsedFrames += delta;
        frames++;
        if (elapsedFrames < 0.5f)
            return;

        float average = elapsedFrames / frames;
        frameCounter.text = Mathf.RoundToInt(1f / average) + " FPS";
        frameDetails.text = (average * 1000f).ToString("F1") + " MS  //  " + Screen.width + " x " + Screen.height;
        frames = 0;
        elapsedFrames = 0f;
    }

    private void UpdateMenuHint(float time)
    {
        if (starting)
            return;
        if (feedbackUntil > 0f && time >= feedbackUntil)
        {
            actionStatus.text = "SELECT A TIMELINE";
            feedbackUntil = 0f;
        }
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
            startButton.Select();
    }

    public void StartGame()
    {
        if (starting)
            return;
        if (!Application.CanStreamedLevelBeLoaded(destinationScene))
        {
            actionStatus.text = "DESTINATION UNAVAILABLE";
            return;
        }

        starting = true;
        mainButtons.interactable = false;
        mainButtons.blocksRaycasts = false;
        actionStatus.text = "INITIALIZING TEMPORAL RUN...";
        timelineStatus.text = "TIMELINE // WARP ACTIVE";
        phaseStatus.text = "CHRONO CORE // OVERCLOCKED";
        timeParticles.BeginIgnition();
        StartCoroutine(EnterLevel());
    }

    private IEnumerator EnterLevel()
    {
        // Let the first acceleration frame render before Unity starts loading the level.
        yield return null;
        Time.timeScale = 1f;
        AsyncOperation loading = SceneManager.LoadSceneAsync(destinationScene);
        loading.allowSceneActivation = false;
        while (elapsedIgnition < ignitionDuration || loading.progress < 0.9f)
            yield return null;

        burstStarted = Time.unscaledTime;
        timeParticles.BeginBurst();
        phaseStatus.text = "CHRONO CORE // TIMELINE ENTRY";
        yield return ChronoSceneTransition.Play(loading, effectsMaterial, clockCore);
    }

    public void OpenSettings()
    {
        if (starting)
            return;
        actionStatus.text = "SETTINGS // COMING SOON";
        feedbackUntil = Time.unscaledTime + 1.6f;
    }

    public void QuitGame()
    {
        if (!starting)
            Application.Quit();
    }

    private void OnDestroy()
    {
        Destroy(effectsMaterial);
        Destroy(ghostMaterial);
    }
}
