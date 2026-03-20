using UnityEngine;

// Этот класс отвечает за переход на СЛЕДУЮЩИЙ этап кампании.
//
// Раньше логика была жёстко пришита к строкам:
// Easy -> Medium -> Hard -> MaxHard
//
// Теперь мы делаем правильно:
// следующий этап ищется по StageOrder.
public static class PresetProgression
{
    public static bool TrySelectNextPreset()
    {
        // Берём текущий выбранный этап.
        QuizDifficultyPreset current = GameSessionSettings.SelectedPreset;

        // Если ничего не выбрано — перейти не можем.
        if (current == null)
            return false;

        // Ищем следующий этап по порядку.
        QuizDifficultyPreset next = GameSessionSettings.GetNextStage(current);

        // Если следующего этапа нет — значит текущий был последним.
        if (next == null)
            return false;

        // На всякий случай проверяем, что следующий этап уже открыт.
        // Обычно после успешного прохождения это будет true,
        // потому что прогресс уже сохранится в FinishQuiz().
        if (!LevelUnlockService.IsUnlocked(next))
            return false;

        // Сохраняем следующий этап как выбранный.
        GameSessionSettings.SelectedPreset = next;

        return true;
    }
}