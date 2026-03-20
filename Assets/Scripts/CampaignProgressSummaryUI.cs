using System;
using TMPro;
using UnityEngine;

// Этот компонент отвечает за верхнюю сводку прогресса кампании.
//
// Он показывает:
// 1) сколько этапов пройдено,
// 2) сколько этапов пройдено идеально,
// 3) сколько нашивок уже открыто.
public class CampaignProgressSummaryUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text completedStagesText;
    // Например:
    // "Пройдено этапов: 3/8"

    [SerializeField] private TMP_Text perfectStagesText;
    // Например:
    // "Идеально: 1/8"

    [SerializeField] private TMP_Text badgesText;
    // Например:
    // "Нашивки: 4/10"

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool useEnglish = LanguageService.UseEnglish;

        // Берём все этапы кампании.
        QuizDifficultyPreset[] stages = GameSessionSettings.GetAllCampaignStages();

        int totalStages = stages != null ? stages.Length : 0;
        int completedCount = StageProgressService.GetCompletedCount(stages);
        int perfectCount = StageProgressService.GetPerfectCount(stages);

        // Считаем количество открытых нашивок.
        int unlockedAchievements = CountUnlockedAchievements();
        int totalAchievements = Enum.GetValues(typeof(AchievementId)).Length;

        if (completedStagesText != null)
        {
            completedStagesText.text = useEnglish
                ? $"Completed stages: {completedCount}/{totalStages}"
                : $"Пройдено этапов: {completedCount}/{totalStages}";
        }

        if (perfectStagesText != null)
        {
            perfectStagesText.text = useEnglish
                ? $"Perfect stages: {perfectCount}/{totalStages}"
                : $"Идеально: {perfectCount}/{totalStages}";
        }

        if (badgesText != null)
        {
            badgesText.text = useEnglish
                ? $"Badges: {unlockedAchievements}/{totalAchievements}"
                : $"Нашивки: {unlockedAchievements}/{totalAchievements}";
        }
    }

    private int CountUnlockedAchievements()
    {
        int count = 0;

        AchievementId[] allIds = (AchievementId[])Enum.GetValues(typeof(AchievementId));

        for (int i = 0; i < allIds.Length; i++)
        {
            if (AchievementService.IsUnlocked(allIds[i]))
                count++;
        }

        return count;
    }
}