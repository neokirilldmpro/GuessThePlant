// Подключаем пространство имён для корутин (IEnumerator)
using System.Collections;

// Подключаем List<T>
using System.Collections.Generic;

// Подключаем Unity API (MonoBehaviour, Debug, Random, WaitForSeconds и т.д.)
using UnityEngine;

// Этот компонент управляет игровым процессом викторины: генерирует вопросы,
// показывает их через UI, принимает ответы, считает score/streak, завершает раунд.
public class QuizGameController : MonoBehaviour
{
    // ===== ДАННЫЕ =====

    [Header("Data")] // Просто подпись в инспекторе Unity
    [SerializeField] private FlowerDatabase database; // Ссылка на базу всех цветов (ScriptableObject)
    [SerializeField] private QuizDifficultyPreset preset; // Ссылка на пресет сложности (ScriptableObject)

    // ===== UI =====

    [Header("UI")] // Подпись в инспекторе
    [SerializeField] private QuizUIView ui; // Ссылка на UI-скрипт, который показывает вопрос и кнопки

    // ===== НАСТРОЙКИ ПРОЦЕССА =====

    [Header("Flow")] // Подпись в инспекторе
    [SerializeField] private float revealDelaySeconds = 0.6f; // Сколько ждать после ответа перед следующим вопросом

    // ===== ВНУТРЕННЕЕ СОСТОЯНИЕ =====

    private List<FlowerQuestion> _questions; // Список сгенерированных вопросов текущего раунда
    private int _currentIndex; // Индекс текущего вопроса (0..Count-1)
    private int _score; // Сколько правильных ответов
    private int _streak; // Серия правильных ответов подряд
    private bool _inputLocked; // Блокировка ввода во время подсветки (чтобы не кликали 10 раз)

    // Awake вызывается до Start; тут удобно подписаться на события UI
    private void Awake()
    {
        // Если ui назначен — подписываемся на событие клика по варианту
        if (ui != null)
            ui.OptionClicked += OnOptionClicked;
    }

    // OnDestroy вызывается при удалении объекта/сцены; важно отписаться от событий
    private void OnDestroy()
    {
        // Если ui назначен — отписываемся
        if (ui != null)
            ui.OptionClicked -= OnOptionClicked;
    }

    // Start вызывается на старте сцены (после Awake)
    private void Start()
    {
        // Автостарт раунда (если хочешь запускать из меню — можно убрать и вызывать StartQuiz() вручную)
        StartQuiz();
    }

    // Публичный метод: начать/перезапустить раунд
    public void StartQuiz()
    {

        // Проверяем, что база назначена
        if (database == null)
        {
            Debug.LogError("[QuizGameController] Database is not assigned!");
            return;
        }

        // Если в меню был выбран пресет — используем его.
        // Это позволяет запускать GameScene из MenuScene.
        if (GameSessionSettings.SelectedPreset != null)
        {
            preset = GameSessionSettings.SelectedPreset;
        }

        // Проверяем, что пресет назначен
        if (preset == null)
        {
            Debug.LogError("[QuizGameController] Preset is not assigned!");
            return;
        }

        // Проверяем, что UI назначен
        if (ui == null)
        {
            Debug.LogError("[QuizGameController] UI is not assigned!");
            return;
        }

        // Сбрасываем состояние раунда
        _score = 0;
        _streak = 0;
        _currentIndex = 0;
        _inputLocked = false;

        // Генерируем вопросы по базе и пресету
        //_questions = FlowerQuestionGenerator.Generate(database, preset, optionsCount: 4);
        _questions = FlowerQuestionGenerator.Generate(database, preset, LanguageService.UseEnglish, optionsCount: 4);

        // Если генератор вернул пусто — значит что-то не так с пулом/базой
        if (_questions == null || _questions.Count == 0)
        {
            Debug.LogError("[QuizGameController] No questions generated. Check database content and preset settings.");
            return;
        }

        // Показываем первый вопрос
        ShowCurrentQuestion();
    }

    // Показать текущий вопрос на UI
    private void ShowCurrentQuestion()
    {
        // Если индекс вышел за список — значит раунд закончился
        if (_currentIndex < 0 || _currentIndex >= _questions.Count)
        {
            FinishQuiz();
            return;
        }

        // Берём текущий вопрос
        FlowerQuestion q = _questions[_currentIndex];

        // Отдаём его в UI для отображения
        // +1 делаем, потому что игроку приятнее видеть 1/20, а не 0/20
        
        ui.ShowQuestion(
            q,
            currentIndex: _currentIndex + 1,
            total: _questions.Count,
            score: _score,
            streak: _streak,
            english: LanguageService.UseEnglish
        );
    }

    // Обработчик клика по кнопке-ответу
    private void OnOptionClicked(int chosenIndex)
    {
        // Если ввод заблокирован — игнорируем клик
        if (_inputLocked)
            return;

        // Защита: если вопросов нет — тоже игнорируем
        if (_questions == null || _questions.Count == 0)
            return;

        // Берём текущий вопрос
        FlowerQuestion q = _questions[_currentIndex];

        // Проверяем, правильный ли индекс выбрал игрок
        bool isCorrect = (chosenIndex == q.correctIndex);

        // Блокируем ввод на время подсветки
        _inputLocked = true;

        // Запрещаем кнопки (чтобы игрок не нажал ещё раз)
        ui.SetInteractable(false);

        // Если ответ правильный
        if (isCorrect)
        {
            // Увеличиваем счёт
            _score++;

            // Увеличиваем серию
            _streak++;

            // Подсвечиваем выбранную кнопку как правильную
            ui.MarkCorrect(chosenIndex);
        }
        else // Если ответ неправильный
        {
            // Сбрасываем серию
            _streak = 0;

            // Подсвечиваем выбранную кнопку красным
            ui.MarkWrong(chosenIndex);

            // Подсвечиваем правильную кнопку зелёным
            //ui.MarkCorrect(q.correctIndex);
        }

        // Запускаем корутину: подождать и перейти к следующему вопросу
        StartCoroutine(GoNextAfterDelay());
    }

    // Корутинa ожидания (пауза перед следующим вопросом)
    private IEnumerator GoNextAfterDelay()
    {
        // Берём задержку и защищаемся от отрицательных значений
        float delay = Mathf.Max(0f, revealDelaySeconds);

        // Если задержка больше 0 — ждём
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // Переходим на следующий вопрос
        _currentIndex++;

        // Разблокируем ввод
        _inputLocked = false;

        // Показываем следующий вопрос (или завершаем)
        ShowCurrentQuestion();
    }

    // Завершение раунда
    private void FinishQuiz()
    {
        // Получаем имя режима (для сохранения)
        string modeName = (preset != null) ? preset.PresetName : "Unknown";

        // Загружаем лучший результат
        int best = SimpleSaveService.LoadBestScore(modeName);

        // Если текущий лучше — сохраняем
        if (_score > best)
            SimpleSaveService.SaveBestScore(modeName, _score);

        // Отмечаем режим как пройденный (можешь потом использовать для UI)
        SimpleSaveService.SaveCompleted(modeName, true);

        // Для начала просто лог в консоль
        Debug.Log($"[QuizGameController] Finished. Mode={modeName}, Score={_score}/{_questions.Count}");

        // TODO: позже здесь будет включаться ResultPanel (экран результата)
    }
}