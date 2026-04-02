// ============================================================
// LeaderboardService.cs
// ------------------------------------------------------------
// Единая точка входа для работы с лидербордами.
//
// ИСПОЛЬЗОВАНИЕ из любого места в игре:
//   LeaderboardService.Instance.SubmitTotalTime();
//   LeaderboardService.Instance.SubmitOnlineWins(wins); // Этап 4
//
// КАК РАБОТАЕТ:
//   При первом обращении сам создаёт нужный провайдер
//   в зависимости от платформы (через #if defines).
//
// ТЕХНИЧЕСКИЕ НАЗВАНИЯ ЛИДЕРБОРДОВ (задаются в консоли Яндекса):
//   "totalBestTime" — суммарное лучшее время (тип: time, сортировка по возрастанию)
//   "onlineWins"    — победы в псевдо-онлайн (тип: numeric, по убыванию)
// ============================================================
using UnityEngine;

public class LeaderboardService
{
    // --------------------------------------------------------
    // Singleton — ленивая инициализация без MonoBehaviour.
    // Не нужно размещать на сцене.
    // --------------------------------------------------------
    private static LeaderboardService _instance;
    public static LeaderboardService Instance
    {
        get
        {
            if (_instance == null)
                _instance = new LeaderboardService();
            return _instance;
        }
    }

    // Текущий провайдер (Яндекс или Заглушка).
    private readonly ILeaderboardProvider _provider;

    // Технические названия лидербордов — одно место для изменений.
    // Если переименуешь в консоли Яндекса — меняешь только здесь.
    private const string BOARD_TOTAL_TIME = "totalBestTime";
    private const string BOARD_ONLINE_WINS = "onlineWins";     // Этап 4

    // --------------------------------------------------------
    // Конструктор — создаём нужный провайдер.
    // Благодаря #if — в билде окажется только один из них.
    // --------------------------------------------------------
    private LeaderboardService()
    {
        _provider = CreateProvider();

        Debug.Log(
            $"[LeaderboardService] Инициализирован. " +
            $"Провайдер: {_provider.GetType().Name}, " +
            $"Доступен: {_provider.IsAvailable}"
        );
    }

    // --------------------------------------------------------
    // Фабричный метод выбора провайдера.
    // ДОБАВЛЕНИЕ CrazyGames:
    //   Добавь ветку #elif CrazyGamesPlatform_yg
    //   и верни new CrazyGamesLeaderboardProvider()
    // --------------------------------------------------------
    private static ILeaderboardProvider CreateProvider()
    {
#if YandexGamesPlatform_yg
        return new YandexLeaderboardProvider();
#else
        // Заглушка для редактора и других платформ.
        return new StubLeaderboardProvider();
#endif
    }

    // --------------------------------------------------------
    // МЕТОД 1: Отправить суммарное лучшее время.
    //
    // Вызывается из QuizGameController.FinishQuiz()
    // когда пройден ПОСЛЕДНИЙ этап кампании.
    //
    // Логика:
    //   1. Берём все этапы кампании
    //   2. Проверяем что ВСЕ пройдены (иначе сумма неполная)
    //   3. Суммируем лучшее время каждого этапа
    //   4. Переводим в миллисекунды и отправляем
    // --------------------------------------------------------
    public void SubmitTotalBestTime()
    {
        // Получаем все этапы кампании.
        QuizDifficultyPreset[] stages = GameSessionSettings.GetAllCampaignStages();

        if (stages == null || stages.Length == 0)
        {
            Debug.LogWarning("[LeaderboardService] Этапы кампании не найдены.");
            return;
        }

        // Проверяем что ВСЕ этапы пройдены.
        // Нет смысла отправлять частичную сумму.
        if (!StageProgressService.AreAllCompleted(stages))
        {
            Debug.Log(
                "[LeaderboardService] Не все этапы пройдены — " +
                "суммарное время не отправляем."
            );
            return;
        }

        // Суммируем лучшее время по каждому этапу.
        float totalSeconds = 0f;
        bool hasAllTimes = true;

        foreach (var stage in stages)
        {
            float bestTime = StageProgressService.GetBestTime(stage);

            if (bestTime <= 0f)
            {
                // У этого этапа нет записанного времени — пропускаем отправку.
                hasAllTimes = false;
                Debug.LogWarning(
                    $"[LeaderboardService] Нет времени для этапа " +
                    $"'{stage.StageKey}'. Отправка отменена."
                );
                break;
            }

            totalSeconds += bestTime;
        }

        if (!hasAllTimes)
            return;

        // Яндекс принимает время в МИЛЛИСЕКУНДАХ (целое число).
        int totalMs = Mathf.RoundToInt(totalSeconds * 1000f);

        Debug.Log(
            $"[LeaderboardService] Суммарное лучшее время: " +
            $"{totalSeconds:F1}s = {totalMs}ms. Отправляем в '{BOARD_TOTAL_TIME}'."
        );

        _provider.SubmitScore(BOARD_TOTAL_TIME, totalMs);
    }

    // --------------------------------------------------------
    // МЕТОД 2: Отправить количество побед в псевдо-онлайн.
    // ЭТАП 4 — пока не используется, но место уже готово.
    // --------------------------------------------------------
    public void SubmitOnlineWins(int wins)
    {
        if (wins < 0)
        {
            Debug.LogWarning("[LeaderboardService] Победы не могут быть < 0.");
            return;
        }

        Debug.Log(
            $"[LeaderboardService] Победы в онлайн: {wins}. " +
            $"Отправляем в '{BOARD_ONLINE_WINS}'."
        );

        _provider.SubmitScore(BOARD_ONLINE_WINS, wins);
    }
}