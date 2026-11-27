using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalSceneTransition : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena siguiente. Debe estar en Build Settings.")]
    public string nextScene = "Nivel-2";

    private bool transitioning;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (transitioning)
            return;

        if (other.GetComponentInParent<PlayerHealth>() == null)
            return;

        transitioning = true;
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(nextScene) &&
            Application.CanStreamedLevelBeLoaded(nextScene))
        {
            SceneManager.LoadScene(nextScene);
            return;
        }

        // Fallback: use the next scene in Build Settings when no explicit name is valid.
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex > 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
            return;
        }

        Debug.LogWarning("No hay una escena siguiente configurada para el portal.");
        transitioning = false;
    }
}
