using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Этот класс отвечает за ResultPanel:
// показывает счёт,
// тексты,
// кнопки Next / Retry / Menu,
// и блок "Новая нашивка!"
public class QuizResultView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    // Корневой объект ResultPanel

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    // Заголовок: "Результат" / "Results"

    [SerializeField] private TMP_Text scoreText;
    // Текст со счётом: "Счёт: 18/20"

    [SerializeField] private TMP_Text requirementText;
    // Текст про открытие следующего уровня

    [SerializeField] private TMP_Text patchRequirementText;
    // НОВОЕ ПОЛЕ:
    // отдельный текст про условие получения нашивки

    [Header("Buttons")]
    [SerializeField] private Button nextLevelButton;
    // Кнопка "Следующий уровень"

    [SerializeField] private Button menuButton;
    // Кнопка "Меню"

    [SerializeField] private Button retryButton;
    // Кнопка "Переиграть"

    [Header("Rules")]
    [SerializeField] private int allowedMistakes = 2;
    // Сколько ошибок можно допустить для прохождения уровня

    [Header("New patch")]
    [SerializeField] private GameObject newPatchRoot;
    // Блок "Новая нашивка!"

    [SerializeField] private TMP_Text newPatchText;
    // Текст внутри этого блока

    [Header("Time Info")]
    [SerializeField] private TMP_Text currentTimeText;
    // Текст текущего времени этапа.

    [SerializeField] private TMP_Text bestTimeText;
    // Текст лучшего времени этапа.

    [SerializeField] private Image[] newPatchImages;

    // Картинка новой нашивки

    [Header("Animation")]
    [SerializeField] private UIPopAnimator newPatchPopAnimator;
    // Pop-анимация блока новой нашивки

    public void Hide()
    {
        // Просто скрываем всю панель результата
        if (root != null)
            root.SetActive(false);
    }

    public void Show(int score, int total, QuizDifficultyPreset currentPreset)
    {
        // На всякий случай сначала скрываем блок новой нашивки,
        // а потом уже решаем, нужно ли показывать его снова
        HideNewPatch();

        // Показываем корневую панель результата
        if (root != null)
            root.SetActive(true);

        // Проверяем текущий язык
        bool en = LanguageService.UseEnglish;

        // ---------- Заголовок ----------
        if (titleText != null)
            titleText.text = en ? "Results" : "Результат";

        // ---------- Счёт ----------
        if (scoreText != null)
            scoreText.text = en ? $"Score: {score}/{total}" : $"Счёт: {score}/{total}";

        // ---------- Логика прохождения ----------
        // Сколько нужно набрать минимум для прохождения уровня
        int requiredScore = Mathf.Max(0, total - allowedMistakes);

        // true = уровень считается пройденным
        bool passed = score >= requiredScore;

        // ---------- Текст про следующий уровень ----------
        if (requirementText != null)
        {
            if (passed)
            {
                // Если уровень пройден —
                // показываем короткое понятное сообщение
                requirementText.text = en
                    ? "Next level unlocked"
                    : "Следующий уровень открыт";
            }
            else
            {
                // Если не пройден —
                // показываем правило, сколько нужно набрать
                requirementText.text = en
                    ? $"To unlock next level: at least {requiredScore}/{total} (mistakes allowed: {allowedMistakes})"
                    : $"Чтобы открыть следующий уровень: минимум {requiredScore}/{total} (ошибок можно: {allowedMistakes})";
            }
        }

        // ---------- Текст про нашивку ----------
        if (patchRequirementText != null)
        {
            // Этот текст показываем всегда,
            // чтобы игрок понимал отдельное правило для нашивки
            patchRequirementText.text = en
                ? "Patch: only with no mistakes"
                : "Нашивка: только без ошибок";
        }

        // ---------- Кнопка Retry ----------
        // Если уровень НЕ пройден — показываем Retry
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(!passed);
            retryButton.interactable = true;
        }

        TMP_Text retryTxt = retryButton != null ? retryButton.GetComponentInChildren<TMP_Text>() : null;
        if (retryTxt != null)
            retryTxt.text = en ? "Retry" : "Переиграть";

        // ---------- Кнопка Next level ----------
        // Если уровень пройден — кнопка доступна,
        // если не пройден — кнопка отключена
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = passed;
            nextLevelButton.gameObject.SetActive(true);
        }

        TMP_Text nextTxt = nextLevelButton != null ? nextLevelButton.GetComponentInChildren<TMP_Text>() : null;
        if (nextTxt != null)
            nextTxt.text = en ? "Next level" : "Следующий уровень";

        // ---------- Кнопка Menu ----------
        TMP_Text menuTxt = menuButton != null ? menuButton.GetComponentInChildren<TMP_Text>() : null;
        if (menuTxt != null)
            menuTxt.text = en ? "Menu" : "Меню";
    }

    // Кнопка "Следующий уровень"
 
    public void OnNextLevelPressed()
    {
        // Если рекламный мост не найден -
        // идём по старой логике без рекламы.
        if (YandexAdsBridge.Instance == null)
        {
            Debug.LogWarning("[QuizResultView] YandexAdsBridge not found. Next level without interstitial.");

            bool switchedNoAds = PresetProgression.TrySelectNextPreset();

            if (switchedNoAds)
                SceneManager.LoadScene(GameSessionSettings.GameSceneName);
            else
                SceneManager.LoadScene(GameSessionSettings.MenuSceneName);

            return;
        }

        // Показываем interstitial.
        // После закрытия пытаемся выбрать следующий уровень.
        YandexAdsBridge.Instance.ShowInterstitialThen(() =>
        {
            bool switched = PresetProgression.TrySelectNextPreset();

            if (switched)
            {
                SceneManager.LoadScene(GameSessionSettings.GameSceneName);
            }
            else
            {
                SceneManager.LoadScene(GameSessionSettings.MenuSceneName);
            }
        });
    }


    // Кнопка "Меню"
    public void OnMenuPressed()
    {
        SceneManager.LoadScene(GameSessionSettings.MenuSceneName);
    }

    // Кнопка "Переиграть"
    
    public void OnRetryPressed()
    {
        // Если рекламный мост не найден - просто перезагружаем уровень.
        if (YandexAdsBridge.Instance == null)
        {
            Debug.LogWarning("[QuizResultView] YandexAdsBridge not found. Retry without interstitial.");
            SceneManager.LoadScene(GameSessionSettings.GameSceneName);
            return;
        }

        // Показываем interstitial,
        // потом перезапускаем тот же уровень.
        YandexAdsBridge.Instance.ShowInterstitialThen(() =>
        {
            SceneManager.LoadScene(GameSessionSettings.GameSceneName);
        });
    }

    // Показ блока "Новая нашивка!"
    public void ShowNewPatches(System.Collections.Generic.List<Sprite> patchSprites)
    {
        // Если список пустой или null —
        // скрываем блок новых нашивок.
        if (patchSprites == null || patchSprites.Count == 0)
        {
            HideNewPatch();
            return;
        }

        // Показываем корневой блок.
        if (newPatchRoot != null)
            newPatchRoot.SetActive(true);

        bool en = LanguageService.UseEnglish;

        // Если открылась одна нашивка — текст в единственном числе.
        // Если несколько — во множественном.
        if (newPatchText != null)
        {
            if (patchSprites.Count == 1)
            {
                newPatchText.text = en ? "New patch unlocked!" : "Новая нашивка!";
            }
            else
            {
                newPatchText.text = en ? "New patches unlocked!" : "Новые нашивки!";
            }
        }

        // Если массив картинок не назначен — дальше продолжать бессмысленно.
        if (newPatchImages == null || newPatchImages.Length == 0)
        {
            Debug.LogWarning("[QuizResultView] newPatchImages array is empty.");
            return;
        }

        // Сначала скрываем все image-слоты.
        for (int i = 0; i < newPatchImages.Length; i++)
        {
            if (newPatchImages[i] == null)
                continue;

            newPatchImages[i].enabled = false;
        }

        // Потом показываем столько картинок, сколько реально есть,
        // но не больше длины массива.
        int count = Mathf.Min(newPatchImages.Length, patchSprites.Count);

        for (int i = 0; i < count; i++)
        {
            if (newPatchImages[i] == null)
                continue;

            newPatchImages[i].sprite = patchSprites[i];
            newPatchImages[i].enabled = true;
            newPatchImages[i].preserveAspect = true;
        }

        // Если назначен pop animator — проигрываем анимацию.
        if (newPatchPopAnimator != null)
            newPatchPopAnimator.Play();
    }

    // Скрытие блока новой нашивки
    public void HideNewPatch()
    {
        if (newPatchRoot != null)
            newPatchRoot.SetActive(false);

        if (newPatchImages != null)
        {
            for (int i = 0; i < newPatchImages.Length; i++)
            {
                if (newPatchImages[i] != null)
                    newPatchImages[i].enabled = false;
            }
        }
    }

    // Этот метод показывает время текущего прохождения
    // и лучшее время для этапа.
    public void SetTimeInfo(float currentTimeSeconds, float bestTimeSeconds, bool english)
    {
        if (currentTimeText != null)
        {
            currentTimeText.text = english
                ? $"Stage time: {FormatTime(currentTimeSeconds)}"
                : $"Время этапа: {FormatTime(currentTimeSeconds)}";
        }

        if (bestTimeText != null)
        {
            if (bestTimeSeconds <= 0f)
            {
                bestTimeText.text = english
                    ? "Best time: —"
                    : "Лучшее время: —";
            }
            else
            {
                bestTimeText.text = english
                    ? $"Best time: {FormatTime(bestTimeSeconds)}"
                    : $"Лучшее время: {FormatTime(bestTimeSeconds)}";
            }
        }
    }
    private string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);

        int totalSeconds = Mathf.RoundToInt(seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;

        return $"{minutes:00}:{secs:00}";
    }
}