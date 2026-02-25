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
}