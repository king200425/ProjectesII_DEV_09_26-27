using UnityEngine;

public class TutorialPrompt : MonoBehaviour
{
    public string message = "";
    public KeyCode key = KeyCode.None;
    public float autoHide = 5f;

    private bool visible;
    private float hideAt;
    private GUIStyle style;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null)
            return;

        visible = true;
        hideAt = Time.unscaledTime + autoHide;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
            visible = false;
    }

    void Update()
    {
        if (!visible)
            return;

        if (key != KeyCode.None && Input.GetKeyDown(key))
            visible = false;

        if (Time.unscaledTime >= hideAt)
            visible = false;
    }

    void OnGUI()
    {
        if (!visible)
            return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 22;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            style.wordWrap = true;
        }

        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(
            new Rect(Screen.width / 2f - 300f, Screen.height - 120f, 600f, 58f),
            Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(
            new Rect(Screen.width / 2f - 290f, Screen.height - 118f, 580f, 54f),
            message,
            style);
    }
}
