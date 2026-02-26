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

        // ===== ОСНОВНЫЕ КНОПКИ =====

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

        // Отступ перед блоком статистики
        EditorGUILayout.Space(8);

        // ===== СТАТИСТИКА И ВАЛИДАЦИЯ =====

        EditorGUILayout.LabelField("Database Tools", EditorStyles.boldLabel);

        // Кнопка: вывести статистику по сложностям в Console
        if (GUILayout.Button("Log Difficulty Stats", GUILayout.Height(26)))
        {
            LogDifficultyStats(database);
        }

        // Кнопка: проверить базу на типичные ошибки
        if (GUILayout.Button("Validate Database (IDs / Names / Images)", GUILayout.Height(26)))
        {
            ValidateDatabase(database);
        }

        // Отступ
        EditorGUILayout.Space(8);

        // ===== БЫСТРЫЙ ПРЕДПРОСМОТР СТАТИСТИКИ ПРЯМО В ИНСПЕКТОРЕ =====
        // Этот блок просто показывает цифры без нажатия кнопки (удобно)
        DrawStatsPreview(database);
    }

    // Автозаполнение базы всеми FlowerData из проекта
    private static void AutoFillDatabase(FlowerDatabase database)
    {
        if (database == null)
        {
            Debug.LogError("[FlowerDatabaseEditor] Database is null.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:FlowerData");
        List<FlowerData> found = new List<FlowerData>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            FlowerData data = AssetDatabase.LoadAssetAtPath<FlowerData>(path);

            if (data != null)
            {
                found.Add(data);
            }
        }

        found = found.Distinct().ToList();
        found = found.OrderBy(x => x != null ? x.Id : int.MaxValue).ToList();

        AssignListToDatabase(database, found);

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
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

        List<FlowerData> current = GetDatabaseListCopy(database);

        current = current
            .Where(x => x != null)
            .OrderBy(x => x.Id)
            .ToList();

        AssignListToDatabase(database, current);

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

        List<FlowerData> current = GetDatabaseListCopy(database);

        int before = current.Count;
        current = current.Where(x => x != null).ToList();
        int removed = before - current.Count;

        AssignListToDatabase(database, current);

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[FlowerDatabaseEditor] Removed null entries: {removed}. Remaining: {current.Count}");
    }

    // ===== НОВОЕ: ЛОГ СТАТИСТИКИ =====

    private static void LogDifficultyStats(FlowerDatabase database)
    {
        if (database == null)
        {
            Debug.LogError("[FlowerDatabaseEditor] Database is null.");
            return;
        }

        // Получаем статистику из FlowerDatabase (runtime-логика)
        FlowerDatabase.DifficultyStats s = database.GetDifficultyStats();

        // Получаем дубли ID
        List<int> duplicates = database.GetDuplicateIds();

        string duplicatesText = (duplicates == null || duplicates.Count == 0)
            ? "none"
            : string.Join(", ", duplicates);

        Debug.Log(
            $"[FlowerDatabaseEditor] Stats for '{database.name}': " +
            $"Total={s.total}, Easy={s.easy}, Medium={s.medium}, Hard={s.hard}, MaxHard={s.maxHard}, Nulls={s.nullItems}, DuplicateIDs={duplicatesText}"
        );
    }

    // ===== НОВОЕ: ВАЛИДАЦИЯ =====

    private static void ValidateDatabase(FlowerDatabase database)
    {
        if (database == null)
        {
            Debug.LogError("[FlowerDatabaseEditor] Database is null.");
            return;
        }

        List<FlowerData> list = GetDatabaseListCopy(database);

        int nullEntries = 0;
        int missingRu = 0;
        int missingEn = 0;
        int missingImage = 0;

        for (int i = 0; i < list.Count; i++)
        {
            FlowerData f = list[i];

            if (f == null)
            {
                nullEntries++;
                continue;
            }

            // Проверка пустого RU имени
            if (string.IsNullOrWhiteSpace(f.NameRu))
            {
                missingRu++;
                Debug.LogWarning($"[FlowerDatabaseEditor] Missing NameRu: asset='{f.name}', id={f.Id}");
            }

            // Проверка пустого EN имени
            if (string.IsNullOrWhiteSpace(f.NameEn))
            {
                missingEn++;
                Debug.LogWarning($"[FlowerDatabaseEditor] Missing NameEn: asset='{f.name}', id={f.Id}");
            }

            // Проверка отсутствия картинки
            if (f.Image == null)
            {
                missingImage++;
                Debug.LogWarning($"[FlowerDatabaseEditor] Missing Image: asset='{f.name}', id={f.Id}");
            }
        }

        List<int> duplicates = database.GetDuplicateIds();

        string duplicatesText = (duplicates == null || duplicates.Count == 0)
            ? "none"
            : string.Join(", ", duplicates);

        Debug.Log(
            $"[FlowerDatabaseEditor] Validation complete for '{database.name}': " +
            $"NullEntries={nullEntries}, MissingRu={missingRu}, MissingEn={missingEn}, MissingImage={missingImage}, DuplicateIDs={duplicatesText}"
        );
    }

    // ===== НОВОЕ: ПРЕДПРОСМОТР СТАТИСТИКИ В ИНСПЕКТОРЕ =====

    private static void DrawStatsPreview(FlowerDatabase database)
    {
        if (database == null)
        {
            EditorGUILayout.HelpBox("Database is null.", MessageType.Error);
            return;
        }

        FlowerDatabase.DifficultyStats s = database.GetDifficultyStats();
        List<int> duplicates = database.GetDuplicateIds();

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Quick Stats Preview", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Total", s.total.ToString());
        EditorGUILayout.LabelField("Easy", s.easy.ToString());
        EditorGUILayout.LabelField("Medium", s.medium.ToString());
        EditorGUILayout.LabelField("Hard", s.hard.ToString());
        EditorGUILayout.LabelField("MaxHard", s.maxHard.ToString());
        EditorGUILayout.LabelField("Null Entries", s.nullItems.ToString());

        string duplicateText = (duplicates == null || duplicates.Count == 0)
            ? "None"
            : string.Join(", ", duplicates);

        EditorGUILayout.LabelField("Duplicate IDs", duplicateText);

        EditorGUILayout.EndVertical();
    }

    // ===== СЛУЖЕБНЫЕ МЕТОДЫ =====

    private static List<FlowerData> GetDatabaseListCopy(FlowerDatabase database)
    {
        return new List<FlowerData>(database.Flowers);
    }

    private static void AssignListToDatabase(FlowerDatabase database, List<FlowerData> list)
    {
        database.SetFlowers(list);
    }
}