using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Этот компонент отвечает за ОДНУ карточку этапа в меню.
//
// То есть если у тебя в меню 8 этапов,
// то на каждой карточке будет висеть этот скрипт.
//
// Он умеет:
// 1) хранить ссылку на свой preset,
// 2) показывать название этапа,
// 3) показывать статус (закрыт / открыт / пройден / perfect),
// 4) показывать лучший счёт и лучшее время,
// 5) краситься в нужный цвет,
// 6) показывать tooltip для закрытого этапа,
// 7) сообщать LevelSelectController, что по нему нажали.
public class CampaignStageButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Main refs")]
    [SerializeField] private Button button;
    // Ссылка на кнопку этой карточки.
    // Именно на неё игрок нажимает.

    [SerializeField] private Image backgroundImage;
    // Фон карточки, который будем перекрашивать
    // в зависимости от состояния этапа.

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    // Название этапа.
    // Например: "Лёгкий 1"

    [SerializeField] private TMP_Text statusText;
    // Текст состояния этапа.
    // Например:
    // "Закрыт"
    // "Открыт"
    // "Пройден"
    // "Идеально"

    [SerializeField] private TMP_Text bestScoreText;
    // Лучший счёт на этом этапе.

    [SerializeField] private TMP_Text bestTimeText;
    // Лучшее время прохождения этапа.

    [Header("Optional icons")]
    [SerializeField] private GameObject lockIcon;
    // Иконка замка для закрытого этапа.

    [SerializeField] private GameObject perfectIcon;
    // Иконка / бейдж perfect.
    // Включается, если этап пройден идеально.

    [SerializeField] private GameObject timerIcon;
    // Иконка таймера.
    // Включается, если на этом этапе useTimer = true.

    [Header("Tooltip")]
    [SerializeField] private TooltipUI tooltip;
    // Tooltip для показа причины блокировки.

    [SerializeField] private Vector2 tooltipOffset = new Vector2(140f, 60f);
    // Смещение tooltip относительно карточки.

    [SerializeField] private float tooltipDelay = 0.08f;
    // Небольшая задержка показа tooltip, чтобы он не мигал.

    [Header("Colors")]
    [SerializeField] private Color lockedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
    // Цвет закрытого этапа.

    [SerializeField] private Color unlockedColor = Color.white;
    // Цвет открытого, но ещё не пройденного этапа.

    [SerializeField] private Color completedColor = new Color(0.82f, 0.95f, 0.82f, 1f);
    // Цвет пройденного этапа.

    [SerializeField] private Color perfectColor = new Color(1f, 0.93f, 0.62f, 1f);
    // Цвет perfect-этапа.

    [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.70f, 0.15f, 1f);
    // Это не фон, а цвет outline/обводки,
    // если ты захочешь использовать Outline или отдельную рамку.

    [Header("Optional selection frame")]
    [SerializeField] private Graphic selectionFrame;
    // Дополнительная рамка вокруг карточки.
    // Если её нет — не страшно.
    // Но если есть, мы будем включать/выключать её,
    // чтобы было видно выбранный этап.

    // Preset, который относится к этой карточке.
    private QuizDifficultyPreset _preset;

    // Ссылка на контроллер списка этапов.
    private LevelSelectController _owner;

    // Корутина показа tooltip.
    private Coroutine _tooltipRoutine;

    // Флаг: курсор находится над карточкой.
    private bool _pointerInside;

    // -------------------------
    // ПРИВЯЗКА ДАННЫХ
    // -------------------------

    // Этот метод вызывает LevelSelectController,
    // когда раздаёт карточкам соответствующие этапы кампании.
    public void Setup(LevelSelectController owner, QuizDifficultyPreset preset)
    {
        _owner = owner;
        _preset = preset;

        Refresh();
    }

    // Публичный getter, если потом понадобится снаружи.
    public QuizDifficultyPreset GetPreset()
    {
        return _preset;
    }

    // -------------------------
    // ОБНОВЛЕНИЕ ВИЗУАЛА
    // -------------------------

    public void Refresh()
    {
        // Если preset ещё не назначен —
        // скрываем базовые данные.
        if (_preset == null)
        {
            if (titleText != null) titleText.text = "No Stage";
            if (statusText != null) statusText.text = string.Empty;
            if (bestScoreText != null) bestScoreText.text = string.Empty;
            if (bestTimeText != null) bestTimeText.text = string.Empty;
            if (lockIcon != null) lockIcon.SetActive(false);
            if (perfectIcon != null) perfectIcon.SetActive(false);
            if (timerIcon != null) timerIcon.SetActive(false);
            if (selectionFrame != null) selectionFrame.gameObject.SetActive(false);
            return;
        }

        // Узнаём текущий язык.
        bool useEnglish = LanguageService.UseEnglish;

        // Узнаём состояние этапа.
        bool unlocked = LevelUnlockService.IsUnlocked(_preset);
        bool completed = StageProgressService.IsCompleted(_preset);
        bool perfect = StageProgressService.IsPerfectCompleted(_preset);
        bool selected = (GameSessionSettings.SelectedPreset == _preset);

        // ---------- Название этапа ----------
        if (titleText != null)
        {
            titleText.text = _preset.GetDisplayName(useEnglish);
        }

        // ---------- Лучший счёт ----------
        if (bestScoreText != null)
        {
            int bestScore = StageProgressService.GetBestScore(_preset);

            // Если ещё не играли — показываем понятный текст.
            if (bestScore <= 0)
            {
                bestScoreText.text = useEnglish
                    ? "Best score: —"
                    : "Лучший счёт: —";
            }
            else
            {
                bestScoreText.text = useEnglish
                    ? $"Best score: {bestScore}/{_preset.QuestionsCount}"
                    : $"Лучший счёт: {bestScore}/{_preset.QuestionsCount}";
            }
        }

        // ---------- Лучшее время ----------
        if (bestTimeText != null)
        {
            float bestTime = StageProgressService.GetBestTime(_preset);

            if (bestTime <= 0f)
            {
                bestTimeText.text = useEnglish
                    ? "Best time: —"
                    : "Лучшее время: —";
            }
            else
            {
                bestTimeText.text = useEnglish
                    ? $"Best time: {FormatTime(bestTime)}"
                    : $"Лучшее время: {FormatTime(bestTime)}";
            }
        }

        // ---------- Текст статуса ----------
        if (statusText != null)
        {
            if (!unlocked)
            {
                statusText.text = useEnglish ? "Locked" : "Закрыт";
            }
            else if (perfect)
            {
                statusText.text = useEnglish ? "Perfect" : "Идеально";
            }
            else if (completed)
            {
                statusText.text = useEnglish ? "Completed" : "Пройден";
            }
            else
            {
                statusText.text = useEnglish ? "Open" : "Открыт";
            }
        }

        // ---------- Иконки ----------
        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (perfectIcon != null)
            perfectIcon.SetActive(perfect);

        if (timerIcon != null)
            timerIcon.SetActive(_preset.UseTimer);

        // ---------- Цвет фона ----------
        if (backgroundImage != null)
        {
            if (!unlocked)
                backgroundImage.color = lockedColor;
            else if (perfect)
                backgroundImage.color = perfectColor;
            else if (completed)
                backgroundImage.color = completedColor;
            else
                backgroundImage.color = unlockedColor;
        }

        // ---------- Рамка выбранного этапа ----------
        if (selectionFrame != null)
        {
            selectionFrame.gameObject.SetActive(selected);
            selectionFrame.color = selectedOutlineColor;
        }

        // ВАЖНО:
        // кнопку НЕ отключаем через interactable = false,
        // потому что тогда может перестать удобно работать tooltip.
        // Вместо этого просто блокируем выбор в коде LevelSelectController.
        if (button != null)
            button.interactable = true;
    }

    // -------------------------
    // НАЖАТИЕ НА КАРТОЧКУ
    // -------------------------

    // Этот метод нужно повесить на OnClick кнопки карточки.
    public void OnPressed()
    {
        // Если preset или owner не назначены — выходим.
        if (_preset == null || _owner == null)
            return;

        // Передаём контроллеру выбранный этап.
        _owner.SelectPreset(_preset);
    }

    // -------------------------
    // TOOLTIP ДЛЯ ЗАКРЫТОГО ЭТАПА
    // -------------------------

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_preset == null)
            return;

        if (tooltip == null)
            return;

        // Если этап открыт — tooltip не нужен.
        if (LevelUnlockService.IsUnlocked(_preset))
            return;

        _pointerInside = true;

        if (_tooltipRoutine != null)
            StopCoroutine(_tooltipRoutine);

        _tooltipRoutine = StartCoroutine(ShowTooltipDelayed());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerInside = false;

        if (_tooltipRoutine != null)
        {
            StopCoroutine(_tooltipRoutine);
            _tooltipRoutine = null;
        }

        if (tooltip != null)
            tooltip.Hide();
    }

    private System.Collections.IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSecondsRealtime(tooltipDelay);

        if (!_pointerInside)
            yield break;

        bool useEnglish = LanguageService.UseEnglish;

        // Получаем понятную причину блокировки из LevelUnlockService.
        string message = LevelUnlockService.GetLockedReason(_preset, useEnglish);

        RectTransform anchor = transform as RectTransform;
        tooltip.ShowNear(anchor, message, tooltipOffset);

        _tooltipRoutine = null;
    }

    // -------------------------
    // ФОРМАТ ВРЕМЕНИ
    // -------------------------

    private string FormatTime(float seconds)
    {
        // Защита от отрицательных значений
        seconds = Mathf.Max(0f, seconds);

        // Переводим float-секунды в минуты и секунды
        int totalSeconds = Mathf.RoundToInt(seconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;

        // Формат:
        // 75 секунд -> 01:15
        return $"{minutes:00}:{secs:00}";
    }
}