// Подключаем корутины (IEnumerator)
using System.Collections;

// Подключаем Unity API
using UnityEngine;

// Подключаем UI (Button)
using UnityEngine.UI;

// Подключаем интерфейсы указателя мыши/тача (hover/press)
using UnityEngine.EventSystems;

// Компонент для UI-кнопки: анимация масштаба + звук клика
// Повесь этот скрипт на каждую кнопку (OptionButton_0..3)
[RequireComponent(typeof(Button))] // Гарантируем, что на объекте есть Button
public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")] // Заголовок в инспекторе
    [SerializeField] private float normalScale = 1f; // Обычный масштаб кнопки
    [SerializeField] private float hoverScale = 1.03f; // Масштаб при наведении мыши
    [SerializeField] private float pressedScale = 0.96f; // Масштаб при нажатии
    [SerializeField] private float scaleLerpSpeed = 16f; // Скорость плавного перехода масштаба

    /*[Header("Sound")] // Заголовок в инспекторе
    [SerializeField] private AudioSource audioSource; // Источник звука (можно общий на сцене)
    [SerializeField] private AudioClip clickClip; // Звук клика по кнопке
    [SerializeField] [Range(0f, 1f)] private float clickVolume = 1f; // Громкость клика*/

    private Button _button; // Кеш ссылки на Button
    private RectTransform _rect; // Кеш ссылки на RectTransform (чтобы менять scale)
    private Coroutine _scaleRoutine; // Ссылка на текущую корутину анимации (чтобы останавливать предыдущую)

    private bool _isPointerOver; // Флаг: курсор находится над кнопкой
    private bool _isPointerDown; // Флаг: кнопка сейчас зажата

    private void Awake() // Unity вызывает при создании объекта
    {
        _button = GetComponent<Button>(); // Получаем Button на этом объекте
        _rect = GetComponent<RectTransform>(); // Получаем RectTransform для анимации масштаба

        if (_rect != null) // Если RectTransform найден
        {
            _rect.localScale = Vector3.one * normalScale; // Ставим стартовый масштаб
        }

       /* if (_button != null) // Если Button найден
        {
            _button.onClick.AddListener(PlayClickSound); // Подписываемся на onClick для звука
        }*/
    }

    /*private void OnDestroy() // Unity вызывает при уничтожении объекта
    {
        if (_button != null) // Если Button существует
        {
            _button.onClick.RemoveListener(PlayClickSound); // Отписываемся от события (хорошая практика)
        }
    }*/

    public void OnPointerEnter(PointerEventData eventData) // Событие: курсор вошёл в кнопку
    {
        _isPointerOver = true; // Запоминаем, что курсор над кнопкой

        // Если кнопка сейчас не зажата, анимируем к hover scale
        if (!_isPointerDown)
        {
            AnimateToScale(hoverScale);
        }
    }

    public void OnPointerExit(PointerEventData eventData) // Событие: курсор вышел из кнопки
    {
        _isPointerOver = false; // Сбрасываем флаг hover

        // Если кнопка не зажата, возвращаем в обычный масштаб
        if (!_isPointerDown)
        {
            AnimateToScale(normalScale);
        }
    }

    public void OnPointerDown(PointerEventData eventData) // Событие: нажали на кнопку (мышь/тач)
    {
        // Если кнопка отключена, не анимируем
        if (_button != null && !_button.interactable)
            return;

        _isPointerDown = true; // Запоминаем состояние "зажата"

        // Анимируем к pressed scale
        AnimateToScale(pressedScale);
        PlayClickSound();
    }

    public void OnPointerUp(PointerEventData eventData) // Событие: отпустили кнопку
    {
        _isPointerDown = false; // Снимаем состояние "зажата"

        // Если курсор ещё над кнопкой — возвращаем к hover scale, иначе к normal
        AnimateToScale(_isPointerOver ? hoverScale : normalScale);
        
    }

    /*private void PlayClickSound() // Метод вызывается через Button.onClick
    {
        // Если кнопка отключена, звук не играем
        if (_button != null && !_button.interactable)
            return;

        // Если не назначен источник звука — просто выходим
        if (audioSource == null)
            return;

        // Если не назначен аудиоклип — просто выходим
        if (clickClip == null)
            return;

        // Проигрываем короткий звук клика без прерывания других звуков на AudioSource
        audioSource.PlayOneShot(clickClip, clickVolume);
    }*/
    private void PlayClickSound()
    {
        // Если кнопка отключена — звук не играем
        if (_button != null && !_button.interactable)
            return;

        // Если есть SfxManager — играем клик через него (с общей громкостью SFX)
        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayClick();
    }

    private void AnimateToScale(float targetScale) // Запуск плавной анимации к нужному масштабу
    {
        if (_rect == null) // Если нет RectTransform — выходим
            return;

        // Если предыдущая корутина ещё работает — останавливаем, чтобы не было конфликтов
        if (_scaleRoutine != null)
        {
            StopCoroutine(_scaleRoutine);
        }

        // Запускаем новую корутину плавного изменения масштаба
        _scaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(float targetScale) // Корутина плавного изменения масштаба
    {
        Vector3 target = Vector3.one * targetScale; // Целевой вектор масштаба (x=y=z)

        // Пока текущий scale заметно отличается от целевого — плавно двигаемся к цели
        while ((_rect.localScale - target).sqrMagnitude > 0.0001f)
        {
            // Плавно приближаем текущий масштаб к целевому
            _rect.localScale = Vector3.Lerp(_rect.localScale, target, Time.unscaledDeltaTime * scaleLerpSpeed);

            // Ждём следующий кадр
            yield return null;
        }

        // В конце ставим точное значение (чтобы не осталось микро-ошибки)
        _rect.localScale = target;

        // Очищаем ссылку на корутину (она завершилась)
        _scaleRoutine = null;
    }
}