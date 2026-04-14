using System;
using UnityEngine;
using UnityEngine.UI;

// Этот компонент управляет большим портфелем достижений.
// Теперь он автоматически обновляется, когда открывается новая нашивка.
public class PortfolioView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Slots")]
    [SerializeField] private Image[] slotImages;
    // Сюда ставишь все слоты портфеля.
    // В идеале их должно быть 10.

    [Header("Visual source")]
    [SerializeField] private AchievementVisuals achievementVisuals;
    // Единый источник спрайтов нашивок.

    [Header("Locked Appearance")]
    [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.25f;

    [SerializeField] private GameObject mainMenuPanel;

    // Базовые спрайты пустых слотов, чтобы можно было вернуть их,
    // если достижение ещё не открыто.
    private Sprite[] _defaultSlotSprites;

    private void Awake()
    {
        CacheDefaultSprites();
    }

    private void OnEnable()
    {
        // Подписываемся на событие открытия достижения.
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
        if (slotImages == null)
            return;

        _defaultSlotSprites = new Sprite[slotImages.Length];

        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
                _defaultSlotSprites[i] = slotImages[i].sprite;
        }
    }

    private void OnAchievementUnlocked(AchievementId id)
    {
        // Как только открылась новая нашивка —
        // сразу перерисовываем портфель.
        Refresh();
    }

    public void Show()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (root != null)
            root.SetActive(true);

        Refresh();
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void Refresh()
    {
        if (slotImages == null || slotImages.Length == 0)
            return;

        if (achievementVisuals == null)
        {
            Debug.LogWarning("[PortfolioView] AchievementVisuals is not assigned.");
            return;
        }

        AchievementId[] allIds = (AchievementId[])Enum.GetValues(typeof(AchievementId));

        int count = Mathf.Min(slotImages.Length, allIds.Length);

        for (int i = 0; i < count; i++)
        {
            ApplySlot(i, allIds[i]);
        }

        // Если слотов больше, чем достижений —
        // лишние делаем пустыми.
        for (int i = count; i < slotImages.Length; i++)
        {
            ResetSlotToDefault(i);
        }
    }

    private void ApplySlot(int index, AchievementId id)
    {
        if (index < 0 || index >= slotImages.Length)
            return;

        Image img = slotImages[index];
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
            if (_defaultSlotSprites != null && index < _defaultSlotSprites.Length)
                img.sprite = _defaultSlotSprites[index];

            Color c = img.color;
            c.a = lockedAlpha;
            img.color = c;
        }
    }

    private void ResetSlotToDefault(int index)
    {
        if (index < 0 || index >= slotImages.Length)
            return;

        Image img = slotImages[index];
        if (img == null)
            return;

        if (_defaultSlotSprites != null && index < _defaultSlotSprites.Length)
            img.sprite = _defaultSlotSprites[index];

        Color c = img.color;
        c.a = lockedAlpha;
        img.color = c;
    }
}