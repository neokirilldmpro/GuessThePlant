/*переключать язык по кнопке
обновлять текст на кнопке/лейбле*/
using UnityEngine;
using TMPro;

// Контроллер настройки языка в SettingsPanel.
// Позволяет переключать язык и обновляет подписи.
public class SettingsLanguageController : MonoBehaviour
{
    [Header("UI Labels")] // Заголовок в инспекторе
    [SerializeField] private TMP_Text settingsTitleText; // Заголовок "Настройки / Settings"
    [SerializeField] private TMP_Text languageLabelText; // Подпись "Язык / Language"
    [SerializeField] private TMP_Text languageValueText; // Значение "Русский / English"
    [SerializeField] private TMP_Text toggleButtonText;  // Текст кнопки "Сменить / Toggle"
    [SerializeField] private TMP_Text VolumeMusic; // Текст громкости

    [Header("Optional Main Menu Labels")] // Можно обновлять и главное меню сразу
    [SerializeField] private TMP_Text playButtonText;     // Кнопка "Играть / Play"
    [SerializeField] private TMP_Text settingsButtonText; // Кнопка "Настройки / Settings"
    [SerializeField] private TMP_Text levelButtonText;    // Кнопка "Уровень / Level"

    [Header("Optional Level Select Labels")] // Можно обновлять и панель уровней
    [SerializeField] private TMP_Text levelSelectTitleText; // Заголовок панели выбора уровня
    [SerializeField] private TMP_Text easyText;             // Easy кнопка
    [SerializeField] private TMP_Text mediumText;           // Medium кнопка
    [SerializeField] private TMP_Text hardText;             // Hard кнопка
    [SerializeField] private TMP_Text maxHardText;          // MaxHard кнопка
    [SerializeField] private TMP_Text backFromSettingsText; // Назад в настройках
    [SerializeField] private TMP_Text backFromLevelsText;   // Назад в выборе уровня

    [Header("Optional Selected Level Label")] // Если есть твой скрипт надписи выбранного уровня
    [SerializeField] private SelectedLevelLabel selectedLevelLabel;

    [Header("SFX")]
    [SerializeField] private TMP_Text VolumeSFXText;

    [Header("PortfolioPanel")]
    [SerializeField] private TMP_Text BackText;


    private void Start()
    {
        RefreshTexts(); // При старте применяем язык ко всем подписям
    }

    private void OnEnable()
    {
        RefreshTexts(); // Когда панель настроек открывается — обновляем
    }

    // Вызывай этот метод по OnClick кнопки "Сменить язык"
    public void ToggleLanguage()
    {
        LanguageService.ToggleLanguage(); // Переключаем RU/EN и сохраняем
        RefreshTexts(); // Обновляем тексты на экране
    }

    // Обновить все тексты по текущему языку
    public void RefreshTexts()
    {
        bool en = LanguageService.UseEnglish; // Текущий язык

        // ===== Settings panel =====
        if (settingsTitleText != null)
            settingsTitleText.text = en ? "Settings" : "Настройки";

        if (languageLabelText != null)
            languageLabelText.text = en ? "Language:" : "Язык:";

        if (languageValueText != null)
            languageValueText.text = en ? "English" : "Русский";

        if (toggleButtonText != null)
            toggleButtonText.text = en ? "Change" : "Сменить";

        if (VolumeMusic != null)
            VolumeMusic.text = en ? "Music volume:" : "Громкость музыки:";

        if (VolumeSFXText != null)
            VolumeSFXText.text = en ? "SFX:" : "Звуки:";




        // ===== Main menu =====
        if (playButtonText != null)
            playButtonText.text = en ? "Play" : "Играть";

        if (settingsButtonText != null)
            settingsButtonText.text = en ? "Settings" : "Настройки";

        if (levelButtonText != null)
            levelButtonText.text = en ? "Level" : "Уровень";

        // ===== Level select =====
        if (levelSelectTitleText != null)
            levelSelectTitleText.text = en ? "Select Level" : "Выбор уровня";

        if (easyText != null)
            easyText.text = en ? "Easy" : "Лёгкий";

        if (mediumText != null)
            mediumText.text = en ? "Medium" : "Средний";

        if (hardText != null)
            hardText.text = en ? "Hard" : "Тяжёлый";

        if (maxHardText != null)
            maxHardText.text = en ? "Max Hard" : "Макс. сложный";

        if (backFromSettingsText != null)
            backFromSettingsText.text = en ? "Back" : "Назад";

        if (backFromLevelsText != null)
            backFromLevelsText.text = en ? "Back" : "Назад";

        // Обновляем надпись выбранного уровня (если используется)
        if (selectedLevelLabel != null)
            selectedLevelLabel.Refresh();


        //  ====PortfolioPanel====
        if (BackText != null)
            BackText.text = en ? "Back" : "Назад";

    }
}