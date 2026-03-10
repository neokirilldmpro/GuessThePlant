using UnityEngine;

[CreateAssetMenu(menuName = "Quiz/Achievement Visuals", fileName = "AchievementVisuals")]
public class AchievementVisuals : ScriptableObject
{
    public Sprite patchEasy;
    public Sprite patchMedium;
    public Sprite patchHard;
    public Sprite patchMaxHard;

    public Sprite GetPatchSprite(AchievementId id)
    {
        switch (id)
        {
            case AchievementId.CompleteEasy: return patchEasy;
            case AchievementId.CompleteMedium: return patchMedium;
            case AchievementId.CompleteHard: return patchHard;
            case AchievementId.CompleteMaxHard: return patchMaxHard;
            default: return null;
        }
    }
}