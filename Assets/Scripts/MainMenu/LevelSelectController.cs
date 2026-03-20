using UnityEngine;
using UnityEngine.SceneManagement;

// Этот контроллер управляет экраном выбора этапов кампании.
//
// Раньше он работал с 4 кнопками сложности.
// Теперь он работает с массивом карточек этапов.
//
// Он отвечает за:
// 1) раздать этапы карточкам,
// 2) выбрать первый доступный этап, если ничего не выбрано,
// 3) обновить все карточки,
// 4) обновить подпись выбранного этапа,
// 5) обновить верхнюю сводку прогресса.
public class LevelSelectController : MonoBehaviour
{
    [Header("Stage cards")]
    [SerializeField] private CampaignStageButtonUI[] stageButtons;
    // Сюда нужно протянуть 8 карточек этапов.
    //
    // ВАЖНО:
    // количество карточек должно совпадать с количеством этапов кампании.

    [Header("Optional UI")]
    [SerializeField] private SelectedLevelLabel selectedLevelLabel;
    // Подпись "Выбран этап: ..."

    [SerializeField] private CampaignProgressSummaryUI progressSummaryUI;
    // Верхняя сводка:
    // пройдено / perfect / нашивки

    [Header("Optional")]
    [SerializeField] private bool autoStartGameAfterSelection = false;
    // Если true — после нажатия на этап сразу грузится игра.
    // Если false — этап просто выбирается,
    // а старт игры будет отдельной кнопкой "Играть".

    private void Start()
    {
        BuildStageButtons();
        ValidateSelectedPreset();
        RefreshAll();
    }

    private void OnEnable()
    {
        BuildStageButtons();
        ValidateSelectedPreset();
        RefreshAll();
    }

    // -------------------------
    // РАЗДАЧА ПРЕСЕТОВ КАРТОЧКАМ
    // -------------------------

    private void BuildStageButtons()
    {
        // Берём все этапы кампании из GameSessionSettings.
        QuizDifficultyPreset[] stages = GameSessionSettings.GetAllCampaignStages();

        if (stages == null || stages.Length == 0)
        {
            Debug.LogWarning("[LevelSelectController] No campaign stages registered in GameSessionSettings.");
            return;
        }

        if (stageButtons == null || stageButtons.Length == 0)
        {
            Debug.LogWarning("[LevelSelectController] stageButtons array is empty.");
            return;
        }

        // Раздаём этапы по карточкам по индексам.
        // Например:
        // stages[0] -> stageButtons[0]
        // stages[1] -> stageButtons[1]
        int count = Mathf.Min(stages.Length, stageButtons.Length);

        for (int i = 0; i < count; i++)
        {
            if (stageButtons[i] == null)
                continue;

            stageButtons[i].Setup(this, stages[i]);
        }

        // Если карточек больше, чем этапов —
        // оставшиеся карточки чистим.
        for (int i = count; i < stageButtons.Length; i++)
        {
            if (stageButtons[i] == null)
                continue;

            stageButtons[i].Setup(this, null);
        }
    }

    // -------------------------
    // ВЫБОР ЭТАПА
    // -------------------------

    public void SelectPreset(QuizDifficultyPreset preset)
    {
        // Защита от null.
        if (preset == null)
        {
            Debug.LogWarning("[LevelSelectController] Tried to select null preset.");
            return;
        }

        // Если этап закрыт — просто ничего не выбираем.
        // Tooltip и так объяснит причину.
        if (!LevelUnlockService.IsUnlocked(preset))
        {
            Debug.Log($"[LevelSelectController] Stage is locked: {preset.StageKey}");
            return;
        }

        // Сохраняем выбор.
        GameSessionSettings.SelectedPreset = preset;

        Debug.Log($"[LevelSelectController] Selected stage: {preset.StageKey}");

        // Обновляем все карточки и подписи.
        RefreshAll();

        // Если включён автостарт — сразу грузим игру.
        if (autoStartGameAfterSelection)
        {
            SceneManager.LoadScene(GameSessionSettings.GameSceneName);
        }
    }

    // -------------------------
    // ПРОВЕРКА ТЕКУЩЕГО ВЫБОРА
    // -------------------------

    private void ValidateSelectedPreset()
    {
        QuizDifficultyPreset selected = GameSessionSettings.SelectedPreset;

        // Если ничего не выбрано —
        // выбираем первый доступный этап.
        if (selected == null)
        {
            GameSessionSettings.SelectedPreset = GameSessionSettings.GetFirstUnlockedStage();
            return;
        }

        // Если выбранный preset больше не зарегистрирован —
        // тоже переходим на первый доступный.
        if (!GameSessionSettings.ContainsPreset(selected))
        {
            GameSessionSettings.SelectedPreset = GameSessionSettings.GetFirstUnlockedStage();
            return;
        }

        // Если выбранный этап почему-то закрыт —
        // тоже переходим на первый доступный.
        if (!LevelUnlockService.IsUnlocked(selected))
        {
            GameSessionSettings.SelectedPreset = GameSessionSettings.GetFirstUnlockedStage();
        }
    }

    // -------------------------
    // ОБНОВЛЕНИЕ ВСЕГО UI
    // -------------------------

    public void RefreshAll()
    {
        // Обновляем каждую карточку этапа.
        if (stageButtons != null)
        {
            for (int i = 0; i < stageButtons.Length; i++)
            {
                if (stageButtons[i] != null)
                    stageButtons[i].Refresh();
            }
        }

        // Обновляем подпись выбранного этапа.
        if (selectedLevelLabel != null)
            selectedLevelLabel.Refresh();

        // Обновляем верхнюю сводку прогресса.
        if (progressSummaryUI != null)
            progressSummaryUI.Refresh();
    }
}