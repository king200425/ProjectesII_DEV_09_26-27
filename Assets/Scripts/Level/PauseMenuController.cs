using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public GameObject panel;
    private bool paused;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (panel == null) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        paused = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        paused = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
