// Подключаем коллекции C# (List, HashSet и т.д.)
using System.Collections.Generic;

// Подключаем Unity API (ScriptableObject, SerializeField, атрибуты и пр.)
using UnityEngine;

// Атрибут добавляет пункт в Unity меню:
// Create -> Quiz -> Flower Database
// Так ты можешь создать этот ScriptableObject прямо из Project окна.
[CreateAssetMenu(fileName = "FlowerDatabase", menuName = "Quiz/Flower Database")]
public class FlowerDatabase : ScriptableObject
{
    // Основной список всех цветов в базе.
    // [SerializeField] = Unity будет сохранять это поле в ассете,
    // даже несмотря на то, что поле private.
    [SerializeField] private List<FlowerData> flowers = new List<FlowerData>();

    // Публичное свойство для чтения списка снаружи.
    // => flowers это короткая запись "вернуть flowers".
    // ВАЖНО: это свойство даёт доступ к самому List (не копию).
    public List<FlowerData> Flowers => flowers;

    // Метод для замены всего списка (удобно для editor-скрипта Auto Fill).
    public void SetFlowers(List<FlowerData> newFlowers)
    {
        // Если newFlowers == null, создаём новый пустой список,
        // чтобы не словить NullReferenceException позже.
        flowers = newFlowers ?? new List<FlowerData>();
    }

    // Структура для удобного хранения статистики по сложностям.
    // [System.Serializable] позволяет Unity видеть/сериализовать её, если нужно.
    [System.Serializable]
    public struct DifficultyStats
    {
        public int total;     // Всего валидных (не null) цветов
        public int easy;      // Количество цветов со сложностью Easy
        public int medium;    // Количество цветов со сложностью Medium
        public int hard;      // Количество цветов со сложностью Hard
        public int maxHard;   // Количество цветов со сложностью MaxHard
        public int nullItems; // Сколько элементов в списке == null
    }

    // Метод считает статистику по всем цветам в базе и возвращает её.
    public DifficultyStats GetDifficultyStats()
    {
        // Создаём пустую структуру статистики.
        // Все int по умолчанию будут = 0.
        DifficultyStats stats = new DifficultyStats();

        // Если список почему-то не создан, возвращаем "нули".
        if (flowers == null)
            return stats;

        // Проходим по всему списку цветов
        for (int i = 0; i < flowers.Count; i++)
        {
            // Берём текущий элемент списка
            FlowerData f = flowers[i];

            // Если элемент пустой (null), считаем его отдельно и идём дальше
            if (f == null)
            {
                stats.nullItems++;
                continue;
            }

            // Если элемент валидный, увеличиваем общий счётчик
            stats.total++;

            // Смотрим сложность текущего цветка и увеличиваем нужный счётчик
            switch (f.Difficulty)
            {
                case FlowerDifficulty.Easy:
                    stats.easy++;
                    break;

                case FlowerDifficulty.Medium:
                    stats.medium++;
                    break;

                case FlowerDifficulty.Hard:
                    stats.hard++;
                    break;

                case FlowerDifficulty.MaxHard:
                    stats.maxHard++;
                    break;
            }
        }

        // Возвращаем готовую статистику
        return stats;
    }

    // Метод ищет повторяющиеся ID в базе.
    // Например, если два FlowerData имеют Id = 25,
    // то в результат попадёт число 25.
    public List<int> GetDuplicateIds()
    {
        // Список, который вернём (список дублей ID)
        List<int> duplicates = new List<int>();

        // Если списка нет, сразу возвращаем пустой список
        if (flowers == null) return duplicates;

        // seen = множество ID, которые мы уже встречали
        HashSet<int> seen = new HashSet<int>();

        // added = множество дублей, которые уже добавили в duplicates,
        // чтобы один и тот же ID не записался туда 10 раз
        HashSet<int> added = new HashSet<int>();

        // Проходим по всем цветам
        for (int i = 0; i < flowers.Count; i++)
        {
            // Берём текущий элемент
            FlowerData f = flowers[i];

            // Пропускаем пустые элементы
            if (f == null) continue;

            // seen.Add(...) возвращает:
            // true  -> если такого ID ещё не было (успешно добавили)
            // false -> если такой ID уже есть (значит это дубль)
            if (!seen.Add(f.Id))
            {
                // Если это дубль, добавим его в duplicates только один раз
                if (added.Add(f.Id))
                    duplicates.Add(f.Id);
            }
        }

        // Возвращаем список всех повторяющихся ID
        return duplicates;
    }
    // Вернуть список цветов со сложностью <= maxDifficulty
    // Используется режимом Pool Mode = By Max Difficulty
    public List<FlowerData> GetPoolByDifficultyMax(FlowerDifficulty maxDifficulty)
    {
        // Создаём пустой список результата
        List<FlowerData> pool = new List<FlowerData>();

        // Если база не инициализирована, возвращаем пустой список
        if (flowers == null)
            return pool;

        // Проходим по всем элементам базы
        for (int i = 0; i < flowers.Count; i++)
        {
            // Берём текущий цветок
            FlowerData f = flowers[i];

            // Пропускаем пустые ссылки
            if (f == null)
                continue;

            // Если сложность цветка меньше или равна maxDifficulty — добавляем в пул
            if (f.Difficulty <= maxDifficulty)
                pool.Add(f);
        }

        // Возвращаем готовый пул
        return pool;
    }

    // Вернуть список цветов по диапазону ID [minId..maxId]
    // Используется режимом Pool Mode = By Id Range
    public List<FlowerData> GetPoolByIdRange(int minId, int maxId)
    {
        // Создаём пустой список результата
        List<FlowerData> pool = new List<FlowerData>();

        // Если база не инициализирована, возвращаем пустой список
        if (flowers == null)
            return pool;

        // Если диапазон передан наоборот (например 80, 61), меняем местами
        if (minId > maxId)
            (minId, maxId) = (maxId, minId);

        // Проходим по всем цветам в базе
        for (int i = 0; i < flowers.Count; i++)
        {
            // Берём текущий цветок
            FlowerData f = flowers[i];

            // Пропускаем пустые ссылки
            if (f == null)
                continue;

            // Пропускаем всё, что вне диапазона
            if (f.Id < minId) continue;
            if (f.Id > maxId) continue;

            // Если цветок попал в диапазон — добавляем в пул
            pool.Add(f);
        }

        // Возвращаем готовый пул
        return pool;
    }
}
/*//список из 110 FlowerData
using System.Collections.Generic;                                                                // Подключаем List<T>
using UnityEngine;                                                                               // Подключаем Unity API

[CreateAssetMenu(menuName = "Flowers/Flower Database", fileName = "FlowerDatabase")]             // Добавляем пункт создания SO в меню Unity
public class FlowerDatabase : ScriptableObject                                                    // База данных всех цветов
{                                                                                                 // Начало класса
    [SerializeField] private List<FlowerData> flowers = new List<FlowerData>();                   // Список всех FlowerData (110 штук)
    public IReadOnlyList<FlowerData> Flowers => flowers;                                          // Публичный read-only доступ к списку
                                                                                                  // Вернуть список цветов (только для чтения снаружи, но List всё равно сериализуется Unity)

    // Установить список цветов (удобно для editor-утилит автозаполнения)
    public void SetFlowers(List<FlowerData> newFlowers)
    {
        // Если пришёл null — создаём пустой список, чтобы избежать NullReference
        flowers = newFlowers ?? new List<FlowerData>();
    }

    public int Count => flowers != null ? flowers.Count : 0;                                      // Количество элементов (с защитой от null)
    // => 'get' , если flowers не равно null то верни flowers.Count иначе верни 0

    public FlowerData GetById(int id)                                                             // Получить цветок по ID (линейный поиск)
    {                                                                                             // Начало метода
        if (flowers == null) return null;                                                         // Если списка нет — возвращаем null
        for (int i = 0; i < flowers.Count; i++)                                                   // Проходим по всем элементам
        {                                                                                         // Начало цикла
            var f = flowers[i];                                                                   // Берём текущий элемент
            if (f == null) continue;                                                              // Пропускаем null ссылки
            if (f.Id == id) return f;                                                             // Если нашли совпадение — возвращаем
        }                                                                                         // Конец цикла
        return null;                                                                              // Если не нашли — возвращаем null
    }                                                                                             // Конец метода
//“фильтр по диапазону ID”: верни все FlowerData, у которых Id между minId и maxId включительно.
    public List<FlowerData> GetPoolByIdRange(int minId, int maxId)                                 // Получить пул по диапазону ID
    {                                                                                              // Начало метода
        var pool = new List<FlowerData>();                                                         // Создаём список результатов
        if (flowers == null) return pool;                                                          // Если списка нет — возвращаем пустой пул
        if (minId > maxId) (minId, maxId) = (maxId, minId);                                        // Если перепутали границы — меняем местами

        for (int i = 0; i < flowers.Count; i++)                                                    // Идём по всем цветкам
        {                                                                                          // Начало цикла
            var f = flowers[i];                                                                    // Берём цветок
            if (f == null) continue;                                                               // Пропускаем null
            if (f.Id < minId) continue;                                                            // Если меньше минимума — пропускаем
            if (f.Id > maxId) continue;                                                            // Если больше максимума — пропускаем
            pool.Add(f);                                                                            // Добавляем в пул
        }                                                                                          // Конец цикла

        return pool;                                                                               // Возвращаем пул
    }                                                                                              // Конец метода
    //Этот метод делает фильтр по сложности: возвращает список всех цветов, у которых Difficulty не выше, чем maxDifficulty.
    public List<FlowerData> GetPoolByDifficultyMax(FlowerDifficulty maxDifficulty)                 // Пул: все цветы сложностью <= maxDifficulty
    {                                                                                              // Начало метода
        var pool = new List<FlowerData>();                                                         // Создаём список результатов
        if (flowers == null) return pool;                                                          // Защита от null

        for (int i = 0; i < flowers.Count; i++)                                                    // Проходим по всем цветкам
        {                                                                                          // Начало цикла
            var f = flowers[i];                                                                    // Берём элемент
            if (f == null) continue;                                                               // Пропускаем null
            if (f.Difficulty <= maxDifficulty) pool.Add(f);                                        // Добавляем, если сложность подходит
        }                                                                                          // Конец цикла

        return pool;                                                                               // Возвращаем пул
    }                                                                                              // Конец метода

#if UNITY_EDITOR                                                                                  // Ниже только для редактора
    private void OnValidate()                                                                       // Проверки в редакторе
    {                                                                                               // Начало OnValidate
        if (flowers == null) flowers = new List<FlowerData>();                                      // Гарантируем, что список не null
    }                                                                                               // Конец OnValidate
#endif                                                                                             // Конец директивы
}       */                                                                                          // Конец класса