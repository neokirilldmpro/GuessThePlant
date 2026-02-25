// File: QuizUIView.cs                                                                 // Имя файла в проекте
using System;                                                                           // Action<T>
using TMPro;                                                                            // TextMeshPro
using UnityEngine;                                                                      // Unity API
using UnityEngine.UI;                                                                   // UI компоненты (Image, Button)

public class QuizUIView : MonoBehaviour                                                  // UI-представление викторины
{                                                                                        // Начало класса
    [Header("Image")]                                                                    // Заголовок инспектора
    [SerializeField] private Image flowerImage;                                          // UI Image для цветка (картинка вопроса)

    [Header("Question Text")]                                                            // Заголовок
    [SerializeField] private TMP_Text questionText;                                      // Текст вопроса (например "Как называется этот цветок?")

    [Header("Options")]                                                                  // Заголовок
    [SerializeField] private Button[] optionButtons;                                     // Массив кнопок вариантов (ожидаем 4)
    [SerializeField] private TMP_Text[] optionTexts;                                     // Массив текстов на кнопках (совпадает по индексам)

    [Header("HUD")]                                                                      // Заголовок
    [SerializeField] private TMP_Text progressText;                                      // Текст прогресса (например "3/20")
    [SerializeField] private TMP_Text scoreText;                                         // Текст счёта (например "Score: 2")
    [SerializeField] private TMP_Text streakText;                                        // Текст серии (например "Streak: 5")

    [Header("Feedback Colors")]                                                          // Заголовок
    [SerializeField] private Color normalColor = Color.white;                            // Цвет кнопки по умолчанию
    [SerializeField] private Color correctColor = Color.green;                           // Цвет правильного
    [SerializeField] private Color wrongColor = Color.red;                               // Цвет неправильного

    private Image[] _buttonImages;                                                       // Кеш Image компонентов кнопок

    public event Action<int> OptionClicked;                                              // Событие клика по варианту (индекс)

    private void Awake()                                                                  // Unity вызывается при создании объекта
    {                                                                                     // Начало Awake
        if (optionButtons == null || optionButtons.Length == 0)                          // Проверяем, что кнопки заданы
        {                                                                                 // Начало if
            Debug.LogError("[UI] optionButtons not assigned");                            // Логируем ошибку
            return;                                                                       // Выходим
        }                                                                                 // Конец if

        _buttonImages = new Image[optionButtons.Length];                                  // Создаём массив кеша Image

        for (int i = 0; i < optionButtons.Length; i++)                                    // Проходим по кнопкам
        {                                                                                 // Начало цикла
            int captured = i;                                                             // Захватываем индекс корректно (важно для лямбды)
            optionButtons[i].onClick.AddListener(() => OptionClicked?.Invoke(captured));  // Подписываем кнопку на событие
            _buttonImages[i] = optionButtons[i].GetComponent<Image>();                    // Кешируем Image для смены цвета
        }                                                                                 // Конец цикла
    }                                                                                     // Конец Awake

    public void SetInteractable(bool interactable)                                        // Включить/выключить кликабельность вариантов
    {                                                                                     // Начало метода
        if (optionButtons == null) return;                                                // Защита от null
        for (int i = 0; i < optionButtons.Length; i++)                                    // Идём по кнопкам
        {                                                                                 // Начало цикла
            if (optionButtons[i] != null) optionButtons[i].interactable = interactable;   // Применяем interactable
        }                                                                                 // Конец цикла
    }                                                                                     // Конец метода

    public void ResetOptionColors()                                                       // Сбросить цвета кнопок
    {                                                                                     // Начало метода
        if (_buttonImages == null) return;                                                // Защита
        for (int i = 0; i < _buttonImages.Length; i++)                                    // Идём по Image
        {                                                                                 // Начало цикла
            if (_buttonImages[i] != null) _buttonImages[i].color = normalColor;           // Ставим normal
        }                                                                                 // Конец цикла
    }                                                                                     // Конец метода

    public void ShowQuestion(FlowerQuestion q, int currentIndex, int total, int score, int streak, bool english) // Показать вопрос на UI
    {                                                                                     // Начало метода
        if (q == null)                                                                    // Защита
        {                                                                                 // Начало if
            Debug.LogError("[UI] Question is null");                                      // Лог
            return;                                                                       // Выход
        }                                                                                 // Конец if

        if (flowerImage != null)                                                         // Если есть ссылка на Image
        {                                                                                 // Начало if
            flowerImage.sprite = q.image;                                                 // Ставим спрайт
            flowerImage.enabled = (q.image != null);                                      // Прячем Image если null
            flowerImage.preserveAspect = true;                                            // Сохраняем пропорции
        }                                                                                 // Конец if

        if (questionText != null)                                                        // Если есть текст вопроса
        {                                                                                 // Начало if
            questionText.text = english ? "What is the name of this flower?" : "Как называется этот цветок?"; // Ставим текст по языку
        }                                                                                 // Конец if

        for (int i = 0; i < optionButtons.Length; i++)                                    // Заполняем варианты
        {                                                                                 // Начало цикла
            bool has = q.options != null && i < q.options.Length;                          // Есть ли строка для этого индекса
            if (optionButtons[i] != null) optionButtons[i].gameObject.SetActive(has);      // Показываем/прячем кнопку
            if (has && optionTexts != null && i < optionTexts.Length && optionTexts[i] != null) // Проверяем массив текстов
            {                                                                              // Начало if
                optionTexts[i].text = q.options[i];                                        // Ставим текст варианта
            }                                                                              // Конец if
        }                                                                                  // Конец цикла

        if (progressText != null) progressText.text = $"{currentIndex}/{total}";           // Обновляем прогресс
        if (scoreText != null) scoreText.text = english ? $"Score: {score}" : $"Счёт: {score}"; // Обновляем счёт
        if (streakText != null) streakText.text = english ? $"Streak: {streak}" : $"Серия: {streak}"; // Обновляем серию

        ResetOptionColors();                                                               // Сбрасываем цвета
        ResetOptionScales(); // Сбрасываем масштаб кнопок
        SetInteractable(true);                                                             // Включаем кнопки
    }                                                                                      // Конец метода

    public void MarkCorrect(int index)                                                     // Подсветить правильный вариант
    {                                                                                      // Начало метода
        if (_buttonImages == null) return;                                                 // Защита
        if (index < 0 || index >= _buttonImages.Length) return;                            // Проверка границ
        if (_buttonImages[index] != null) _buttonImages[index].color = correctColor;       // Ставим зелёный (или заданный)
    }                                                                                      // Конец метода

    public void MarkWrong(int index)                                                       // Подсветить выбранный неверный
    {                                                                                      // Начало метода
        if (_buttonImages == null) return;                                                 // Защита
        if (index < 0 || index >= _buttonImages.Length) return;                            // Проверка границ
        if (_buttonImages[index] != null) _buttonImages[index].color = wrongColor;         // Ставим красный (или заданный)
    }
    // Сбросить масштаб всех кнопок к 1 (полезно при показе нового вопроса)
    public void ResetOptionScales()
    {
        if (optionButtons == null) return; // Если массив кнопок не назначен — выходим

        for (int i = 0; i < optionButtons.Length; i++) // Идём по всем кнопкам
        {
            if (optionButtons[i] == null) continue; // Пропускаем пустые ссылки
            RectTransform rt = optionButtons[i].GetComponent<RectTransform>(); // Берём RectTransform кнопки
            if (rt != null) rt.localScale = Vector3.one; // Ставим нормальный масштаб
        }
    }

}                                                                                          // Конец класса
