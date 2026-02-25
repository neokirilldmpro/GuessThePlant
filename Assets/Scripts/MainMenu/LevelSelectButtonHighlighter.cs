/*Этот скрипт:
знает 4 кнопки уровней
смотрит, какой Preset выбран сейчас
красит выбранную кнопку в цвет selectedColor
остальные — в normalColor*/
// Подключаем Unity API
using UnityEngine;

// Подключаем UI (Button, Image)
using UnityEngine.UI;

// Скрипт подсветки выбранного уровня на панели выбора.
// Вешается на объект LevelSelectPanel (или отдельный объект в этой панели).
public class LevelSelectButtonHighlighter : MonoBehaviour
{
    [Header("Buttons")] // Заголовок в инспекторе
    [SerializeField] private Button easyButton; // Кнопка Easy
    [SerializeField] private Button mediumButton; // Кнопка Medium
    [SerializeField] private Button hardButton; // Кнопка Hard
    [SerializeField] private Button maxHardButton; // Кнопка MaxHard

    [Header("Presets")] // Заголовок в инспекторе
    [SerializeField] private QuizDifficultyPreset easyPreset; // Пресет Easy
    [SerializeField] private QuizDifficultyPreset mediumPreset; // Пресет Medium
    [SerializeField] private QuizDifficultyPreset hardPreset; // Пресет Hard
    [SerializeField] private QuizDifficultyPreset maxHardPreset; // Пресет MaxHard

    [Header("Colors")] // Заголовок в инспекторе
    [SerializeField] private Color normalColor = Color.white; // Цвет обычной кнопки
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f); // Цвет выбранного уровня (золотистый)

    [Header("Optional")] // Заголовок в инспекторе
    [SerializeField] private bool refreshContinuously = false; // Если true — обновлять каждый кадр (обычно не нужно)

    private void OnEnable() // Unity вызывает при активации объекта/панели
    {
        Refresh(); // Обновляем подсветку, когда панель открылась
    }

    private void Start() // Unity вызывает при старте
    {
        Refresh(); // На всякий случай обновляем ещё раз
    }

    private void Update() // Unity вызывает каждый кадр
    {
        // Обычно не нужен, но можно включить для отладки
        if (refreshContinuously)
        {
            Refresh();
        }
    }

    // Публичный метод обновления подсветки (можно вызывать вручную после выбора уровня)
    public void Refresh()
    {
        // Текущий выбранный пресет из статических настроек сессии
        QuizDifficultyPreset selected = GameSessionSettings.SelectedPreset;

        // Красим каждую кнопку: если её пресет выбран — selectedColor, иначе normalColor
        ApplyButtonColor(easyButton, selected == easyPreset);
        ApplyButtonColor(mediumButton, selected == mediumPreset);
        ApplyButtonColor(hardButton, selected == hardPreset);
        ApplyButtonColor(maxHardButton, selected == maxHardPreset);
    }

    // Вспомогательный метод: покрасить кнопку
    private void ApplyButtonColor(Button button, bool isSelected)
    {
        // Если кнопка не назначена — ничего не делаем
        if (button == null)
            return;

        // У Button обычно есть Image на том же объекте (фон кнопки)
        Image img = button.GetComponent<Image>();

        // Если Image нет — выходим
        if (img == null)
            return;

        // Ставим цвет в зависимости от выбранности
        img.color = isSelected ? selectedColor : normalColor;
    }
}