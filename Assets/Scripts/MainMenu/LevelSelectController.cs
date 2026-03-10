using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [Header("Level Presets")]
    [SerializeField] private QuizDifficultyPreset easyPreset;
    [SerializeField] private QuizDifficultyPreset mediumPreset;
    [SerializeField] private QuizDifficultyPreset hardPreset;
    [SerializeField] private QuizDifficultyPreset maxHardPreset;

    [Header("Level Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button maxHardButton;

    [Header("Behavior")]
    [SerializeField] private bool autoStartGameAfterSelection = false;

    [Header("Optional UI")]
    [SerializeField] private LevelSelectButtonHighlighter buttonHighlighter;
    [SerializeField] private SelectedLevelLabel selectedLevelLabel;

    private void Start()
    {
        RefreshLocks();

        // Если выбранный уровень оказался закрытым, откатываемся на Easy
        if (!LevelUnlockService.IsUnlocked(GameSessionSettings.SelectedPreset))
        {
            GameSessionSettings.SelectedPreset = easyPreset;
        }

        RefreshVisuals();
    }

    private void OnEnable()
    {
        RefreshLocks();

        if (!LevelUnlockService.IsUnlocked(GameSessionSettings.SelectedPreset))
        {
            GameSessionSettings.SelectedPreset = easyPreset;
        }

        RefreshVisuals();
    }

    public void SelectEasy()
    {
        SelectPreset(easyPreset);
    }

    public void SelectMedium()
    {
        SelectPreset(mediumPreset);
    }

    public void SelectHard()
    {
        SelectPreset(hardPreset);
    }

    public void SelectMaxHard()
    {
        SelectPreset(maxHardPreset);
    }

    private void SelectPreset(QuizDifficultyPreset preset)
    {
        if (preset == null)
        {
            Debug.LogError("[LevelSelectController] Tried to select null preset.");
            return;
        }

        if (!LevelUnlockService.IsUnlocked(preset))
        {
            Debug.Log($"[LevelSelectController] Preset locked: {preset.PresetName}");
            return;
        }

        GameSessionSettings.SelectedPreset = preset;
        Debug.Log($"[LevelSelectController] Selected preset: {preset.PresetName}");

        RefreshVisuals();

        if (autoStartGameAfterSelection)
        {
            SceneManager.LoadScene(GameSessionSettings.GameSceneName);
        }
    }

    public void RefreshLocks()
    {
        ApplyLock(easyButton, easyPreset);
        ApplyLock(mediumButton, mediumPreset);
        ApplyLock(hardButton, hardPreset);
        ApplyLock(maxHardButton, maxHardPreset);
    }

    private void ApplyLock(Button button, QuizDifficultyPreset preset)
    {
        if (button == null)
            return;

        button.interactable = LevelUnlockService.IsUnlocked(preset);
    }

    private void RefreshVisuals()
    {
        if (buttonHighlighter != null)
            buttonHighlighter.Refresh();

        if (selectedLevelLabel != null)
            selectedLevelLabel.Refresh();
    }
}