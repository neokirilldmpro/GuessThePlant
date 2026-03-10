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

    [SerializeField] private QuizResultView resultView;

    // ===== НАСТРОЙКИ ПРОЦЕССА =====

    [Header("Flow")] // Подпись в инспекторе
    [SerializeField] private float revealDelaySeconds = 0.6f; // Сколько ждать после ответа перед следующим вопросом

    // ===== ВНУТРЕННЕЕ СОСТОЯНИЕ =====

    private List<FlowerQuestion> _questions; // Список сгенерированных вопросов текущего раунда
    private int _currentIndex; // Индекс текущего вопроса (0..Count-1)
    private int _score; // Сколько правильных ответов
    private int _streak; // Серия правильных ответов подряд
    private bool _inputLocked; // Блокировка ввода во время подсветки (чтобы не кликали 10 раз)


    [SerializeField] private AchievementVisuals achievementVisuals;

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
        if (resultView != null) resultView.Hide();
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

            // SFX правильного ответа
            if (SfxManager.Instance != null)
                SfxManager.Instance.PlayCorrect();
        }
        else // Если ответ неправильный
        {
            // Сбрасываем серию
            _streak = 0;

            // Подсвечиваем выбранную кнопку красным
            ui.MarkWrong(chosenIndex);

            // Подсвечиваем правильную кнопку зелёным
            //ui.MarkCorrect(q.correctIndex);

            // SFX неправильного ответа
            if (SfxManager.Instance != null)
                SfxManager.Instance.PlayWrong();
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

    private void FinishQuiz()
    {
        int total = _questions != null ? _questions.Count : 0;
        int score = _score;

        // Пройти уровень можно с ошибками <= 2
        int allowedMistakes = 2;
        int requiredScore = Mathf.Max(0, total - allowedMistakes);
        bool passed = score >= requiredScore;

        // Нашивка только за идеальное прохождение
        bool perfectRun = (score == total);

        Sprite newPatchSprite = null;

        // 1) Сохраняем прогресс уровня
        string modeName = (preset != null) ? preset.PresetName : "Unknown";

        int best = SimpleSaveService.LoadBestScore(modeName);
        if (score > best)
            SimpleSaveService.SaveBestScore(modeName, score);

        if (passed)
            SimpleSaveService.SaveCompleted(modeName, true);

        // 2) Выдаём нашивку только без единой ошибки
        if (perfectRun)
        {
            AchievementId? id = GetAchievementIdForPreset(preset);

            if (id.HasValue)
            {
                bool alreadyUnlocked = AchievementService.IsUnlocked(id.Value);

                if (!alreadyUnlocked)
                {
                    AchievementService.Unlock(id.Value);

                    if (achievementVisuals != null)
                        newPatchSprite = achievementVisuals.GetPatchSprite(id.Value);
                }
            }
        }

        // 3) Показываем результат
        if (ui != null)
            ui.SetInteractable(false);

        if (resultView != null)
        {
            resultView.Show(score, total, preset);
            resultView.ShowNewPatch(newPatchSprite);
        }

        Debug.Log(
            $"[QuizGameController] Finished. Mode={modeName}, Score={score}/{total}, Passed={passed}, Perfect={perfectRun}, NewPatch={(newPatchSprite != null)}"
        );
    }
    private void UnlockAchievementForPreset(QuizDifficultyPreset p)
    {
        if (p == null) return;

        string key = p.PresetName.Trim().ToLowerInvariant();

        if (key == "easy") AchievementService.Unlock(AchievementId.CompleteEasy);
        else if (key == "medium") AchievementService.Unlock(AchievementId.CompleteMedium);
        else if (key == "hard") AchievementService.Unlock(AchievementId.CompleteHard);
        else if (key == "maxhard" || key == "max hard" || key == "max_hard") AchievementService.Unlock(AchievementId.CompleteMaxHard);
    }

    private AchievementId? GetAchievementIdForPreset(QuizDifficultyPreset p)
    {
        if (p == null) return null;

        string key = p.PresetName.Trim().ToLowerInvariant();

        if (key == "easy") return AchievementId.CompleteEasy;
        if (key == "medium") return AchievementId.CompleteMedium;
        if (key == "hard") return AchievementId.CompleteHard;
        if (key == "maxhard" || key == "max hard" || key == "max_hard") return AchievementId.CompleteMaxHard;

        return null;
    }

    public bool TryUseRewardedCorrectAnswer()
    {
        // Если ввод уже заблокирован,
        // значит либо игрок уже нажал ответ,
        // либо сейчас идёт переход к следующему вопросу.
        if (_inputLocked)
            return false;

        // Если вопросов нет - использовать награду нельзя.
        if (_questions == null || _questions.Count == 0)
            return false;

        // Если индекс вопроса некорректный - тоже выходим.
        if (_currentIndex < 0 || _currentIndex >= _questions.Count)
            return false;

        // Берём текущий вопрос.
        FlowerQuestion q = _questions[_currentIndex];

        // На время обработки награды блокируем ввод.
        _inputLocked = true;

        // Отключаем нажатия по вариантам ответа.
        if (ui != null)
            ui.SetInteractable(false);

        // Подсвечиваем правильный вариант.
        // Для игрока это выглядит так:
        // он посмотрел rewarded -> игра показала правильный ответ.
        if (ui != null)
            ui.MarkCorrect(q.correctIndex);

        // Засчитываем это как правильный ответ.
        _score++;

        // И увеличиваем streak,
        // потому что с точки зрения результата ответ считается правильным.
        _streak++;

        // Проигрываем "правильный" звук.
        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayCorrect();

        // Переходим к следующему вопросу после той же задержки,
        // что и при обычном правильном ответе.
        StartCoroutine(GoNextAfterDelay());

        return true;
    }
}