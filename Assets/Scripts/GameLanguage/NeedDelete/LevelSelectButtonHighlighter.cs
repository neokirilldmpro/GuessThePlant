using UnityEngine;
using UnityEngine.UI;

// Этот скрипт отвечает только за внешний вид кнопок выбора уровней.
// Теперь у кнопок будет 3 визуальных состояния:
//
// 1) normalColor   -> уровень доступен, но не выбран
// 2) selectedColor -> уровень доступен и выбран
// 3) lockedColor   -> уровень пока закрыт
public class LevelSelectButtonHighlighter : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button easyButton;      // Кнопка Easy
    [SerializeField] private Button mediumButton;    // Кнопка Medium
    [SerializeField] private Button hardButton;      // Кнопка Hard
    [SerializeField] private Button maxHardButton;   // Кнопка MaxHard

    [Header("Presets")]
    [SerializeField] private QuizDifficultyPreset easyPreset;     // Preset Easy
    [SerializeField] private QuizDifficultyPreset mediumPreset;   // Preset Medium
    [SerializeField] private QuizDifficultyPreset hardPreset;     // Preset Hard
    [SerializeField] private QuizDifficultyPreset maxHardPreset;  // Preset MaxHard

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    // Цвет обычной доступной кнопки

    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f);
    // Цвет выбранного доступного уровня

    [SerializeField] private Color lockedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
    // Цвет закрытого уровня
    // Его можно потом подобрать в инспекторе, если захочешь светлее/темнее

    [Header("Optional")]
    [SerializeField] private bool refreshContinuously = false;
    // Обычно false.
    // Если true, то скрипт будет каждый кадр заново проверять цвета.
    // Это бывает полезно только если что-то часто меняется на лету.

    private void OnEnable()
    {
        // Когда объект включили, сразу обновляем все кнопки
        Refresh();
    }

    private void Start()
    {
        // И ещё раз обновим на старте для надёжности
        Refresh();
    }

    private void Update()
    {
        // Непрерывное обновление обычно не нужно,
        // но оставляем как опцию
        if (refreshContinuously)
        {
            Refresh();
        }
    }

    // Главный публичный метод:
    // его можно вызывать из других скриптов после смены выбора уровня
    public void Refresh()
    {
        // Узнаём, какой preset сейчас выбран в GameSessionSettings
        QuizDifficultyPreset selected = GameSessionSettings.SelectedPreset;

        // Для каждой кнопки отдельно решаем, какой цвет она должна получить
        ApplyButtonColor(easyButton, easyPreset, selected == easyPreset);
        ApplyButtonColor(mediumButton, mediumPreset, selected == mediumPreset);
        ApplyButtonColor(hardButton, hardPreset, selected == hardPreset);
        ApplyButtonColor(maxHardButton, maxHardPreset, selected == maxHardPreset);
    }

    // Этот метод красит одну конкретную кнопку
    private void ApplyButtonColor(Button button, QuizDifficultyPreset preset, bool isSelected)
    {
        // Если ссылка на кнопку не назначена — ничего не делаем
        if (button == null)
            return;

        // Получаем Image на самой кнопке
        Image img = button.GetComponent<Image>();

        // Если Image не найден — красить нечего
        if (img == null)
            return;

        // Проверяем, открыт ли этот уровень
        // Здесь используется твоя уже добавленная логика открытия уровней
        bool unlocked = LevelUnlockService.IsUnlocked(preset);

        // Если уровень закрыт — всегда серый цвет
        if (!unlocked)
        {
            img.color = lockedColor;
            return;
        }

        // Если уровень открыт и выбран — цвет selected
        if (isSelected)
        {
            img.color = selectedColor;
            return;
        }

        // Если уровень открыт, но не выбран — обычный цвет
        img.color = normalColor;
    }
}