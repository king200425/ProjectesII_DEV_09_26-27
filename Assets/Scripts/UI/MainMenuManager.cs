using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TrapTesting");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}