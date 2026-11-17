using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";
    private const string FullscreenKey = "Fullscreen";

    [SerializeField] private GameObject mainButtons;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Text volumeLabel;
    [SerializeField] private Toggle fullscreen;
    [SerializeField] private CanvasGroup leftCrystal;
    [SerializeField] private CanvasGroup rightCrystal;

    private void Start()
    {
        if (masterVolume != null && volumeLabel != null)
        {
            float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
            AudioListener.volume = volume;
            masterVolume.SetValueWithoutNotify(volume);
            UpdateVolumeLabel(volume);
        }

        if (fullscreen != null)
        {
            if (PlayerPrefs.HasKey(FullscreenKey))
                Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) != 0;
            fullscreen.SetIsOnWithoutNotify(Screen.fullScreen);
        }

        startButton?.Select();
    }

    private void Update()
    {
        float time = Time.unscaledTime;
        if (leftCrystal != null)
            leftCrystal.alpha = Pulse(time, 4f, .55f, .85f);
        if (rightCrystal != null)
            rightCrystal.alpha = Pulse(time, 3.5f, .60f, .90f);

        if (Input.GetKeyDown(KeyCode.Escape) && settingsPanel != null && settingsPanel.activeSelf)
            CloseSettings();

        EventSystem events = EventSystem.current;
        if (events != null && events.currentSelectedGameObject == null)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
                masterVolume.Select();
            else
                startButton?.Select();
        }
    }

    private static float Pulse(float time, float period, float low, float high)
    {
        return Mathf.Lerp(low, high, .5f + .5f * Mathf.Cos(time * (2f * Mathf.PI / period)));
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TrapTesting");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        mainButtons.SetActive(false);
        masterVolume.Select();
    }

    public void CloseSettings()
    {
        mainButtons.SetActive(true);
        settingsPanel.SetActive(false);
        settingsButton.Select();
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        UpdateVolumeLabel(volume);
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    private void UpdateVolumeLabel(float volume)
    {
        volumeLabel.text = Mathf.RoundToInt(volume * 100f) + "%";
    }

    public void SetFullscreen(bool enabled)
    {
        Screen.fullScreen = enabled;
        PlayerPrefs.SetInt(FullscreenKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
