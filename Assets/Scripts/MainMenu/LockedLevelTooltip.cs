using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Этот скрипт показывает tooltip, если игрок навёл курсор
// на закрытый уровень в меню выбора сложности.
public class LockedLevelTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Target level")]
    [SerializeField] private QuizDifficultyPreset targetPreset;
    // Сюда нужно поставить тот preset, к которому относится именно эта кнопка.
    // Например:
    // кнопка Medium -> mediumPreset
    // кнопка Hard   -> hardPreset

    [Header("Tooltip ref")]
    [SerializeField] private TooltipUI tooltip;
    // Ссылка на TooltipUI из сцены

    [Header("Tooltip text")]
    [SerializeField] private string ruHint = "Для доступа к этому уровню нужно пройти предыдущий.";
    [SerializeField] private string enHint = "To access this level, complete the previous one first.";
    // Тексты подсказки на двух языках

    [Header("Anti-flicker")]
    [SerializeField] private float showDelay = 0.08f;
    // Небольшая задержка, чтобы tooltip не мигал слишком резко

    [SerializeField] private Vector2 offset = new Vector2(140f, 60f);
    // Смещение tooltip относительно кнопки

    private Coroutine _showRoutine;
    // Ссылка на корутину, чтобы можно было её остановить

    private bool _pointerInside;
    // Флаг: находится ли курсор над кнопкой прямо сейчас

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Если preset не назначен — работать нельзя
        if (targetPreset == null)
            return;

        // Если tooltip не назначен — показывать нечего
        if (tooltip == null)
            return;

        // Если уровень уже открыт, подсказка не нужна
        if (LevelUnlockService.IsUnlocked(targetPreset))
            return;

        // Запоминаем, что курсор внутри
        _pointerInside = true;

        // Если раньше уже была корутина показа,
        // останавливаем её, чтобы не создать дубли
        if (_showRoutine != null)
            StopCoroutine(_showRoutine);

        // Запускаем корутину с небольшой задержкой
        _showRoutine = StartCoroutine(ShowDelayed());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Курсор ушёл
        _pointerInside = false;

        // Если корутина ещё не успела показать tooltip,
        // останавливаем её
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        // И прячем tooltip, если он уже показан
        if (tooltip != null)
            tooltip.Hide();
    }

    private IEnumerator ShowDelayed()
    {
        // Ждём немного в реальном времени
        // Realtime важно, чтобы tooltip не зависел от Time.timeScale
        yield return new WaitForSecondsRealtime(showDelay);

        // Если за это время курсор уже ушёл — ничего не показываем
        if (!_pointerInside)
            yield break;

        // Определяем текущий язык
        bool en = LanguageService.UseEnglish;

        // Выбираем нужный текст
        string message = en ? enHint : ruHint;

        // Берём RectTransform текущей кнопки,
        // чтобы показать tooltip рядом с ней
        RectTransform anchor = transform as RectTransform;

        // Показываем tooltip возле кнопки
        tooltip.ShowNear(anchor, message, offset);

        // Корутина завершилась — очищаем ссылку
        _showRoutine = null;
    }
}