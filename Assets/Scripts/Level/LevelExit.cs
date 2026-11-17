using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    public GameObject completePanel;

    private bool completed;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        completed = true;
        Time.timeScale = 1f;

        if (completePanel != null)
        {
            completePanel.SetActive(true);
            Cursor.visible = true;
        }

        PauseMenuController pause = FindObjectOfType<PauseMenuController>();
        if (pause != null)
            pause.enabled = false;

        UnityEngine.Debug.Log("Nivel 1 completado");
    }

    void Update()
    {
        if (!completed || completePanel != null)
            return;

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    void OnGUI()
    {
        if (!completed || completePanel != null)
            return;

        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 40;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.95f, 0.9f, 0.75f);

            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.fontSize = 20;
            bodyStyle.alignment = TextAnchor.MiddleCenter;
            bodyStyle.normal.textColor = Color.white;
        }

        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.DrawTexture(
            new Rect(0f, 0f, Screen.width, Screen.height),
            Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(
            new Rect(0f, Screen.height / 2f - 70f, Screen.width, 60f),
            "NIVEL 1 COMPLETADO",
            titleStyle);

        GUI.Label(
            new Rect(0f, Screen.height / 2f + 10f, Screen.width, 35f),
            "La Torre del Olvido · Siguiente: El Corredor de los Ecos",
            bodyStyle);

        GUI.Label(
            new Rect(0f, Screen.height / 2f + 60f, Screen.width, 30f),
            "R: reiniciar   ·   Esc: salir",
            bodyStyle);
    }
}
