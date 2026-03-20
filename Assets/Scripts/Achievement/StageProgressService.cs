using UnityEngine;

// Этот статический класс отвечает за прогресс этапов кампании.
// Он умеет хранить:
//
// - пройден ли этап
// - пройден ли этап идеально
// - лучший счёт
// - лучшее время
// - количество попыток
//
// Всё хранится в PlayerPrefs.
// Для браузерной игры и текущего этапа разработки этого достаточно.
public static class StageProgressService
{
    // -------------------------
    // КЛЮЧИ
    // -------------------------

    // Эти префиксы нужны, чтобы ключи в PlayerPrefs были аккуратными
    // и не путались между собой.
    private const string CompletedPrefix = "STAGE_COMPLETED_";
    private const string PerfectPrefix = "STAGE_PERFECT_";
    private const string BestScorePrefix = "STAGE_BEST_SCORE_";
    private const string BestTimePrefix = "STAGE_BEST_TIME_";
    private const string AttemptsPrefix = "STAGE_ATTEMPTS_";

    // -------------------------
    // ПУБЛИЧНЫЕ МЕТОДЫ ЧТЕНИЯ
    // -------------------------

    // Проверка: пройден ли этап
    public static bool IsCompleted(QuizDifficultyPreset preset)
    {
        if (!IsValidPreset(preset))
            return false;

        return PlayerPrefs.GetInt(GetCompletedKey(preset), 0) == 1;
    }

    // Проверка: пройден ли этап идеально
    public static bool IsPerfectCompleted(QuizDifficultyPreset preset)
    {
        if (!IsValidPreset(preset))
            return false;

        return PlayerPrefs.GetInt(GetPerfectKey(preset), 0) == 1;
    }

    // Лучший счёт этапа
    public static int GetBestScore(QuizDifficultyPreset preset)
    {
        if (!IsValidPreset(preset))
            return 0;

        return PlayerPrefs.GetInt(GetBestScoreKey(preset), 0);
    }

    // Лучшее время этапа.
    // Чем меньше — тем лучше.
    // Если времени ещё нет, возвращаем 0.
    public static float GetBestTime(QuizDifficultyPreset preset)
    {
        if (!IsValidPreset(preset))
            return 0f;

        return PlayerPrefs.GetFloat(GetBestTimeKey(preset), 0f);
    }

    // Количество попыток на этом этапе
    public static int GetAttempts(QuizDifficultyPreset preset)
    {
        if (!IsValidPreset(preset))
            return 0;

        return PlayerPrefs.GetInt(GetAttemptsKey(preset), 0);
    }

    // Проверка: открыт ли этап
    public static bool IsUnlocked(QuizDifficultyPreset preset)
    {
        if (!IsValidPreset(preset))
            return false;

        // Если это первый этап — он открыт всегда
        if (preset.IsFirstStage())
            return true;

        // Для остальных этапов:
        // открыт, если пройден предыдущий этап
        QuizDifficultyPreset previous = preset.PreviousStage;

        if (previous == null)
            return false;

        return IsCompleted(previous);
    }

    // -------------------------
    // ПУБЛИЧНЫЕ МЕТОДЫ ЗАПИСИ
    // -------------------------

    // Этот метод вызывается после завершения этапа.
    //
    // score              -> сколько правильных ответов
    // totalQuestions     -> сколько вопросов было всего
    // totalTimeSeconds   -> общее время прохождения этапа
    //
    // ВАЖНО:
    // этот метод НЕ решает игровую логику таймера,
    // а только сохраняет уже готовый результат.
    public static void SaveStageResult(QuizDifficultyPreset preset, int score, int totalQuestions, float totalTimeSeconds)
    {
        if (!IsValidPreset(preset))
            return;

        // 1) Увеличиваем количество попыток
        int attempts = GetAttempts(preset);
        attempts++;
        PlayerPrefs.SetInt(GetAttemptsKey(preset), attempts);

        // 2) Определяем, пройден ли этап
        // Пока логика такая:
        // можно ошибиться максимум 2 раза.
        int allowedMistakes = 2;
        int requiredScore = Mathf.Max(0, totalQuestions - allowedMistakes);
        bool completed = score >= requiredScore;

        // 3) Определяем, perfect ли прохождение
        bool perfect = score == totalQuestions;

        // 4) Если этап пройден, сохраняем completed
        if (completed)
        {
            PlayerPrefs.SetInt(GetCompletedKey(preset), 1);
        }

        // 5) Если этап пройден идеально, сохраняем perfect
        if (perfect)
        {
            PlayerPrefs.SetInt(GetPerfectKey(preset), 1);
        }

        // 6) Обновляем лучший счёт
        int currentBestScore = GetBestScore(preset);
        if (score > currentBestScore)
        {
            PlayerPrefs.SetInt(GetBestScoreKey(preset), score);
        }

        // 7) Обновляем лучшее время
        // Здесь логика такая:
        // меньшее время считается лучшим.
        //
        // Но если таймер в этапе не используется,
        // totalTimeSeconds можно всё равно сохранять как общее время прохождения.
        float currentBestTime = GetBestTime(preset);

        if (currentBestTime <= 0f || totalTimeSeconds < currentBestTime)
        {
            PlayerPrefs.SetFloat(GetBestTimeKey(preset), totalTimeSeconds);
        }

        // 8) Сохраняем всё в PlayerPrefs
        PlayerPrefs.Save();
    }

    // -------------------------
    // СВОДНАЯ СТАТИСТИКА
    // -------------------------

    // Сколько этапов из массива уже пройдено
    public static int GetCompletedCount(QuizDifficultyPreset[] stages)
    {
        if (stages == null)
            return 0;

        int count = 0;

        for (int i = 0; i < stages.Length; i++)
        {
            if (IsCompleted(stages[i]))
                count++;
        }

        return count;
    }

    // Сколько этапов из массива пройдено идеально
    public static int GetPerfectCount(QuizDifficultyPreset[] stages)
    {
        if (stages == null)
            return 0;

        int count = 0;

        for (int i = 0; i < stages.Length; i++)
        {
            if (IsPerfectCompleted(stages[i]))
                count++;
        }

        return count;
    }

    // Все ли этапы пройдены
    public static bool AreAllCompleted(QuizDifficultyPreset[] stages)
    {
        if (stages == null || stages.Length == 0)
            return false;

        for (int i = 0; i < stages.Length; i++)
        {
            if (!IsCompleted(stages[i]))
                return false;
        }

        return true;
    }

    // Все ли этапы пройдены идеально
    public static bool AreAllPerfect(QuizDifficultyPreset[] stages)
    {
        if (stages == null || stages.Length == 0)
            return false;

        for (int i = 0; i < stages.Length; i++)
        {
            if (!IsPerfectCompleted(stages[i]))
                return false;
        }

        return true;
    }

    // -------------------------
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // -------------------------

    private static bool IsValidPreset(QuizDifficultyPreset preset)
    {
        // Проверяем, что preset вообще существует
        if (preset == null)
            return false;

        // Проверяем, что у него есть stageKey
        if (string.IsNullOrWhiteSpace(preset.StageKey))
        {
            Debug.LogError("[StageProgressService] StageKey is empty on preset: " + preset.name);
            return false;
        }

        return true;
    }

    private static string GetCompletedKey(QuizDifficultyPreset preset)
    {
        return CompletedPrefix + preset.StageKey;
    }

    private static string GetPerfectKey(QuizDifficultyPreset preset)
    {
        return PerfectPrefix + preset.StageKey;
    }

    private static string GetBestScoreKey(QuizDifficultyPreset preset)
    {
        return BestScorePrefix + preset.StageKey;
    }

    private static string GetBestTimeKey(QuizDifficultyPreset preset)
    {
        return BestTimePrefix + preset.StageKey;
    }

    private static string GetAttemptsKey(QuizDifficultyPreset preset)
    {
        return AttemptsPrefix + preset.StageKey;
    }
}