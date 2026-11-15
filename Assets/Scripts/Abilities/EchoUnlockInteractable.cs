using UnityEngine;

public class EchoUnlockInteractable : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.F;
    public string prompt = "Press F to unlock ECHO";
    public string unlockedMessage = "ECHO UNLOCKED";

    private bool playerInRange;
    private bool unlocked;
    private float messageTimer;
    private SpriteRenderer spriteRenderer;
    private Collider2D trigger;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trigger = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerHealth>() != null)
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerHealth>() != null)
            playerInRange = false;
    }

    void Update()
    {
        if (unlocked)
        {
            if (messageTimer > 0f)
                messageTimer -= Time.unscaledDeltaTime;

            return;
        }

        if (playerInRange && Input.GetKeyDown(interactKey))
            Unlock();
    }

    void Unlock()
    {
        unlocked = true;
        SkillUnlockState.SetEchoUnlocked(true);

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(0.4f, 1f, 0.65f, 1f);

        if (trigger != null)
            trigger.enabled = false;

        messageTimer = 3f;
    }

    void OnGUI()
    {
        if (unlocked)
        {
            if (messageTimer > 0f)
                DrawMessage(unlockedMessage);

            return;
        }

        if (playerInRange)
            DrawMessage(prompt);
    }

    void DrawMessage(string text)
    {
        float width = 420f;
        float height = 46f;
        Rect rect = new Rect(
            (Screen.width - width) * 0.5f,
            Screen.height - 96f,
            width,
            height);

        GUI.color = new Color(0.05f, 0.03f, 0.08f, 0.85f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 18;
        style.normal.textColor = Color.white;
        GUI.Label(rect, text, style);
    }
}
