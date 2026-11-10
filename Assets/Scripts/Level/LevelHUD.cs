using UnityEngine;

public class LevelHUD : MonoBehaviour
{
    private GUIStyle style;

    void Start()
    {
        MemoryFragment.Collected = 0;
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
            style.normal.textColor = new Color(0.95f, 0.9f, 0.75f);
        }

        GUI.Label(
            new Rect(20f, 18f, 320f, 28f),
            "Memorias: " + MemoryFragment.Collected + "/3",
            style);

        if (!TimeSlowPrototype.IsSlowed)
            return;

        style.normal.textColor = new Color(0.7f, 0.9f, 1f);
        GUI.Label(new Rect(20f, 46f, 420f, 28f), "Q — tiempo ralentizado", style);
    }
}
