using UnityEngine;

// Этот класс отвечает за проверку,
// открыт ли этап для игрока.
//
// Раньше здесь была логика на 4 старых сложности.
// Теперь мы просто опираемся на новую кампанийную систему:
// этап открыт, если:
//
// 1) это первый этап,
// или
// 2) пройден предыдущий этап.
public static class LevelUnlockService
{
    // Проверить, открыт ли этап.
    public static bool IsUnlocked(QuizDifficultyPreset preset)
    {
        // Защита от null
        if (preset == null)
            return false;

        // Вся реальная логика уже лежит в StageProgressService.
        return StageProgressService.IsUnlocked(preset);
    }

    // Проверить, пройден ли этап.
    public static bool IsCompleted(QuizDifficultyPreset preset)
    {
        if (preset == null)
            return false;

        return StageProgressService.IsCompleted(preset);
    }

    // Проверить, пройден ли этап идеально.
    public static bool IsPerfectCompleted(QuizDifficultyPreset preset)
    {
        if (preset == null)
            return false;

        return StageProgressService.IsPerfectCompleted(preset);
    }

    // Удобный метод для будущих tooltip:
    // вернуть текст причины, почему этап закрыт.
    public static string GetLockedReason(QuizDifficultyPreset preset, bool useEnglish)
    {
        if (preset == null)
            return useEnglish ? "Stage is not assigned." : "Этап не назначен.";

        // Если этап уже открыт — причина не нужна.
        if (IsUnlocked(preset))
            return string.Empty;

        // Если это почему-то первый этап, но он закрыт —
        // значит что-то не так в данных.
        if (preset.IsFirstStage())
            return useEnglish ? "This stage should be available from start." : "Этот этап должен быть открыт с самого начала.";

        // Берём предыдущий этап.
        QuizDifficultyPreset previous = preset.PreviousStage;

        if (previous == null)
            return useEnglish ? "Previous stage is not assigned." : "Не назначен предыдущий этап.";

        // Локализуем его красивое имя.
        string previousName = previous.GetDisplayName(useEnglish);

        // Возвращаем понятный текст.
        return useEnglish
            ? $"Complete previous stage: {previousName}"
            : $"Для доступа нужно пройти предыдущий этап: {previousName}";
    }
}