
// Ётот enum описывает все нашивки в игре.
// “еперь у нас 10 нашивок:
//
// 1-8  -> за perfect-прохождение каждого этапа
// 9    -> за полное прохождение кампании
// 10   -> за идеальное прохождение всей кампании
using System;
[Serializable]
public enum AchievementId
{
    


    PerfectEasy1,
    PerfectEasy2,
    PerfectMedium1,
    PerfectMedium2,
    PerfectHard1,
    PerfectHard2,
    PerfectExpert1,
    PerfectExpert2,

    CompleteCampaign,
    PerfectCampaign
}