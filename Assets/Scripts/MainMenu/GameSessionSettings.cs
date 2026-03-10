// Подключаем Unity API (нужно для ScriptableObject типа QuizDifficultyPreset)
using UnityEngine;

// Статический класс для хранения временных данных между сценами.
// Здесь будем хранить выбранный пресет сложности перед загрузкой GameScene.
public static class GameSessionSettings
{
    // Выбранный пресет сложности (Easy/Medium/Hard/MaxHard).
    // Его установим в меню, а в GameScene прочитаем.
    public static QuizDifficultyPreset SelectedPreset;

    // Имя сцены игры (чтобы не писать строку "GameScene" в 10 местах).
    public const string GameSceneName = "GameScene";

    // Имя сцены меню (пригодится позже, например для кнопки "В меню").
    public const string MenuSceneName = "MenuScene";

    public static QuizDifficultyPreset EasyPreset;
    public static QuizDifficultyPreset MediumPreset;
    public static QuizDifficultyPreset HardPreset;
    public static QuizDifficultyPreset MaxHardPreset;

    public static bool TrySetNextPresetByKey(string key)
    {
        key = key.Trim().ToLowerInvariant();

        QuizDifficultyPreset next = null;

        if (key == "easy") next = EasyPreset;
        else if (key == "medium") next = MediumPreset;
        else if (key == "hard") next = HardPreset;
        else if (key == "maxhard") next = MaxHardPreset;

        if (next == null) return false;

        SelectedPreset = next;
        return true;
    }
}