using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Этот компонент управляет всей логикой раунда:
// 1) генерирует вопросы,
// 2) показывает их через UI,
// 3) принимает ответы,
// 4) считает score / streak,
// 5) завершает этап,
// 6) сохраняет результат через StageProgressService,
// 7) выдаёт нашивки этапа и кампании,
// 8) поддерживает таймер на timed-этапах.
public class QuizGameController : MonoBehaviour
{
    // -------------------------
    // ДАННЫЕ
    // -------------------------

    [Header("Data")]
    [SerializeField] private FlowerDatabase database;
    // База всех цветов.

    [SerializeField] private QuizDifficultyPreset preset;
    // Preset текущего этапа.

    [SerializeField] private AchievementVisuals achievementVisuals;
    // Спрайты нашивок.

    // -------------------------
    // UI
    // -------------------------

    [Header("UI")]
    [SerializeField] private QuizUIView ui;
    // Игровой UI.

    [SerializeField] private QuizResultView resultView;
    // ResultPanel.

    // -------------------------
    // НАСТРОЙКИ ЛОГИКИ
    // -------------------------

    [Header("Flow")]
    [SerializeField] private float revealDelaySeconds = 0.6f;
    // Задержка после ответа перед следующим вопросом.

    [SerializeField] private int allowedMistakesToPass = 2;
    // Сколько ошибок можно допустить, чтобы этап считался пройденным.

    // -------------------------
    // ВНУТРЕННЕЕ СОСТОЯНИЕ
    // -------------------------

    private List<FlowerQuestion> _questions;
    private int _currentIndex;
    private int _score;
    private int _streak;
    private bool _inputLocked;
    private bool _roundActive;
    private bool _resultAlreadyShown;
    private float _elapsedStageTime;

    // -------------------------
    // СОСТОЯНИЕ ТАЙМЕРА
    // -------------------------

    private bool _timedStage;
    // true = у текущего этапа включён таймер

    private bool _questionTimerRunning;
    // true = таймер текущего вопроса сейчас тикает

    private float _questionTimeRemaining;
    // Сколько секунд осталось на текущий вопрос

    private float _questionTimeLimit;
    // Полный лимит времени на текущий вопрос.
    // Нужен для отображения fillAmount.

    // -------------------------
    // ПОДПИСКА НА UI
    // -------------------------

    private void Awake()
    {
        if (ui != null)
            ui.OptionClicked += OnOptionClicked;
    }

    private void OnDestroy()
    {
        if (ui != null)
            ui.OptionClicked -= OnOptionClicked;
    }

    // -------------------------
    // UNITY ЖИЗНЕННЫЙ ЦИКЛ
    // -------------------------

    private void Start()
    {
        StartQuiz();
    }

    private void Update()
    {
        // Пока этап активен — копим общее время прохождения этапа.
        if (_roundActive)
            _elapsedStageTime += Time.deltaTime;

        // Отдельно обрабатываем таймер текущего вопроса.
        UpdateQuestionTimer();
    }

    // -------------------------
    // СТАРТ ЭТАПА
    // -------------------------

    public void StartQuiz()
    {
        if (database == null)
        {
            Debug.LogError("[QuizGameController] Database is not assigned!");
            return;
        }

        // Если этап выбран в меню — используем его.
        if (GameSessionSettings.SelectedPreset != null)
            preset = GameSessionSettings.SelectedPreset;

        if (preset == null)
        {
            Debug.LogError("[QuizGameController] Preset is not assigned!");
            return;
        }

        if (ui == null)
        {
            Debug.LogError("[QuizGameController] UI is not assigned!");
            return;
        }

        _score = 0;
        _streak = 0;
        _currentIndex = 0;
        _inputLocked = false;
        _resultAlreadyShown = false;
        _elapsedStageTime = 0f;
        _roundActive = true;

        // Определяем, timed ли это этап.
        _timedStage = preset.UseTimer;

        // На всякий случай сразу прячем таймер,
        // если этап обычный.
        if (ui != null)
            ui.SetTimerVisible(_timedStage);

        _questions = FlowerQuestionGenerator.Generate(
            database,
            preset,
            LanguageService.UseEnglish,
            optionsCount: 4
        );

        if (_questions == null || _questions.Count == 0)
        {
            _roundActive = false;
            Debug.LogError("[QuizGameController] No questions generated. Check database and preset.");
            return;
        }

        if (resultView != null)
            resultView.Hide();

        ShowCurrentQuestion();
    }

    // -------------------------
    // ПОКАЗ ТЕКУЩЕГО ВОПРОСА
    // -------------------------

    private void ShowCurrentQuestion()
    {
        // Если вопросы закончились — завершаем этап.
        if (_questions == null || _currentIndex < 0 || _currentIndex >= _questions.Count)
        {
            FinishQuiz();
            return;
        }

        FlowerQuestion q = _questions[_currentIndex];

        // Каждый новый вопрос должен начинаться без сообщения "Время вышло!".
        if (ui != null)
            ui.HideTimeExpiredMessage();

        ui.ShowQuestion(
            q,
            currentIndex: _currentIndex + 1,
            total: _questions.Count,
            score: _score,
            streak: _streak,
            english: LanguageService.UseEnglish
        );

        // Каждый раз, когда показываем новый вопрос,
        // заново запускаем таймер, если этап timed.
        StartQuestionTimerIfNeeded();
    }

    // -------------------------
    // ЛОГИКА ТАЙМЕРА
    // -------------------------

    private void StartQuestionTimerIfNeeded()
    {
        // Если этап не timed — просто скрываем таймер.
        if (!_timedStage)
        {
            _questionTimerRunning = false;

            if (ui != null)
                ui.SetTimerVisible(false);

            return;
        }

        // Если в preset случайно поставили 0 или меньше —
        // подстрахуемся, чтобы игра не ломалась.
        _questionTimeLimit = Mathf.Max(0.1f, preset.SecondsPerQuestion);
        _questionTimeRemaining = _questionTimeLimit;
        _questionTimerRunning = true;

        if (ui != null)
        {
            ui.SetTimerVisible(true);
            ui.UpdateTimerDisplay(
                _questionTimeRemaining,
                _questionTimeLimit,
                LanguageService.UseEnglish
            );
        }
    }

    private void StopQuestionTimer()
    {
        _questionTimerRunning = false;
    }

    private void UpdateQuestionTimer()
    {
        // Если таймер не нужен или не запущен — выходим.
        if (!_timedStage)
            return;

        if (!_questionTimerRunning)
            return;

        // Пока идёт reveal / переход / popup — таймер не должен тикать.
        if (_inputLocked)
            return;

        // Уменьшаем время.
        _questionTimeRemaining -= Time.deltaTime;

        // Если время закончилось — обрабатываем timeout.
        if (_questionTimeRemaining <= 0f)
        {
            _questionTimeRemaining = 0f;

            if (ui != null)
            {
                ui.UpdateTimerDisplay(
                    _questionTimeRemaining,
                    _questionTimeLimit,
                    LanguageService.UseEnglish
                );
            }

            HandleTimeExpired();
            return;
        }

        // Обновляем UI таймера каждый кадр.
        if (ui != null)
        {
            ui.UpdateTimerDisplay(
                _questionTimeRemaining,
                _questionTimeLimit,
                LanguageService.UseEnglish
            );
        }
    }

    private void HandleTimeExpired()
    {
        // Если мы уже заблокировали ввод — повторно не заходим.
        if (_inputLocked)
            return;

        // Если текущий индекс сломан — выходим.
        if (_questions == null || _currentIndex < 0 || _currentIndex >= _questions.Count)
            return;

        FlowerQuestion q = _questions[_currentIndex];

        // Время закончилось -> вопрос считается ошибкой.
        _inputLocked = true;
        _questionTimerRunning = false;
        _streak = 0;

        if (ui != null)
        {
            ui.SetInteractable(false);

            // Каждый новый вопрос должен начинаться без сообщения "Время вышло!".
            /*if (ui != null)
                ui.HideTimeExpiredMessage();*/
            if (ui != null)
                ui.ShowTimeExpiredMessage(); // показываем "Время вышло!"

            // Показываем правильный вариант,
            // чтобы игрок видел верный ответ.
            //ui.MarkCorrect(q.correctIndex);
        }

        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayWrong();

        StartCoroutine(GoNextAfterDelay());
    }

    // -------------------------
    // ОБРАБОТКА ОТВЕТА ИГРОКА
    // -------------------------

    private void OnOptionClicked(int chosenIndex)
    {
        if (_inputLocked)
            return;

        if (_questions == null || _questions.Count == 0)
            return;

        if (_currentIndex < 0 || _currentIndex >= _questions.Count)
            return;

        FlowerQuestion q = _questions[_currentIndex];
        bool isCorrect = (chosenIndex == q.correctIndex);

        _inputLocked = true;

        // Как только игрок ответил —
        // таймер этого вопроса больше не тикает.
        StopQuestionTimer();

        if (ui != null)
            ui.SetInteractable(false);

        if (isCorrect)
        {
            _score++;
            _streak++;

            if (ui != null)
                ui.MarkCorrect(chosenIndex);

            if (SfxManager.Instance != null)
                SfxManager.Instance.PlayCorrect();
        }
        else
        {
            _streak = 0;

            if (ui != null)
                ui.MarkWrong(chosenIndex);

            if (SfxManager.Instance != null)
                SfxManager.Instance.PlayWrong();
        }

        StartCoroutine(GoNextAfterDelay());
    }

    // -------------------------
    // ПЕРЕХОД К СЛЕДУЮЩЕМУ ВОПРОСУ
    // -------------------------

    private IEnumerator GoNextAfterDelay()
    {
        float delay = Mathf.Max(0f, revealDelaySeconds);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        _currentIndex++;
        _inputLocked = false;

        ShowCurrentQuestion();
    }

    // -------------------------
    // ЗАВЕРШЕНИЕ ЭТАПА
    // -------------------------

    private void FinishQuiz()
    {
        // Добавь это в самое начало метода FinishQuiz()
        if (OnlineMatchService.CurrentMatch != null && OnlineMatchService.CurrentMatch.IsOnlineMatch)
        {
            // Передаем результаты в онлайн-сервис
            OnlineMatchService.FinishMatch(_score, _elapsedStageTime);

            // Вместо обычного окна результатов показываем онлайн-окно
            if (resultView != null)
                resultView.ShowOnlineResult();

            return; // Прекращаем выполнение метода, чтобы не сработала логика кампании
        }

        if (_resultAlreadyShown)
            return;

        _resultAlreadyShown = true;
        _roundActive = false;
        _questionTimerRunning = false;

        if (ui != null)
            ui.SetTimerVisible(false);

        int total = _questions != null ? _questions.Count : 0;
        int score = _score;

        int requiredScore = Mathf.Max(0, total - allowedMistakesToPass);
        bool passed = score >= requiredScore;
        bool perfectRun = (score == total);

        float totalTimeSeconds = Mathf.Max(0f, _elapsedStageTime);

        List<AchievementId> unlockedNow = new List<AchievementId>();
        List<Sprite> newPatchSprites = new List<Sprite>();

        // Сохраняем результат этапа.
        StageProgressService.SaveStageResult(
            preset,
            score,
            total,
            totalTimeSeconds
        );
        // Отправляем суммарное время в лидерборд Яндекса.
        // Метод сам проверит — все ли этапы пройдены.
        // Если нет — просто ничего не отправит.
        LeaderboardService.Instance.SubmitTotalBestTime();

        // Если этап пройден идеально —
        // выдаём нашивку именно этого этапа.
        if (perfectRun)
        {
            AchievementId stagePerfectAchievement = preset.PerfectAchievementId;

            TryUnlockAchievement(
                stagePerfectAchievement,
                newPatchSprites,
                unlockedNow
            );
        }

        // Проверяем кампанию целиком.
        QuizDifficultyPreset[] allStages = GameSessionSettings.GetAllCampaignStages();

        if (allStages != null && allStages.Length > 0)
        {
            if (StageProgressService.AreAllCompleted(allStages))
            {
                TryUnlockAchievement(
                    AchievementId.CompleteCampaign,
                    newPatchSprites,
                    unlockedNow
                );
            }

            if (StageProgressService.AreAllPerfect(allStages))
            {
                TryUnlockAchievement(
                    AchievementId.PerfectCampaign,
                    newPatchSprites,
                    unlockedNow
                );
            }
        }

        if (ui != null)
            ui.SetInteractable(false);

        if (resultView != null)
        {
            resultView.Show(score, total, preset);
            // После сохранения результата лучший результат уже обновлён,
            // значит можно сразу показать и текущее, и лучшее время.
            float bestTime = StageProgressService.GetBestTime(preset);

            resultView.SetTimeInfo(totalTimeSeconds, bestTime, LanguageService.UseEnglish);

            resultView.ShowNewPatches(newPatchSprites);
        }

        string unlockedList = unlockedNow.Count > 0
            ? string.Join(", ", unlockedNow)
            : "none";

        Debug.Log(
            $"[QuizGameController] FinishQuiz -> StageKey={preset?.StageKey}, " +
            $"StageOrder={preset?.StageOrder}, Score={score}/{total}, Passed={passed}, " +
            $"Perfect={perfectRun}, TotalTime={totalTimeSeconds:F2}s, " +
            $"UnlockedNow={unlockedList}, PopupCount={newPatchSprites.Count}"
        );
    }

    private bool TryUnlockAchievement(
        AchievementId achievementId,
        List<Sprite> newPatchSprites,
        List<AchievementId> unlockedNow
    )
    {
        bool unlockedRightNow = AchievementService.Unlock(achievementId);

        if (!unlockedRightNow)
            return false;

        if (unlockedNow != null)
            unlockedNow.Add(achievementId);

        if (achievementVisuals == null)
        {
            Debug.LogWarning("[QuizGameController] AchievementVisuals is not assigned.");
            return true;
        }

        Sprite sprite = achievementVisuals.GetPatchSprite(achievementId);

        if (sprite == null)
        {
            Debug.LogWarning($"[QuizGameController] Sprite for achievement {achievementId} is not assigned in AchievementVisuals.");
            return true;
        }

        if (newPatchSprites != null)
            newPatchSprites.Add(sprite);

        return true;
    }

    // -------------------------
    // REWARDED: ПРАВИЛЬНЫЙ ОТВЕТ
    // -------------------------

    public bool TryUseRewardedCorrectAnswer()
    {
        if (_inputLocked)
            return false;

        if (_questions == null || _questions.Count == 0)
            return false;

        if (_currentIndex < 0 || _currentIndex >= _questions.Count)
            return false;

        FlowerQuestion q = _questions[_currentIndex];

        _inputLocked = true;

        // Rewarded тоже должен останавливать таймер вопроса,
        // иначе игрок терял бы время во время награды.
        StopQuestionTimer();

        if (ui != null)
            ui.SetInteractable(false);

        if (ui != null)
            ui.MarkCorrect(q.correctIndex);

        _score++;
        _streak++;

        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayCorrect();

        StartCoroutine(GoNextAfterDelay());

        return true;
    }
}