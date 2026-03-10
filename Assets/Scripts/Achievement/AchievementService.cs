using UnityEngine;

public static class AchievementService
{
    private const string Prefix = "ACH_";

    // Проверить, получено ли достижение
    public static bool IsUnlocked(AchievementId id)
    {
        string key = Prefix + id;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // Выдать достижение (сохраняет в PlayerPrefs)
    public static void Unlock(AchievementId id)
    {
        string key = Prefix + id;

        // Если уже было — лишний раз не пишем
        if (PlayerPrefs.GetInt(key, 0) == 1)
            return;

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    // (Опционально) сбросить всё — удобно для теста
    public static void ResetAll()
    {
        foreach (AchievementId id in System.Enum.GetValues(typeof(AchievementId)))
        {
            PlayerPrefs.DeleteKey(Prefix + id);
        }
        PlayerPrefs.Save();
    }
}