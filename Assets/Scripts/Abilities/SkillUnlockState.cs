using UnityEngine;

public static class SkillUnlockState
{
    private const string EchoKey = "Skill_Echo_Unlocked";

    public static bool EchoUnlocked
    {
        get { return PlayerPrefs.GetInt(EchoKey, 0) == 1; }
    }

    public static void SetEchoUnlocked(bool unlocked)
    {
        PlayerPrefs.SetInt(EchoKey, unlocked ? 1 : 0);
        PlayerPrefs.Save();
    }
}
