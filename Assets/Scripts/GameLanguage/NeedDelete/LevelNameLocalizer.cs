using UnityEngine;

// Вспомогательный класс для перевода названий уровней (Easy/Medium/Hard/MaxHard)
// в зависимости от выбранного языка.
public static class LevelNameLocalizer
{
    // Вернуть локализованное имя уровня по строке (например, "Easy")
    public static string LocalizePresetName(string presetName, bool useEnglish)
    {
        // Защита от null/пустой строки
        if (string.IsNullOrWhiteSpace(presetName))
            return useEnglish ? "Unknown" : "Неизвестно";

        // Нормализуем строку: обрежем пробелы и приведём к нижнему регистру
        string key = presetName.Trim().ToLowerInvariant();

        // Пытаемся распознать стандартные названия
        switch (key)
        {
            case "easy":
                return useEnglish ? "Easy" : "Лёгкий";

            case "medium":
                return useEnglish ? "Medium" : "Средний";

            case "hard":
                return useEnglish ? "Hard" : "Тяжёлый";

            case "maxhard":
            case "max hard":
            case "max_hard":
                return useEnglish ? "Max Hard" : "Максимальный";

            default:
                // Если имя нестандартное — возвращаем как есть
                // (это полезно, если потом появятся специальные режимы)
                return presetName;
        }
    }
}