// ============================================================
// StubLeaderboardProvider.cs
// ------------------------------------------------------------
// Заглушка-провайдер для платформ БЕЗ лидерборда.
// Используется:
//   - В Unity Editor (для тестирования без краша)
//   - На CrazyGames (пока нет своего провайдера)
//   - На любой другой платформе, где лидерборд не реализован
//
// Ничего не отправляет — только логирует.
// Когда будешь добавлять CrazyGames — создай
// CrazyGamesLeaderboardProvider по аналогии с Yandex-версией.
// ============================================================
using UnityEngine;

public class StubLeaderboardProvider : ILeaderboardProvider
{
    // Заглушка всегда "недоступна" — не будет ошибок,
    // но и ничего не отправит.
    public bool IsAvailable => false;

    public void SubmitScore(string leaderboardName, int score)
    {
        // В редакторе выводим лог, чтобы видеть что "отправилось бы".
        Debug.Log(
            $"[StubLeaderboardProvider] (Заглушка) Лидерборд '{leaderboardName}'" +
            $" — результат {score}. На этой платформе лидерборд не реализован."
        );
    }
}