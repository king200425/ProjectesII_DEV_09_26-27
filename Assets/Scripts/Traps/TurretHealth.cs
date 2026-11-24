using UnityEngine;

public class TurretHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 39;

    [Header("Visuals")]
    public SpriteRenderer visual;
    public Sprite normalSprite;
    public Sprite lightSprite;
    public SpriteRenderer headVisual;
    public Sprite normalHeadSprite;
    public Sprite lightHeadSprite;
    public TextMesh healthLabel;

    private int health;
    private bool lightPhase;

    public int Health => health;
    public bool LightPhase => lightPhase;

    void Start()
    {
        health = maxHealth;
        ApplyPhase();
    }

    public bool Hit(bool isGhostBullet)
    {
        if (health <= 0)
            return false;

        bool canDamage = lightPhase ? isGhostBullet : !isGhostBullet;
        if (!canDamage)
            return false;

        health--;
        if (health <= 0)
            health = 0;

        if (health <= 26 && health > 13)
            lightPhase = true;
        else if (health <= 13)
            lightPhase = false;

        ApplyPhase();

        if (health <= 0)
        {
            if (visual != null)
                visual.enabled = false;
            Destroy(gameObject, 0.15f);
        }

        return true;
    }

    void ApplyPhase()
    {
        if (visual != null)
            visual.sprite = lightPhase ? lightSprite : normalSprite;

        if (headVisual != null)
            headVisual.sprite = lightPhase ? lightHeadSprite : normalHeadSprite;

        if (healthLabel != null)
        {
            healthLabel.text = health + " / " + maxHealth + (lightPhase ? "  ECHO ONLY" : "");
            healthLabel.color = lightPhase
                ? new Color(1f, 0.72f, 0.72f, 1f)
                : new Color(0.75f, 1f, 1f, 1f);
        }
    }
}
