using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup mainButtons;
    [SerializeField] private RectTransform outerDial;
    [SerializeField] private RectTransform innerDial;
    [SerializeField] private RectTransform sweepHand;
    [SerializeField] private RectTransform hourHand;
    [SerializeField] private RectTransform clockCore;
    [SerializeField] private RawImage pinkGhost;
    [SerializeField] private RawImage effects;
    [SerializeField] private Text frameCounter;
    [SerializeField] private Text frameDetails;
    [SerializeField] private Text actionStatus;
    [SerializeField] private Text timelineStatus;
    [SerializeField] private Text phaseStatus;
    [SerializeField] private Text cyanTitleEcho;
    [SerializeField] private Text pinkTitleEcho;
    [SerializeField] private ChronoMenuParticles timeParticles;
    [SerializeField] private string destinationScene = "Level01_TorreDelOlvido";
    [SerializeField, Min(.5f)] private float ignitionDuration = 2.2f;

    private Material effectsMaterial;
    private Material ghostMaterial;
    private readonly System.Random glitchTiming = new(177);
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
    private AsyncOperation sceneLoad;
    private float burstStarted = -1f;

    private void Awake()
    {
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
        float delta = Time.unscaledDeltaTime;
        float time = Time.unscaledTime;
        float charge = starting ? Mathf.Clamp01(elapsedIgnition / ignitionDuration) : 0f;
        float burst = burstStarted < 0f ? 0f : Mathf.Clamp01((time - burstStarted) / .75f);
        if (starting)
        {
            elapsedIgnition += delta;
            ignition = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsedIgnition / 1.15f));
        }

        outerDial.Rotate(0f, 0f, -Mathf.Lerp(9f, 800f, ignition) * delta);
        innerDial.Rotate(0f, 0f, Mathf.Lerp(14.4f, 1300f, ignition) * delta);
        sweepHand.Rotate(0f, 0f, -Mathf.Lerp(45f, 2200f, ignition) * delta);
        hourHand.Rotate(0f, 0f, -Mathf.Lerp(5f, 780f, ignition) * delta);
        float coreSize = Mathf.Lerp(1f, .72f, charge) + Mathf.Sin(Mathf.Min(1f, burst * 4f) * Mathf.PI) * .45f;
        clockCore.localScale = Vector3.one * coreSize * (1f + Mathf.Sin(time * (4f + 12f * ignition)) * (.025f + .09f * ignition));
        effectsMaterial.SetFloat("_Ignition", ignition);
        effectsMaterial.SetFloat("_Charge", charge);
        effectsMaterial.SetFloat("_Burst", burst);
        timeParticles.SetIgnition(ignition);

        float titlePulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(time * 1.65f)), 12f);
        float titleSplit = Mathf.Round(titlePulse * 9f + ignition * 3f);
        cyanTitleEcho.rectTransform.anchoredPosition = cyanEchoPosition + new Vector2(titleSplit, 0f);
        pinkTitleEcho.rectTransform.anchoredPosition = pinkEchoPosition - new Vector2(Mathf.Round(titleSplit * 1.35f), 0f);
        cyanTitleEcho.color = new Color(0f, .94f, 1f, .35f + titlePulse * .24f);
        pinkTitleEcho.color = new Color(.79f, .39f, 1f, .36f + titlePulse * .2f);

        if (time >= nextGlitch)
        {
            glitchEnd = time + .12f + (float)glitchTiming.NextDouble() * .09f;
            nextGlitch = time + 2.5f + (float)glitchTiming.NextDouble() * 2.3f;
        }
        bool flickering = time < glitchEnd;
        float shift = flickering ? Mathf.Round(Mathf.Sin(time * 113f) * 9f) : 0f;
        pinkGhost.rectTransform.anchoredPosition = ghostPosition + new Vector2(shift, 0f);
        ghostMaterial.SetFloat("_Glitch", flickering ? .85f : .025f);
        pinkGhost.color = new Color(1f, 1f, 1f, flickering ? .4f + .6f * Mathf.Abs(Mathf.Sin(time * 91f)) : .84f);

        elapsedFrames += delta;
        frames++;
        if (elapsedFrames >= .5f)
        {
            float average = elapsedFrames / frames;
            frameCounter.text = Mathf.RoundToInt(1f / average) + " FPS";
            frameDetails.text = (average * 1000f).ToString("F1") + " MS  //  " + Screen.width + " x " + Screen.height;
            frames = 0;
            elapsedFrames = 0f;
        }

        if (!starting && feedbackUntil > 0f && time >= feedbackUntil)
        {
            actionStatus.text = "SELECT A TIMELINE";
            feedbackUntil = 0f;
        }
        if (!starting && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
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
        sceneLoad = SceneManager.LoadSceneAsync(destinationScene);
        sceneLoad.allowSceneActivation = false;
        while (elapsedIgnition < ignitionDuration || sceneLoad.progress < .9f)
            yield return null;
        burstStarted = Time.unscaledTime;
        timeParticles.BeginBurst();
        phaseStatus.text = "CHRONO CORE // TIMELINE ENTRY";
        yield return new WaitForEndOfFrame();
        Texture2D departure = ScreenCapture.CaptureScreenshotAsTexture();
        var transition = ChronoSceneTransition.Create(effectsMaterial, departure, clockCore);
        yield return transition.Cover();
        transition.Activate(sceneLoad);
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
        if (starting)
            return;
        Application.Quit();
    }

    private void OnDestroy()
    {
        Destroy(effectsMaterial);
        Destroy(ghostMaterial);
    }
}
