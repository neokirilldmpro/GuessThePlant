using UnityEngine;

// Логика переключения на следующий уровень по выбранному пресету.
// Работает на основе PresetName: Easy -> Medium -> Hard -> MaxHard
public static class PresetProgression
{
    public static bool TrySelectNextPreset()
    {
        QuizDifficultyPreset current = GameSessionSettings.SelectedPreset;

        // Если текущего нет — не можем перейти
        if (current == null) return false;

        string key = current.PresetName.Trim().ToLowerInvariant();

        // Ищем следующий по имени
        string nextKey = null;

        if (key == "easy") nextKey = "medium";
        else if (key == "medium") nextKey = "hard";
        else if (key == "hard") nextKey = "maxhard";
        else return false; // maxhard или неизвестное — дальше нет

        // Ищем в проекте preset с таким именем ассета/PreserName
        // ВАЖНО: это runtime, поэтому AssetDatabase тут нельзя.
        // Поэтому мы используем заранее заданные ссылки через GameSessionSettings.
        return GameSessionSettings.TrySetNextPresetByKey(nextKey);
    }
}