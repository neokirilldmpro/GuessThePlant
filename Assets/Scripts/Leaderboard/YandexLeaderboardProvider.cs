// ============================================================
// YandexLeaderboardProvider.cs
// ------------------------------------------------------------
// –еализаци€ ILeaderboardProvider дл€ платформы яндекс »гры.
//
// ¬ј∆Ќќ:
//   ¬есь код обЄрнут в #if YandexGamesPlatform_yg.
//   Ёто define, который PluginYG2 добавл€ет автоматически
//   при выборе платформы Yandex в Basic Settings.
//   ¬ билде дл€ CrazyGames этот код не скомпилируетс€ вообще.
//
// ѕќƒ Ћё„≈Ќ»≈:
//   Ётот класс создаЄтс€ в LeaderboardService.CreateProvider().
//   ¬ручную создавать не нужно.
// ============================================================
#if YandexGamesPlatform_yg

using YG;
using UnityEngine;

public class YandexLeaderboardProvider : ILeaderboardProvider
{
    // --------------------------------------------------------
    // Ћидерборд доступен только если игрок авторизован на яндексе.
    // YG2.player.auth Ч bool из PluginYG2 (true = авторизован).
    // --------------------------------------------------------
    public bool IsAvailable => YG2.player.auth;

    // --------------------------------------------------------
    // ќтправл€ем результат через PluginYG2.
    //
    // YG2.SetLeaderboard(name, score):
    //   - name  Ч техническое название лидерборда из консоли яндекса
    //   - score Ч int, дл€ типа "time" Ч в ћ»ЋЋ»—≈ ”Ќƒј’
    //
    // яндекс сам обновл€ет запись только если новый результат
    // Ћ”„Ў≈ предыдущего (дл€ time Ч меньше, дл€ numeric Ч больше).
    // --------------------------------------------------------
    public void SubmitScore(string leaderboardName, int score)
    {
        if (!IsAvailable)
        {
            Debug.LogWarning(
                $"[YandexLeaderboardProvider] »грок не авторизован. " +
                $"–езультат '{leaderboardName}' = {score} не отправлен."
            );
            return;
        }

        Debug.Log(
            $"[YandexLeaderboardProvider] ќтправл€ем в лидерборд " +
            $"'{leaderboardName}': {score}"
        );

        YG2.SetLeaderboard(leaderboardName, score);
    }
}

#endif