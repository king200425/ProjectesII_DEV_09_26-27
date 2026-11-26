using UnityEngine;

// Test scene only: the turret scene is not connected to the level's echo altar.
public class TurretTestEchoUnlock : MonoBehaviour
{
    void Awake()
    {
        SkillUnlockState.SetEchoUnlocked(true);
    }
}
