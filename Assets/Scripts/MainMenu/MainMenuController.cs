/*Этот скрипт управляет:
открытием/закрытием панелей
кнопкой Играть
кнопкой Выход (если захочешь добавить позже)*/


// Подключаем Unity API
using UnityEngine;

// Подключаем загрузку сцен
using UnityEngine.SceneManagement;

// Контроллер главного меню.
// Управляет панелями: главное меню / настройки / выбор уровня.
public class MainMenuController : MonoBehaviour
{
    [Header("Panels")] // Заголовок в инспекторе
    [SerializeField] private GameObject mainMenuPanel; // Панель главного меню (Играть / Настройки / Уровень)
    [SerializeField] private GameObject settingsPanel; // Панель настроек
    [SerializeField] private GameObject levelSelectPanel; // Панель выбора уровня

    [Header("Defaults")] // Заголовок в инспекторе
    [SerializeField] private QuizDifficultyPreset defaultPreset; // Пресет по умолчанию, если игрок ещё ничего не выбрал

    [Header("Optional UI")]
    [SerializeField] private SelectedLevelLabel selectedLevelLabel;

    private void Start() // Unity вызывает при запуске сцены
    {
        /*PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared");*/
        // Если выбранный уровень ещё не задан — ставим дефолтный (если он назначен)
        if (GameSessionSettings.SelectedPreset == null && defaultPreset != null)
        {
            GameSessionSettings.SelectedPreset = defaultPreset;
        }

        // При старте открываем главное меню
        ShowMainMenu();
    }

    // Показать главное меню, скрыть остальные панели
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true); // Включаем главное меню
        if (settingsPanel != null) settingsPanel.SetActive(false); // Выключаем настройки
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false); // Выключаем выбор уровня
        // Обновляем надпись выбранного уровня при возврате в главное меню
        if (selectedLevelLabel != null) selectedLevelLabel.Refresh();
    }

    // Открыть панель настроек
    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false); // Скрываем главное меню
        if (settingsPanel != null) settingsPanel.SetActive(true); // Показываем настройки
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false); // Скрываем выбор уровня
    }

    // Открыть панель выбора уровня
    public void OpenLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false); // Скрываем главное меню
        if (settingsPanel != null) settingsPanel.SetActive(false); // Скрываем настройки
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true); // Показываем выбор уровня
    }

    // Нажатие на кнопку "Играть"
    // Загружает сцену игры с текущим выбранным пресетом.
    /*public void Play()
    {
        // Если пресет не выбран и дефолт тоже не назначен — это ошибка настройки меню
        if (GameSessionSettings.SelectedPreset == null)
        {
            Debug.LogError("[MainMenuController] SelectedPreset is null. Assign defaultPreset or choose a level first.");
            return;
        }

        // Загружаем сцену игры
        SceneManager.LoadScene(GameSessionSettings.GameSceneName);
    }*/
    public void Play()
    {
        // Если preset не выбран - запускать игру нельзя.
        if (GameSessionSettings.SelectedPreset == null)
        {
            Debug.LogError("[MainMenuController] SelectedPreset is null. Assign defaultPreset or choose a level first.");
            return;
        }

        // Если мост рекламы не найден - не ломаем игру, просто грузим сцену.
        if (YandexAdsBridge.Instance == null)
        {
            Debug.LogWarning("[MainMenuController] YandexAdsBridge not found. Loading game without interstitial.");
            SceneManager.LoadScene(GameSessionSettings.GameSceneName);
            return;
        }

        // Показываем interstitial.
        // После закрытия рекламы грузим игровую сцену.
        YandexAdsBridge.Instance.ShowInterstitialThen(() =>
        {
            SceneManager.LoadScene(GameSessionSettings.GameSceneName);
        });
    }


    // Универсальная кнопка "Назад" (из Настроек/Выбора уровня)
    public void BackToMainMenu()
    {
        ShowMainMenu();
    }

    // Опционально: выход из игры (на ПК работает, в WebGL обычно игнорируется)
    public void QuitGame()
    {
        Debug.Log("[MainMenuController] Quit requested.");

        Application.Quit();
    }
}