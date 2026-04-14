using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Управляет выходом в меню из GameScene (с подтверждением)
public class ExitToMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject confirmPanel; // Панель подтверждения (ExitConfirmPanel)
    [SerializeField] private TMP_Text confirmText;    // Текст на панели подтверждения

    [Header("Buttons")]
    [SerializeField] private TMP_Text _OnConfirmYes;
    [SerializeField] private TMP_Text _OnConfirmNo;

    private void Start()
    {
        // При старте игры панель подтверждения скрыта
        HideConfirm();
    }

    // Нажатие на кнопку "Меню" в HUD
    public void OnMenuButtonPressed()
    {
        ShowConfirm();
    }

    // Кнопка "Да" (подтверждение выхода в меню)
    public void OnConfirmYes()
    {
        // === ДОБАВЛЕНО: Сбрасываем онлайн-матч перед выходом ===
        if (OnlineMatchService.CurrentMatch != null)
        {
            OnlineMatchService.ClearMatch();
        }

        // Загружаем сцену меню
        SceneManager.LoadScene(GameSessionSettings.MenuSceneName);
    }

    // Нажатие "Нет"
    public void OnConfirmNo()
    {
        HideConfirm();
    }

    private void ShowConfirm()
    {
        bool en = LanguageService.UseEnglish;
        if (confirmPanel != null)
            confirmPanel.SetActive(true);

        // Локализация текста
        if (confirmText != null)
        {
            /*bool en = LanguageService.UseEnglish;*/
            confirmText.text = en ? "Exit to menu?" : "Выйти в меню?";
        }

        // Подпись на OnConfirmYes
        TMP_Text _OnConfirmYesTxt = _OnConfirmYes != null ? _OnConfirmYes.GetComponentInChildren<TMP_Text>() : null;
        if (_OnConfirmYesTxt != null) _OnConfirmYesTxt.text = en ? "Yes" : "Да";

        // Подпись на OnConfirmNo
        TMP_Text _OnConfirmNoTxt = _OnConfirmNo != null ? _OnConfirmNo.GetComponentInChildren<TMP_Text>() : null;
        if (_OnConfirmNoTxt != null) _OnConfirmNoTxt.text = en ? "No" : "Нет";

        // (Опционально) можно поставить паузу, чтобы игра не шла
        Time.timeScale = 0f;
    }

    private void HideConfirm()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // Снимаем паузу
        Time.timeScale = 1f;
    }
}