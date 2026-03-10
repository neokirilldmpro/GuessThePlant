using TMPro;          // Для TMP_Text
using UnityEngine;    // Unity API
using UnityEngine.UI; // Button

// Этот скрипт вешается на кнопку rewarded-рекламы в GameScene.
// Его задача:
// 1) по клику показать rewarded;
// 2) если награда реально выдана - попросить QuizGameController
//    засчитать текущий ответ как правильный.
public class RewardedCorrectAnswerButtonYG : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private QuizGameController quizGameController;
    // Ссылка на контроллер викторины.
    // Именно он знает текущий вопрос и умеет засчитать "правильный ответ".

    [SerializeField] private Button button;
    // Кнопка rewarded-рекламы.

    [SerializeField] private TMP_Text buttonText;
    // Текст на кнопке.

    [Header("Texts")]
    [SerializeField] private string ruText = "Правильный ответ";
    [SerializeField] private string enText = "Right answer";
    // Текст кнопки на двух языках.

    private void Start()
    {
        RefreshText();
    }

    private void OnEnable()
    {
        RefreshText();
    }

    // Обновление текста по текущему языку.
    public void RefreshText()
    {
        if (buttonText == null)
            return;

        bool en = LanguageService.UseEnglish;
        buttonText.text = en ? enText : ruText;
    }

    // Этот метод вешаем на OnClick кнопки.
    public void OnRewardButtonPressed()
    {
        // Если ссылки на QuizGameController нет - выходим с логом.
        if (quizGameController == null)
        {
            Debug.LogError("[RewardedCorrectAnswerButtonYG] quizGameController is not assigned!");
            return;
        }

        // Если рекламный мост не найден - тоже выходим.
        if (YandexAdsBridge.Instance == null)
        {
            Debug.LogWarning("[RewardedCorrectAnswerButtonYG] YandexAdsBridge not found.");
            return;
        }

        // Если сама кнопка назначена - можно временно её выключить,
        // чтобы игрок не кликал подряд.
        if (button != null)
            button.interactable = false;

        // Просим показать rewarded.
        YandexAdsBridge.Instance.ShowRewardedForCorrectAnswer(() =>
        {
            // Когда награда реально получена,
            // пробуем засчитать текущий вопрос как правильный.
            bool rewardUsed = quizGameController.TryUseRewardedCorrectAnswer();

            // Если по какой-то причине не удалось применить награду,
            // пишем это в лог.
            if (!rewardUsed)
            {
                Debug.LogWarning("[RewardedCorrectAnswerButtonYG] Reward granted, but current question could not be completed.");
            }
        });

        // ВАЖНО:
        // Через небольшую задержку вернём кнопку в доступное состояние.
        // Это простой UX-фолбэк на случай, если реклама не открылась.
        Invoke(nameof(ReEnableButton), 0.5f);
    }

    private void ReEnableButton()
    {
        if (button != null)
            button.interactable = true;
    }
}