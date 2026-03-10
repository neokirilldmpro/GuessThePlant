using UnityEngine;

// ¬ызываем обновление текстов меню сразу при загрузке MenuScene
public class MenuLanguageBootstrap : MonoBehaviour
{
    [SerializeField] private SettingsLanguageController settingsLanguageController; // ссылка на твой контроллер локализации

    private void Start()
    {
        if (settingsLanguageController != null)
            settingsLanguageController.RefreshTexts();
    }

    private void OnEnable()
    {
        if (settingsLanguageController != null)
            settingsLanguageController.RefreshTexts();
    }
}