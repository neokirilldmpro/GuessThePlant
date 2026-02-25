// Подключаем Unity API
using UnityEngine;

// Подключаем загрузку сцен (если включишь автостарт)
using UnityEngine.SceneManagement;

// Контроллер панели выбора уровня.
// Ставит выбранный QuizDifficultyPreset в GameSessionSettings.
public class LevelSelectController : MonoBehaviour
{
    [Header("Level Presets")] // Заголовок в инспекторе
    [SerializeField] private QuizDifficultyPreset easyPreset; // Пресет Easy
    [SerializeField] private QuizDifficultyPreset mediumPreset; // Пресет Medium
    [SerializeField] private QuizDifficultyPreset hardPreset; // Пресет Hard
    [SerializeField] private QuizDifficultyPreset maxHardPreset; // Пресет MaxHard

    [Header("Behavior")] // Заголовок в инспекторе
    [SerializeField] private bool autoStartGameAfterSelection = false; // Если true — сразу запускаем игру

    [Header("Optional UI")] // Заголовок в инспекторе
    [SerializeField] private LevelSelectButtonHighlighter buttonHighlighter; // Подсветка выбранной кнопки
    [SerializeField] private SelectedLevelLabel selectedLevelLabel; // Надпись "Выбран уровень: ..."

    private void Start() // Unity вызывает при старте
    {
        // Обновляем подсветку при старте
        if (buttonHighlighter != null)
        {
            buttonHighlighter.Refresh();
        }

        // Обновляем текст выбранного уровня при старте
        if (selectedLevelLabel != null)
        {
            selectedLevelLabel.Refresh();
        }
    }

    // Выбор Easy
    public void SelectEasy()
    {
        SelectPreset(easyPreset);
    }

    // Выбор Medium
    public void SelectMedium()
    {
        SelectPreset(mediumPreset);
    }

    // Выбор Hard
    public void SelectHard()
    {
        SelectPreset(hardPreset);
    }

    // Выбор MaxHard
    public void SelectMaxHard()
    {
        SelectPreset(maxHardPreset);
    }

    // Общий метод выбора пресета
    private void SelectPreset(QuizDifficultyPreset preset)
    {
        // Проверка: пресет должен быть назначен
        if (preset == null)
        {
            Debug.LogError("[LevelSelectController] Tried to select null preset.");
            return;
        }

        // Сохраняем выбранный пресет в статические настройки сессии
        GameSessionSettings.SelectedPreset = preset;

        // Лог в консоль для проверки
        Debug.Log($"[LevelSelectController] Selected preset: {preset.PresetName}");

        // Обновляем подсветку выбранной кнопки
        if (buttonHighlighter != null)
        {
            buttonHighlighter.Refresh();
        }

        // Обновляем надпись с выбранным уровнем
        if (selectedLevelLabel != null)
        {
            selectedLevelLabel.Refresh();
        }

        // Если включён автостарт — сразу грузим GameScene
        if (autoStartGameAfterSelection)
        {
            SceneManager.LoadScene(GameSessionSettings.GameSceneName);
        }
    }
}