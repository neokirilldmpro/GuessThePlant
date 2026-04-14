using TMPro;
using UnityEngine;

// Компонент, который заставляет TextMeshPro плавно мигать
[RequireComponent(typeof(TMP_Text))] // Автоматически добавит TMP компонент, если его нет
public class PulsingText : MonoBehaviour
{
    [Header("Настройки мигания")]
    [Tooltip("Скорость мигания (чем больше, тем быстрее)")]
    [SerializeField] private float pulseSpeed = 4.0f;

    [Tooltip("Минимальная прозрачность (от 0 до 1, где 0 - невидимый)")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.2f; // Чтобы текст не исчезал полностью

    [Tooltip("Максимальная прозрачность (от 0 до 1, где 1 - полностью непрозрачный)")]
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 1.0f;

    private TMP_Text textComponent; // Ссылка на компонент текста
    private float time;            // Локальное время для расчета

    void Start()
    {
        // Получаем компонент текста при старте
        textComponent = GetComponent<TMP_Text>();

        // Убеждаемся, что мы не сломаем игру, если компонента нет
        if (textComponent == null)
        {
            Debug.LogError($"Нет TextMeshPro компонента на объекте {gameObject.name}!");
            enabled = false; // Отключаем скрипт
        }
    }

    void Update()
    {
        // Каждую секунду увеличиваем время
        time += Time.deltaTime;

        // Самая важная магия: используем синусоиду для получения плавного числа от -1 до 1
        // (Time.time * pulseSpeed) управляет скоростью колебаний
        float sine = Mathf.Sin(time * pulseSpeed);

        // Превращаем диапазон [-1, 1] в диапазон [0, 1] для Лерпа
        // (синус + 1) дает [0, 2], делим на 2, получаем [0, 1]
        float t = (sine + 1.0f) / 2.0f;

        // Плавно вычисляем новую прозрачность (Альфа-канал) между minAlpha и maxAlpha
        float newAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        // Применяем новую прозрачность к цвету текста
        Color textColor = textComponent.color;
        textColor.a = newAlpha;
        textComponent.color = textColor;
    }
}