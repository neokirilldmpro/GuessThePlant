using UnityEngine;
using UnityEngine.UI;

public class PortfolioView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root; // PortfolioPanel

    [Header("Slots (same order as AchievementId enum)")]
    [SerializeField] private Image[] slotImages; // 4 слота

    [Header("Patch Sprites")]
    [SerializeField] private Sprite patchEasy;
    [SerializeField] private Sprite patchMedium;
    [SerializeField] private Sprite patchHard;
    [SerializeField] private Sprite patchMaxHard;

    [Header("Locked Appearance")]
    [SerializeField, Range(0f, 1f)] private float lockedAlpha = 0.25f;


    [SerializeField] private GameObject mainMenuPanel;

    private void Start()
    {
        Hide();
        Refresh();
    }

    public void Show()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
 
        if (root != null) root.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void Refresh()
    {
        if (slotImages == null || slotImages.Length == 0) return;

        ApplySlot(0, AchievementId.CompleteEasy, patchEasy);
        ApplySlot(1, AchievementId.CompleteMedium, patchMedium);
        ApplySlot(2, AchievementId.CompleteHard, patchHard);
        ApplySlot(3, AchievementId.CompleteMaxHard, patchMaxHard);
    }

    private void ApplySlot(int index, AchievementId id, Sprite patch)
    {
        if (index < 0 || index >= slotImages.Length) return;

        Image img = slotImages[index];
        if (img == null) return;

        bool unlocked = AchievementService.IsUnlocked(id);

        // Если разблокировано — показываем нашивку
        if (unlocked && patch != null)
        {
            img.sprite = patch;
            var c = img.color; c.a = 1f; img.color = c;
        }
        else
        {
            // Если не разблокировано — либо пустой слот (sprite = null), либо “замок”
            // Тут оставим sprite как есть, но сделаем полупрозрачным
            var c = img.color; c.a = lockedAlpha; img.color = c;
        }
    }
}