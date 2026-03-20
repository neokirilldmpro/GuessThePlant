using UnityEngine;

// Этот ScriptableObject описывает ОДИН этап кампании.
// Раньше он у тебя был просто "preset сложности",
// теперь он становится полноценным описанием этапа.
[CreateAssetMenu(fileName = "QuizDifficultyPreset", menuName = "Quiz/Difficulty Preset")]
public class QuizDifficultyPreset : ScriptableObject
{

    [Header("Stage identity")]
    [Tooltip("Это стабильный внутренний ключ этапа.Он нужен для сохранений.")]
    [SerializeField] private string stageKey;
 
    // Почему нельзя опираться только на красивое имя?
    // Потому что красивое имя ты потом можешь поменять:
    // например "Лёгкий 1" -> "Весенний старт".
    // А stageKey должен оставаться одним и тем же.
    //
    // Примеры:
    // easy_1
    // easy_2
    // medium_1
    // expert_2
    [Tooltip("Порядковый номер этапа в кампании.Нужен для UI, прогрессии и сортировки.Диапазон: 1..8")]
    [SerializeField] private int stageOrder = 1;

    [Header("Display names")]
    [SerializeField] private string displayNameRu = "Этап";
    // Красивое название этапа на русском

    [SerializeField] private string displayNameEn = "Stage";
    // Красивое название этапа на английском

    [Header("Quiz settings")]
    [SerializeField] private string presetName = "Easy";
    // Это твоё старое поле / или аналог старого названия режима.
    // Пока оставляем для совместимости со старой логикой,
    // чтобы не ломать проект слишком резко.

    [Tooltip("Сколько вопросов будет в этом этапе.Для новой кампании советую 15.")]
    [SerializeField] private int questionsCount = 15;

    [Tooltip("Нужно ли стараться избегать повторов правильных цветов в одном этапе.")]
    [SerializeField] private bool uniqueCorrectAnswers = true;

    [Tooltip("Каким способом набирается пул вопросов. Сейчас для кампании удобнее всего использовать ByIdRange.")]
    [SerializeField] private QuestionPoolMode poolMode = QuestionPoolMode.ByIdRange;
   
    [Tooltip("Минимальный ID цветка для режима ByIdRange")]
    [SerializeField] private int minId = 1;

    [Tooltip("Максимальный ID цветка для режима ByIdRange")]
    [SerializeField] private int maxId = 15;

    [SerializeField] private FlowerDifficulty maxDifficulty = FlowerDifficulty.Easy;
    // Используется, если poolMode = ByMaxDifficulty

    [Header("Timer")]
    [Tooltip("Включён ли таймер на этом этапе")]
    [SerializeField] private bool useTimer = false;

    [Tooltip("Сколько секунд даётся на один вопрос. Если useTimer = false, это поле можно игнорировать.")]
    [SerializeField] private float secondsPerQuestion = 0f;

    [Header("Unlock rule")]
    [Tooltip("Предыдущий этап, который нужно пройти, чтобы открыть этот.Для первого этапа здесь будет null.")]
    [SerializeField] private QuizDifficultyPreset previousStage;

    [Header("Perfect reward")]
    [Tooltip("Какая нашивка выдаётся за perfect-прохождение этого этапа.")]
    [SerializeField] private AchievementId perfectAchievementId = AchievementId.PerfectEasy1;

    // -------------------------
    // ПУБЛИЧНЫЕ СВОЙСТВА
    // -------------------------

    public string StageKey => stageKey;
    public int StageOrder => stageOrder;
    public string DisplayNameRu => displayNameRu;
    public string DisplayNameEn => displayNameEn;

    public string PresetName => presetName;
    public int QuestionsCount => questionsCount;
    public bool UniqueCorrectAnswers => uniqueCorrectAnswers;
    public QuestionPoolMode PoolMode => poolMode;
    public int MinId => minId;
    public int MaxId => maxId;
    public FlowerDifficulty MaxDifficulty => maxDifficulty;

    public bool UseTimer => useTimer;
    public float SecondsPerQuestion => secondsPerQuestion;

    public QuizDifficultyPreset PreviousStage => previousStage;
    public AchievementId PerfectAchievementId => perfectAchievementId;

    // Удобный метод:
    // вернуть локализованное название этапа.
    public string GetDisplayName(bool useEnglish)
    {
        return useEnglish ? displayNameEn : displayNameRu;
    }

    // Удобный флаг:
    // это первый этап кампании или нет.
    public bool IsFirstStage()
    {
        return previousStage == null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Этот метод вызывается в редакторе,
        // когда ты меняешь значения в инспекторе.

        // Не даём номеру этапа стать меньше 1.
        if (stageOrder < 1)
            stageOrder = 1;

        // Не даём количеству вопросов стать меньше 1.
        if (questionsCount < 1)
            questionsCount = 1;

        // Если таймер выключен —
        // для аккуратности обнуляем секунды.
        if (!useTimer)
            secondsPerQuestion = 0f;

        // Если таймер включён, но секунд мало или 0 —
        // ставим безопасное значение.
        if (useTimer && secondsPerQuestion <= 0f)
            secondsPerQuestion = 10f;

        // Защита от ситуации, когда minId больше maxId.
        if (minId > maxId)
        {
            int temp = minId;
            minId = maxId;
            maxId = temp;
        }
    }
#endif
}

// Это enum режима набора пула.
// Скорее всего он у тебя уже есть.
// Если уже есть такой enum в проекте - второй раз НЕ создавай.
public enum QuestionPoolMode
{
    ByIdRange,
    ByMaxDifficulty
}