using UnityEngine;

public class HUDController : MonoBehaviour
{
    public PlayerHealth player;
    public TimeSlowPrototype timeSlow;
    public UnityEngine.UI.Text memoryText;
    public UnityEngine.UI.Image[] memoryIcons;
    public UnityEngine.UI.Image[] healthPips;
    public UnityEngine.UI.Image qFill;
    public UnityEngine.UI.Text qText;
    public UnityEngine.UI.Text eText;

    void Start()
    {
        MemoryFragment.Collected = 0;
        if (player == null) player = FindObjectOfType<PlayerHealth>();
        if (timeSlow == null) timeSlow = FindObjectOfType<TimeSlowPrototype>();
    }

    void Update()
    {
        if (memoryText != null)
            memoryText.text = MemoryFragment.Collected + " / 3";

        if (memoryIcons != null)
        {
            for (int i = 0; i < memoryIcons.Length; i++)
            {
                if (memoryIcons[i] == null) continue;
                memoryIcons[i].color = i < MemoryFragment.Collected
                    ? new Color(0.4f, 0.95f, 1f, 1f)
                    : new Color(0.25f, 0.35f, 0.42f, 0.7f);
            }
        }

        if (healthPips != null && player != null)
        {
            int stage = player.HealthStage;
            for (int i = 0; i < healthPips.Length; i++)
            {
                if (healthPips[i] == null) continue;
                healthPips[i].color = i < stage
                    ? new Color(1f, 0.45f, 0.35f, 1f)
                    : new Color(0.2f, 0.25f, 0.3f, 0.8f);
            }
        }

        if (qFill != null && timeSlow != null)
            qFill.fillAmount = TimeSlowPrototype.IsSlowed
                ? timeSlow.RemainingRatio
                : timeSlow.CooldownRatio;

        if (qText != null && timeSlow != null)
            qText.text = TimeSlowPrototype.IsSlowed ? "Q  TIEMPO ACTIVO" : "Q  TIEMPO";

        if (eText != null)
            eText.text = SkillUnlockState.EchoUnlocked ? "E  ECO" : "E  BLOQUEADO";
    }
}
