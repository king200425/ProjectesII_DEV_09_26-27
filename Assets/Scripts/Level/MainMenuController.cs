using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public UnityEngine.GameObject creditsPanel;

    void Start()
    {
        Wire("PlayButton", Play);
        Wire("CreditsButton", ToggleCredits);
        Wire("QuitButton", Quit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Play();
    }

    private void Wire(string name, UnityEngine.Events.UnityAction action)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) return;
        Button button = go.GetComponent<Button>();
        if (button != null) button.onClick.AddListener(action);
    }

    public void ToggleCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(!creditsPanel.activeSelf);
    }

    public void Play()
    {
        SceneManager.LoadScene("Level01_TorreDelOlvido");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
