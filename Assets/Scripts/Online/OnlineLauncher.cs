using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using YG; // Обязательно добавляем для связи с плагином PluginYG2

[RequireComponent(typeof(Button))]
public class OnlineLauncher : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Settings")]
    [SerializeField] private TooltipUI tooltip;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(140f, 60f);
    [SerializeField] private float showDelay = 0.08f;

    [Header("Texts")]
    [SerializeField] private string hintRu = "Сначала пройдите все этапы кампании";
    [SerializeField] private string hintEn = "Complete all campaign stages first";

    private Button _button;
    private Coroutine _showRoutine;
    private bool _pointerInside;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        CheckUnlockCondition();
        _pointerInside = false;

        // Магия здесь: подписываемся на событие загрузки облачных сохранений Яндекса
        YG2.onGetSDKData += CheckUnlockCondition;
    }

    private void OnDisable()
    {
        if (tooltip != null) tooltip.Hide();

        // Обязательно отписываемся, чтобы не было ошибок при переходе между сценами
        YG2.onGetSDKData -= CheckUnlockCondition;
    }

    private void CheckUnlockCondition()
    {
        if (_button == null) return;

        QuizDifficultyPreset[] stages = GameSessionSettings.GetAllCampaignStages();
        bool allCompleted = false;

        if (stages != null && stages.Length > 0)
        {
            allCompleted = StageProgressService.AreAllCompleted(stages);
        }

        _button.interactable = allCompleted;
    }

    // Вызов матча (остается как было)
    public void Launch()
    {
        OnlineMatchService.StartMatch();
    }

    // === Тултип ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.interactable) return;
        if (tooltip == null) return;

        _pointerInside = true;
        if (_showRoutine != null) StopCoroutine(_showRoutine);

        _showRoutine = StartCoroutine(ShowDelayed());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerInside = false;
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }
        if (tooltip != null) tooltip.Hide();
    }

    private IEnumerator ShowDelayed()
    {
        yield return new WaitForSecondsRealtime(showDelay);
        if (!_pointerInside) yield break;

        string msg = LanguageService.UseEnglish ? hintEn : hintRu;
        RectTransform anchor = transform as RectTransform;
        tooltip.ShowNear(anchor, msg, tooltipOffset);

        _showRoutine = null;
    }
}