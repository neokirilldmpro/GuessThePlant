/*Добавим маленький скрипт SelectedLevelLabel.cs, который:
читает GameSessionSettings.SelectedPreset
показывает текст в TMP_Text
обновляется, когда открываешь меню обратно*/

// Подключаем Unity API
using UnityEngine;

// Подключаем TextMeshPro
using TMPro;

// Скрипт для отображения выбранного уровня в главном меню.
// Пример: "Выбран уровень: Средний"
public class SelectedLevelLabel : MonoBehaviour
{
    [Header("UI")] // Заголовок в инспекторе
    [SerializeField] private TMP_Text label; // Ссылка на TMP текст, куда будем писать строку

    [Header("Fallback")] // Заголовок в инспекторе
    [SerializeField] private string noSelectionTextRu = "Выбран уровень: не выбран"; // Текст, если уровень ещё не выбран
    [SerializeField] private string noSelectionTextEn = "Selected level: not selected"; // EN-вариант (на будущее)

    [Header("Language")] // Заголовок в инспекторе
    [SerializeField] private bool useEnglish = false; // Язык подписи (пока можно оставить false)

    private void Start() // Unity вызывает при старте сцены
    {
        Refresh(); // Обновляем текст при запуске
    }

    private void OnEnable() // Unity вызывает при активации объекта
    {
        Refresh(); // Обновляем текст, когда панель/объект снова включается
    }

    // Публичный метод обновления текста (можно вызывать из MainMenuController)

    public void Refresh()
    {
        if (label == null)
        {
            Debug.LogWarning("[SelectedLevelLabel] Label TMP_Text is not assigned.");
            return;
        }

        QuizDifficultyPreset selected = GameSessionSettings.SelectedPreset;
        bool useEnglish = LanguageService.UseEnglish; // Берём глобальный язык

        if (selected == null)
        {
            label.text = useEnglish ? "Selected level: not selected" : "Выбран уровень: не выбран";
            return;
        }

        // Локализуем название уровня (Easy -> Лёгкий и т.д.)
        string localizedLevelName = LevelNameLocalizer.LocalizePresetName(selected.PresetName, useEnglish);

        // Собираем итоговую строку
        label.text = useEnglish
            ? $"Selected level: {localizedLevelName}"
            : $"Выбран уровень: {localizedLevelName}";
    }
}