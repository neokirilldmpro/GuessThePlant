//один цветок(id, RU/EN, сложность, Sprite)

using UnityEngine;                                                                             // Подключаем Unity API

[CreateAssetMenu(menuName = "Flowers/Flower Data", fileName = "FlowerData")]                    // Добавляем пункт создания SO в меню Unity
public class FlowerData : ScriptableObject                                                      // ScriptableObject с данными одного цветка
{                                                                                                // Начало класса
    [Header("Identity")]                                                                        // Заголовок в инспекторе для удобства
    [SerializeField] private int id = 1;                                                        // Уникальный ID (1..110)
    [SerializeField] private string nameRu = "Роза";                                            // Название на русском
    [SerializeField] private string nameEn = "Rose";                                            // Название на английском

    [Header("Classification")]                                                                  // Заголовок в инспекторе
    [SerializeField] private FlowerDifficulty difficulty = FlowerDifficulty.Easy;               // Сложность распознавания (для режимов)

    [Header("Visual")]                                                                          // Заголовок в инспекторе
    [SerializeField] private Sprite image;                                                      // Спрайт (1024x1024) для вопроса

    public int Id => id;                                                                        // Публичный геттер ID
    public string NameRu => nameRu;                                                             // Публичный геттер RU названия
    public string NameEn => nameEn;                                                             // Публичный геттер EN названия
    public FlowerDifficulty Difficulty => difficulty;                                            // Публичный геттер сложности
    public Sprite Image => image;                                                               // Публичный геттер спрайта

#if UNITY_EDITOR                                                                               // Компилируем ниже только в редакторе
    private void OnValidate()                                                                    // Unity вызывает при изменении значений в инспекторе
    {                                                                                            // Начало OnValidate
        if (id < 1) id = 1;                                                                      // Страхуемся от некорректных ID
        if (string.IsNullOrWhiteSpace(nameRu)) nameRu = "—";                                    // Страхуемся от пустого RU названия
        if (string.IsNullOrWhiteSpace(nameEn)) nameEn = "—";                                    // Страхуемся от пустого EN названия
    }                                                                                            // Конец OnValidate
#endif                                                                                          // Конец директивы редактора
}                                                                                                // Конец класса