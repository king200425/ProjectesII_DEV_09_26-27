using UnityEngine;

public class BlockoutDebugHUD : MonoBehaviour
{
    private GUIStyle style;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (FindObjectOfType<BlockoutDebugHUD>() != null) return;
        var go = new GameObject("BlockoutDebugHUD");
        DontDestroyOnLoad(go);
        go.AddComponent<BlockoutDebugHUD>();
    }

    private void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 22;
            style.normal.textColor = Color.white;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        string status = player != null
            ? $"Player y={player.transform.position.y:F2}"
            : "Player not found";

        GUI.Box(new Rect(10, 10, 520, 96), GUIContent.none);
        GUI.Label(new Rect(20, 16, 500, 30), "A/D or Left/Right: move", style);
        GUI.Label(new Rect(20, 44, 500, 30), "Space / W / Up: jump", style);
        GUI.Label(new Rect(20, 72, 500, 30), status, style);
    }
}
