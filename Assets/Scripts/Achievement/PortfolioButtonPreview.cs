using UnityEngine;
using UnityEngine.UI;

public class PortfolioButtonPreview : MonoBehaviour
{
    [Header("Mini slots on the button (same order as AchievementId)")]
    [SerializeField] private Image[] miniSlots; // 4 мини-слота на кнопке

    [Header("Patch sprites (same as in portfolio)")]
    [SerializeField] private Sprite patchEasy;
    [SerializeField] private Sprite patchMedium;
    [SerializeField] private Sprite patchHard;
    [SerializeField] private Sprite patchMaxHard;

    [Header("Locked appearance")]
    [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.25f;

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
        if (miniSlots == null || miniSlots.Length == 0) return;

        Apply(0, AchievementId.CompleteEasy, patchEasy);
        Apply(1, AchievementId.CompleteMedium, patchMedium);
        Apply(2, AchievementId.CompleteHard, patchHard);
        Apply(3, AchievementId.CompleteMaxHard, patchMaxHard);
    }

    private void Apply(int index, AchievementId id, Sprite sprite)
    {
        if (index < 0 || index >= miniSlots.Length) return;

        Image img = miniSlots[index];
        if (img == null) return;

        bool unlocked = AchievementService.IsUnlocked(id);

        if (unlocked && sprite != null)
        {
            img.sprite = sprite;
            var c = img.color; c.a = 1f; img.color = c;
        }
        else
        {
            // Оставляем базовую рамку/пустой слот (если есть), либо просто глушим альфой
            var c = img.color; c.a = lockedAlpha; img.color = c;
        }
    }
}