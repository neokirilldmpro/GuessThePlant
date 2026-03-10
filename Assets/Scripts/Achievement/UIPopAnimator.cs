using System.Collections;
using UnityEngine;

// Простая pop-анимация UI объекта через scale (0 -> overshoot -> 1).
public class UIPopAnimator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private RectTransform target; // Что анимируем (обычно NewPatchRoot)

    [Header("Timing")]
    [SerializeField] private float inDuration = 0.18f;     // Время "вылета"
    [SerializeField] private float settleDuration = 0.12f; // Время "усадки"
    [SerializeField] private float overshootScale = 1.15f; // Насколько перелететь выше 1

    private Coroutine _routine;

    private void Reset()
    {
        // Автоматически подхватываем RectTransform того же объекта
        target = GetComponent<RectTransform>();
    }

    public void Play()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        if (target == null) return;

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        // Старт с нуля
        target.localScale = Vector3.zero;

        // 1) Быстро вылететь до overshoot
        yield return ScaleTo(Vector3.one * overshootScale, Mathf.Max(0.01f, inDuration));

        // 2) Усадка к 1.0
        yield return ScaleTo(Vector3.one, Mathf.Max(0.01f, settleDuration));

        _routine = null;
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 start = target.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            // Сглаживание (ease-out)
            k = 1f - Mathf.Pow(1f - k, 3f);

            target.localScale = Vector3.LerpUnclamped(start, targetScale, k);
            yield return null;
        }

        target.localScale = targetScale;
    }
}