using UnityEngine;

// Этот компонент живёт в меню и один раз регистрирует
// все этапы кампании в GameSessionSettings.
//
// Раньше здесь было 4 отдельных поля:
// easy / medium / hard / maxHard.
//
// Теперь у нас 8 этапов,
// поэтому правильнее хранить их в массиве.
public class PresetRegistry : MonoBehaviour
{
    [Header("Campaign stages")]
    [Tooltip("Сюда в инспекторе нужно будет протянуть ВСЕ 8 этапов кампании." +
             "Порядок в массиве не критичен,потому что RegisterCampaignPresets сам отсортирует их по StageOrder." +
             "Но для твоего удобства лучше сразу класть их по порядку:1, 2, 3, 4, 5, 6, 7, 8")]
    [SerializeField] private QuizDifficultyPreset[] campaignStages;

    // === НОВОЕ ПОЛЕ ===
    [Header("Online Mode")]
    [SerializeField] private QuizDifficultyPreset onlinePreset;


    private void Awake()
    {
        // Регистрируем все этапы в GameSessionSettings.
        GameSessionSettings.RegisterCampaignPresets(campaignStages);

        // === ПЕРЕДАЕМ НАШ ОНЛАЙН ПРЕСЕТ В ГЛОБАЛЬНЫЕ НАСТРОЙКИ ===
        GameSessionSettings.OnlinePreset = onlinePreset;
    }
}