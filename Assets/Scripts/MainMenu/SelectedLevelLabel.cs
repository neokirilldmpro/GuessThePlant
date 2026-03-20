using TMPro;
using UnityEngine;

// Этот скрипт показывает в меню,
// какой этап сейчас выбран.
//
// Раньше он локализовал старые строки типа Easy / Medium / Hard.
// Теперь у каждого этапа есть собственное имя:
// displayNameRu / displayNameEn
//
// Значит здесь больше не нужно угадывать название уровня по PresetName.
// Мы просто берём красивое имя из самого preset.
public class SelectedLevelLabel : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Ссылка на текст, куда будем писать строку.")]
    [SerializeField] private TMP_Text label;

    [Header("Texts")]
    [SerializeField] private string noSelectionTextRu = "Выбран этап: не выбран";
    [SerializeField] private string noSelectionTextEn = "Selected stage: not selected";

    [SerializeField] private string selectedPrefixRu = "Выбран этап: ";
    [SerializeField] private string selectedPrefixEn = "Selected stage: ";
    // Префикс, который будет добавляться перед названием этапа.

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    // Публичный метод, чтобы другие скрипты могли обновить подпись
    // после смены этапа.
    public void Refresh()
    {
        // Если текст не назначен — пишем предупреждение и выходим.
        if (label == null)
        {
            Debug.LogWarning("[SelectedLevelLabel] TMP_Text label is not assigned.");
            return;
        }

        // Смотрим текущий язык.
        bool useEnglish = LanguageService.UseEnglish;

        // Берём текущий выбранный preset.
        QuizDifficultyPreset selected = GameSessionSettings.SelectedPreset;

        // Если ничего не выбрано — показываем fallback-текст.
        if (selected == null)
        {
            label.text = useEnglish ? noSelectionTextEn : noSelectionTextRu;
            return;
        }

        // Берём красивое локализованное имя этапа прямо из preset.
        string stageName = selected.GetDisplayName(useEnglish);

        // Собираем финальную строку.
        label.text = useEnglish
            ? selectedPrefixEn + stageName
            : selectedPrefixRu + stageName;
    }
}