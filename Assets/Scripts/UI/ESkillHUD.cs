
using UnityEngine;
using TMPro;

public class ESkillHUD : MonoBehaviour
{
    public PlayerRecorder playerRecorder;
    public UnityEngine.UI.Image skillImage;
    public TextMeshProUGUI countdownText;

    [Header("Colors")]
    public Color recordingColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color readyColor = new Color(1f, 0.35f, 0.35f, 1f);
    public Color replayColor = new Color(0.35f, 1f, 0.5f, 1f);

    private Color originalColor;

    void Start()
    {
        if (skillImage != null)
            originalColor = skillImage.color;
    }

    void Update()
    {
        if (playerRecorder == null ||
            skillImage == null ||
            countdownText == null)
            return;

        countdownText.gameObject.SetActive(false);

        if (playerRecorder.IsRecording)
        {
            // Recording: gray-white
            skillImage.color = recordingColor;
            countdownText.gameObject.SetActive(true);

            float remaining = playerRecorder.RecordingRemaining;

            if (remaining > 1f)
                countdownText.text = Mathf.CeilToInt(remaining).ToString();
            else
                countdownText.text = remaining.ToString("F2");
        }
        else if (playerRecorder.IsReplaying)
        {
            // Replaying: green
            skillImage.color = replayColor;
        }
        else if (playerRecorder.HasRecording)
        {
            // Recording completed: red
            skillImage.color = readyColor;
        }
        else
        {
            // Default: original purple
            skillImage.color = originalColor;
        }
    }
}
