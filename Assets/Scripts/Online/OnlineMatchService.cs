using UnityEngine;
using UnityEngine.SceneManagement;

public static class OnlineMatchService
{
    public const int QuestionsCount = 10;
    public static OnlineMatchData CurrentMatch { get; private set; }

    // === НОВАЯ ПЕРЕМЕННАЯ: Запоминаем, кто был соперником в прошлый раз ===
    private static bool _lastWasGhost = false;


    // ЗАПУСК МАТЧА
    public static void StartMatch()
    {
        bool en = LanguageService.UseEnglish; // Проверяем текущий язык
        CurrentMatch = new OnlineMatchData { IsOnlineMatch = true };

        // === ЛОГИКА ЧЕРЕДОВАНИЯ ===
        // Если призрак вообще существует, берём его ТОЛЬКО если в прошлый раз был НЕ он.
        bool useGhost = false;
        if (GhostRecordService.HasGhost())
        {
            useGhost = !_lastWasGhost; // Инвертируем прошлый выбор
        }

        if (useGhost)
        {
            CurrentMatch.IsGhost = true;
            CurrentMatch.OpponentScore = GhostRecordService.LoadScore();

            // === ШУМ ВО ВРЕМЕНИ И ФИЗИЧЕСКИЙ ПРЕДЕЛ ===
            float exactTime = GhostRecordService.LoadTime();

            // Генерируем шум (от -0.8 до +2.5 секунд)
            float fuzzedTime = exactTime + Random.Range(-0.8f, 2.5f);

            // Устанавливаем минимально возможное время (например, 9.8 сек)
            float absoluteMinTime = 9.8f;

            // Выбираем то, что больше: либо наш лимит, либо время с шумом
            CurrentMatch.OpponentTime = Mathf.Max(absoluteMinTime, fuzzedTime);

            // Маскируем призрака случайным никнеймом
            CurrentMatch.OpponentName = BotOpponent.GetRandomName(en);

            _lastWasGhost = true; // Запоминаем, что сейчас была Тень

            Debug.Log(
                $"[OnlineMatchService] Соперник: ПРИЗРАК '{CurrentMatch.OpponentName}'. " +
                $"Ориг. время: {exactTime:F1}s, С шумом: {CurrentMatch.OpponentTime:F1}s"
            );
        }
        else
        {
            // Создаем обычного бота
            CurrentMatch.IsGhost = false;
            var bot = BotOpponent.Generate(OnlineWinsService.GetWins(), en);
            CurrentMatch.OpponentScore = bot.score;
            CurrentMatch.OpponentTime = bot.time;
            CurrentMatch.OpponentName = bot.name;

            _lastWasGhost = false; // Запоминаем, что сейчас был Бот

            Debug.Log(
                $"[OnlineMatchService] Соперник: БОТ '{bot.name}'. " +
                $"Счёт: {bot.score} за {bot.time:F1}s"
            );
        }

        // Ставим наш специальный онлайн-пресет перед загрузкой сцены
        GameSessionSettings.SelectedPreset = GameSessionSettings.OnlinePreset;

        // Загружаем игровую сцену
        SceneManager.LoadScene(GameSessionSettings.GameSceneName);
    }

    // ... дальше идет метод FinishMatch, его не трогаем ...

    // Метод, вызываемый в конце игры
    // ЗАВЕРШЕНИЕ МАТЧА
    // ЗАВЕРШЕНИЕ МАТЧА
    public static void FinishMatch(int pScore, float pTime)
    {
        if (CurrentMatch == null) return;

        CurrentMatch.PlayerScore = pScore;
        CurrentMatch.PlayerTime = pTime;

        // === ЛОГИКА ПОБЕДИТЕЛЯ (с учетом ничьи) ===
        if (pScore > CurrentMatch.OpponentScore)
        {
            CurrentMatch.Result = MatchResult.Win;
        }
        else if (pScore < CurrentMatch.OpponentScore)
        {
            CurrentMatch.Result = MatchResult.Lose;
        }
        else
        {
            // Очки равны — проверяем разницу во времени
            float timeDiff = Mathf.Abs(pTime - CurrentMatch.OpponentTime);

            // Если разница 1 секунда или меньше — это НИЧЬЯ!
            if (timeDiff <= 1f)
            {
                CurrentMatch.Result = MatchResult.Draw;

                // === ХИТРЫЙ ТРЮК ===
                // Чтобы на экране не было нелепых цифр (например, 00:11 vs 00:12 при ничьей),
                // мы просто копируем твоё время боту!
                // Игрок увидит, что вы закончили миллисекунда в миллисекунду.
                CurrentMatch.OpponentTime = pTime;
            }
            else
            {
                // Иначе побеждает тот, у кого время меньше
                CurrentMatch.Result = pTime < CurrentMatch.OpponentTime ? MatchResult.Win : MatchResult.Lose;
            }
        }

        // Если победил — начисляем победу в статистику
        if (CurrentMatch.Result == MatchResult.Win) OnlineWinsService.AddWin();

        // Пытаемся обновить свой рекорд "Призрака"
        GhostRecordService.TrySaveNewGhost(pScore, pTime);
    }
    // --------------------------------------------------------
    // Сброс матча (вызвать при выходе в меню)
    // --------------------------------------------------------
    public static void ClearMatch()
    {
        CurrentMatch = null;
    }
}
