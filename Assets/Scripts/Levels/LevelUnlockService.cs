using UnityEngine;

public static class LevelUnlockService
{
    public static bool IsUnlocked(QuizDifficultyPreset preset)
    {
        if (preset == null)
            return false;

        string key = preset.PresetName.Trim().ToLowerInvariant();

        // Первый уровень всегда открыт
        if (key == "easy")
            return true;

        // Остальные открываются после прохождения предыдущего
        if (key == "medium")
            return IsCompleted(GameSessionSettings.EasyPreset);

        if (key == "hard")
            return IsCompleted(GameSessionSettings.MediumPreset);

        if (key == "maxhard" || key == "max hard" || key == "max_hard")
            return IsCompleted(GameSessionSettings.HardPreset);

        return false;
    }

    private static bool IsCompleted(QuizDifficultyPreset preset)
    {
        if (preset == null)
            return false;

        return SimpleSaveService.LoadCompleted(preset.PresetName);
    }
}