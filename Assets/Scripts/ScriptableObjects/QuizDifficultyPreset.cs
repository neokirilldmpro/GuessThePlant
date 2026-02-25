// File: QuizDifficultyPreset.cs                                                                     // Имя файла в проекте
using UnityEngine;                                                                                   // Подключаем Unity API

public enum PoolMode                                                                                  // Режим формирования пула
{                                                                                                    // Начало enum
    ByIdRange = 0,                                                                                   // По диапазону ID
    ByMaxDifficulty = 1                                                                              // По максимальной сложности (<=)
}                                                                                                    // Конец enum

[CreateAssetMenu(menuName = "Flowers/Quiz Difficulty Preset", fileName = "QuizDifficultyPreset")]     // Создание пресета в меню
public class QuizDifficultyPreset : ScriptableObject                                                   // Пресет правил для режима (Easy/Medium/Hard/MaxHard)
{                                                                                                     // Начало класса
    [Header("Common")]                                                                                // Общие параметры
    [SerializeField] private string presetName = "Easy";                                              // Имя пресета (для UI)
    [SerializeField] private int questionsCount = 20;                                                 // Сколько вопросов в раунде
    [SerializeField] private bool uniqueCorrectAnswers = true;                                        // Не повторять правильные ответы по возможности

    [Header("Pool Mode")]                                                                             // Параметры пула
    [SerializeField] private PoolMode poolMode = PoolMode.ByIdRange;                                  // Как формируем пул

    [Header("By ID Range")]                                                                           // Используется, если ByIdRange
    [SerializeField] private int minId = 1;                                                           // Минимальный ID
    [SerializeField] private int maxId = 110;                                                         // Максимальный ID

    [Header("By Max Difficulty")]                                                                     // Используется, если ByMaxDifficulty
    [SerializeField] private FlowerDifficulty maxDifficulty = FlowerDifficulty.Easy;                  // Верхняя граница сложности

    [Header("Language")]                                                                              // Язык раунда
    [SerializeField] private bool useEnglish = false;                                                 // true = EN, false = RU

    public string PresetName => presetName;                                                           // Геттер имени пресета
    public int QuestionsCount => questionsCount;                                                      // Геттер количества вопросов
    public bool UniqueCorrectAnswers => uniqueCorrectAnswers;                                         // Геттер уникальности правильных
    public PoolMode PoolMode => poolMode;                                                             // Геттер режима пула
    public int MinId => minId;                                                                        // Геттер minId
    public int MaxId => maxId;                                                                        // Геттер maxId
    public FlowerDifficulty MaxDifficulty => maxDifficulty;                                           // Геттер maxDifficulty
    public bool UseEnglish => useEnglish;                                                             // Геттер языка

#if UNITY_EDITOR                                                                                      // Только в редакторе
    private void OnValidate()                                                                          // Автоподправление параметров
    {                                                                                                  // Начало OnValidate
        if (questionsCount < 1) questionsCount = 1;                                                    // Минимум 1 вопрос
        if (minId < 1) minId = 1;                                                                      // Минимум 1
        if (maxId < 1) maxId = 1;                                                                      // Минимум 1
        if (minId > maxId) (minId, maxId) = (maxId, minId);                                           // Границы не должны быть перепутаны
        if (string.IsNullOrWhiteSpace(presetName)) presetName = name;                                  // Если пусто — берём имя ассета
    }                                                                                                  // Конец OnValidate
#endif                                                                                                 // Конец директивы
}                                                                                                     // Конец класса