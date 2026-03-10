using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PatchSlotTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Slot info")]
    [SerializeField] private AchievementId achievementId;
    [SerializeField] private string ruHint = "Пройди уровень ...";
    [SerializeField] private string enHint = "Complete level ...";

    [Header("Refs")]
    [SerializeField] private TooltipUI tooltip;

    [Header("Anti-flicker")]
    [SerializeField] private float showDelay = 0.08f; // задержка перед показом

    private Coroutine _showRoutine;
    private bool _pointerInside;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AchievementService.IsUnlocked(achievementId)) return;
        if (tooltip == null) return;

        _pointerInside = true;

        // Запускаем задержку
        if (_showRoutine != null) StopCoroutine(_showRoutine);
        _showRoutine = StartCoroutine(ShowDelayed(eventData));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerInside = false;

        // Если ещё не показали — отменяем
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        if (tooltip != null)
            tooltip.Hide();
    }

    private IEnumerator ShowDelayed(PointerEventData eventData)
    {
        yield return new WaitForSecondsRealtime(showDelay);
        if (!_pointerInside) yield break;

        bool en = LanguageService.UseEnglish;
        string msg = en ? enHint : ruHint;

        RectTransform slotRect = transform as RectTransform;
        tooltip.ShowNear(slotRect, msg, new Vector2(140f, 60f));

        _showRoutine = null;
    }
}