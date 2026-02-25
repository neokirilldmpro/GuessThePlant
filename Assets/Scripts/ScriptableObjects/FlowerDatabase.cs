//список из 110 FlowerData
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
}                                                                                                 // Конец класса