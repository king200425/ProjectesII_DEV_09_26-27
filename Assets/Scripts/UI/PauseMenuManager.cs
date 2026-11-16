using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;

    private bool isPaused;
    private float previousTimeScale;

    private void Awake()
    {
        if (pauseMenu == null || transform.IsChildOf(pauseMenu.transform))
        {
            Debug.LogError("Assign a pause panel outside the PauseManager hierarchy.", this);
            enabled = false;
            return;
        }

        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void OnDisable()
    {
        ResumeGame();
    }

    public void PauseGame()
    {
        if (!isActiveAndEnabled || isPaused || pauseMenu == null)
            return;

        pauseMenu.SetActive(true);
        if (!pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(false);
            return;
        }

        previousTimeScale = Time.timeScale;
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = previousTimeScale;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void OpenSettings()
    {
        Debug.Log("Settings menu will be added next.");
    }

    public void QuitGame()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }
}
