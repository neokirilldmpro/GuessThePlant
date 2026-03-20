using System;
using UnityEngine;
using UnityEngine.UI;

// Мини-превью нашивок на кнопке рюкзака.
// Теперь тоже автоматически обновляется при открытии новой нашивки.
public class PortfolioButtonPreview : MonoBehaviour
{
    [Header("Mini slots")]
    [SerializeField] private Image[] miniSlots;

    [Header("Visual source")]
    [SerializeField] private AchievementVisuals achievementVisuals;

    [Header("Locked appearance")]
    [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.25f;

    private Sprite[] _defaultMiniSprites;

    private void Awake()
    {
        CacheDefaultSprites();
    }

    private void OnEnable()
    {
        AchievementService.AchievementUnlocked += OnAchievementUnlocked;
        Refresh();
    }

    private void OnDisable()
    {
        AchievementService.AchievementUnlocked -= OnAchievementUnlocked;
    }

    private void Start()
    {
        Refresh();
    }

    private void CacheDefaultSprites()
    {
        if (miniSlots == null)
            return;

        _defaultMiniSprites = new Sprite[miniSlots.Length];

        for (int i = 0; i < miniSlots.Length; i++)
        {
            if (miniSlots[i] != null)
                _defaultMiniSprites[i] = miniSlots[i].sprite;
        }
    }

    private void OnAchievementUnlocked(AchievementId id)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (miniSlots == null || miniSlots.Length == 0)
            return;

        if (achievementVisuals == null)
        {
            Debug.LogWarning("[PortfolioButtonPreview] AchievementVisuals is not assigned.");
            return;
        }

        AchievementId[] allIds = (AchievementId[])Enum.GetValues(typeof(AchievementId));

        int count = Mathf.Min(miniSlots.Length, allIds.Length);

        for (int i = 0; i < count; i++)
        {
            ApplySlot(i, allIds[i]);
        }

        for (int i = count; i < miniSlots.Length; i++)
        {
            ResetSlotToDefault(i);
        }
    }

    private void ApplySlot(int index, AchievementId id)
    {
        if (index < 0 || index >= miniSlots.Length)
            return;

        Image img = miniSlots[index];
        if (img == null)
            return;

        bool unlocked = AchievementService.IsUnlocked(id);
        Sprite patchSprite = achievementVisuals.GetPatchSprite(id);

        if (unlocked && patchSprite != null)
        {
            img.sprite = patchSprite;

            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }
        else
        {
            if (_defaultMiniSprites != null && index < _defaultMiniSprites.Length)
                img.sprite = _defaultMiniSprites[index];

            Color c = img.color;
            c.a = lockedAlpha;
            img.color = c;
        }
    }

    private void ResetSlotToDefault(int index)
    {
        if (index < 0 || index >= miniSlots.Length)
            return;

        Image img = miniSlots[index];
        if (img == null)
            return;

        if (_defaultMiniSprites != null && index < _defaultMiniSprites.Length)
            img.sprite = _defaultMiniSprites[index];

        Color c = img.color;
        c.a = lockedAlpha;
        img.color = c;
    }
}