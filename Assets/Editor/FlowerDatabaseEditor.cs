// Подключаем базовые коллекции
using System.Collections.Generic;

// Подключаем LINQ для сортировки/фильтрации (можно и без него, но так удобнее)
using System.Linq;

// Подключаем Unity runtime API (ScriptableObject и т.д.)
using UnityEngine;

// Подключаем Unity Editor API (кастомный инспектор, AssetDatabase)
using UnityEditor;

// Говорим Unity: это кастомный инспектор для FlowerDatabase
[CustomEditor(typeof(FlowerDatabase))]
public class FlowerDatabaseEditor : Editor
{
    // Переопределяем отрисовку инспектора
    public override void OnInspectorGUI()
    {
        // Сначала рисуем обычный инспектор (стандартные поля FlowerDatabase)
        DrawDefaultInspector();

        // Небольшой отступ для красоты
        EditorGUILayout.Space(10);

        // Получаем ссылку на редактируемый FlowerDatabase
        FlowerDatabase database = (FlowerDatabase)target;

        // Кнопка: автоматически собрать все FlowerData из проекта
        if (GUILayout.Button("Auto Fill From All FlowerData In Project", GUILayout.Height(32)))
        {
            AutoFillDatabase(database);
        }

        // Кнопка: только отсортировать текущий список по ID
        if (GUILayout.Button("Sort Current List By ID", GUILayout.Height(26)))
        {
            SortDatabase(database);
        }

        // Кнопка: удалить null-элементы из списка
        if (GUILayout.Button("Remove Null Entries", GUILayout.Height(26)))
        {
            RemoveNullEntries(database);
        }
    }

    // Автозаполнение базы всеми FlowerData из проекта
    private static void AutoFillDatabase(FlowerDatabase database)
    {
        // Проверка на null (на всякий случай)
        if (database == null)
        {
            Debug.LogError("[FlowerDatabaseEditor] Database is null.");
            return;
        }

        // Ищем GUID всех ассетов типа FlowerData во всём проекте
        string[] guids = AssetDatabase.FindAssets("t:FlowerData");

        // Временный список для найденных элементов
        List<FlowerData> found = new List<FlowerData>();

        // Проходим по всем найденным GUID
        for (int i = 0; i < guids.Length; i++)
        {
            // Получаем путь ассета по GUID
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            // Загружаем ассет как FlowerData
            FlowerData data = AssetDatabase.LoadAssetAtPath<FlowerData>(path);

            // Если загрузка успешна — добавляем в список
            if (data != null)
            {
                found.Add(data);
            }
        }

        // Убираем возможные дубликаты ссылок (редко, но пусть будет защита)
        found = found.Distinct().ToList();

        // Сортируем по ID (null в конец, но null мы уже не добавляем)
        found = found.OrderBy(x => x != null ? x.Id : int.MaxValue).ToList();

        // ВАЖНО: записываем список в FlowerDatabase
        // Ниже зависит от того, есть ли у тебя публичное поле/свойство.
        // Если поле flowers public/serialized, и есть свойство-обёртка — используем метод доступа.
        // Смотри комментарий ниже.
        AssignListToDatabase(database, found);

        // Помечаем ассет как изменённый (чтобы Unity сохранил его)
        EditorUtility.SetDirty(database);

        // Сохраняем изменения в ассетах
        AssetDatabase.SaveAssets();

        // Обновляем базу ассетов
        AssetDatabase.Refresh();

        Debug.Log($"[FlowerDatabaseEditor] AutoFill complete. Added {found.Count} FlowerData assets to database '{database.name}'.");
    }

    // Сортировка текущего списка базы по ID
    private static void SortDatabase(FlowerDatabase database)
    {
        if (database == null)
        {
            Debug.LogError("[FlowerDatabaseEditor] Database is null.");
            return;
        }

        // Получаем текущий список
        List<FlowerData> current = GetDatabaseListCopy(database);

        // Сортируем по ID
        current = current
            .Where(x => x != null) // Убираем null сразу
            .OrderBy(x => x.Id)    // Сортируем по id
            .ToList();

        // Записываем обратно
        AssignListToDatabase(database, current);

        // Сохраняем
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FlowerDatabaseEditor] Sort complete. Items: {current.Count}");
    }

    // Удаление null элементов из списка
    private static void RemoveNullEntries(FlowerDatabase database)
    {
        if (database == null)
        {
            Debug.LogError("[FlowerDatabaseEditor] Database is null.");
            return;
        }

        // Получаем текущий список
        List<FlowerData> current = GetDatabaseListCopy(database);

        int before = current.Count; // Количество до очистки

        // Убираем null
        current = current.Where(x => x != null).ToList();

        int removed = before - current.Count; // Сколько удалили

        // Записываем обратно
        AssignListToDatabase(database, current);

        // Сохраняем
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FlowerDatabaseEditor] Removed null entries: {removed}. Remaining: {current.Count}");
    }

    // ===== СЛУЖЕБНЫЕ МЕТОДЫ =====

    // Получить копию списка из FlowerDatabase
    // Здесь предполагается, что у твоего FlowerDatabase есть метод/свойство доступа к списку.
    private static List<FlowerData> GetDatabaseListCopy(FlowerDatabase database)
    {
        // ВАЖНО:
        // Если у тебя в FlowerDatabase поле называется flowers и оно public:
        // return new List<FlowerData>(database.flowers);
        //
        // Если поле private [SerializeField] и есть свойство Flowers:
        // return new List<FlowerData>(database.Flowers);
        //
        // Ниже — версия под свойство Flowers (самый частый вариант):
        return new List<FlowerData>(database.Flowers);
    }

    // Записать список в FlowerDatabase
    private static void AssignListToDatabase(FlowerDatabase database, List<FlowerData> list)
    {
        // ВАЖНО:
        // Если у тебя поле public List<FlowerData> flowers; -> database.flowers = list;
        // Если private [SerializeField] List<FlowerData> flowers; и есть метод SetFlowers(...) — вызови его.
        // Если только свойство IReadOnlyList — тогда лучше добавить метод в FlowerDatabase (ниже покажу).
        //
        // Ниже — вариант через метод SetFlowers (рекомендую):
        database.SetFlowers(list);
    }
}