using UnityEngine;

// Ётот ScriptableObject хранит спрайты всех нашивок.
// “еперь у нас не 4 нашивки, а 10:
//
// 1-8  -> за идеальное прохождение каждого из 8 этапов
// 9    -> за прохождение всей кампании
// 10   -> за идеальное прохождение всей кампании
[CreateAssetMenu(menuName = "Quiz/Achievement Visuals", fileName = "AchievementVisuals")]
public class AchievementVisuals : ScriptableObject
{
    [Header("Perfect stage patches")]
    public Sprite patchPerfectEasy1;
    public Sprite patchPerfectEasy2;
    public Sprite patchPerfectMedium1;
    public Sprite patchPerfectMedium2;
    public Sprite patchPerfectHard1;
    public Sprite patchPerfectHard2;
    public Sprite patchPerfectExpert1;
    public Sprite patchPerfectExpert2;

    [Header("Campaign patches")]
    public Sprite patchCompleteCampaign;
    public Sprite patchPerfectCampaign;

    // Ётот метод возвращает нужный спрайт по ID достижени€.
    public Sprite GetPatchSprite(AchievementId id)
    {
        switch (id)
        {
            case AchievementId.PerfectEasy1:
                return patchPerfectEasy1;

            case AchievementId.PerfectEasy2:
                return patchPerfectEasy2;

            case AchievementId.PerfectMedium1:
                return patchPerfectMedium1;

            case AchievementId.PerfectMedium2:
                return patchPerfectMedium2;

            case AchievementId.PerfectHard1:
                return patchPerfectHard1;

            case AchievementId.PerfectHard2:
                return patchPerfectHard2;

            case AchievementId.PerfectExpert1:
                return patchPerfectExpert1;

            case AchievementId.PerfectExpert2:
                return patchPerfectExpert2;

            case AchievementId.CompleteCampaign:
                return patchCompleteCampaign;

            case AchievementId.PerfectCampaign:
                return patchPerfectCampaign;

            default:
                return null;
        }
    }
}