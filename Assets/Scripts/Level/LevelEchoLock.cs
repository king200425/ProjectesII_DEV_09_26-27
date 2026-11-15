using UnityEngine;

public class LevelEchoLock : MonoBehaviour
{
    void Awake()
    {
        SkillUnlockState.SetEchoUnlocked(false);
    }
}
