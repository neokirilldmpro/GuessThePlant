// File: SimpleSaveService.cs                                                      // Имя файла в проекте
using UnityEngine;                                                                  // Unity API (PlayerPrefs)

public static class SimpleSaveService                                                // Простейший сервис сохранений через PlayerPrefs
{                                                                                    // Начало класса
    private const string KeyPrefix = "FLOWER_QUIZ_";                                  // Префикс ключей, чтобы не пересекаться с другими данными

    public static void SaveBestScore(string presetName, int bestScore)               // Сохраняем лучший результат для пресета
    {                                                                                // Начало метода
        string key = KeyPrefix + "BEST_" + presetName;                               // Формируем ключ
        PlayerPrefs.SetInt(key, bestScore);                                          // Записываем int
        PlayerPrefs.Save();                                                          // Принудительно сохраняем (важно для WebGL/браузера)
    }                                                                                // Конец метода

    public static int LoadBestScore(string presetName)                               // Загружаем лучший результат
    {                                                                                // Начало метода
        string key = KeyPrefix + "BEST_" + presetName;                               // Формируем ключ
        return PlayerPrefs.GetInt(key, 0);                                           // Читаем (по умолчанию 0)
    }                                                                                // Конец метода

    public static void SaveCompleted(string presetName, bool completed)              // Сохраняем факт прохождения режима
    {                                                                                // Начало метода
        string key = KeyPrefix + "DONE_" + presetName;                               // Формируем ключ
        PlayerPrefs.SetInt(key, completed ? 1 : 0);                                  // Храним как 1/0
        PlayerPrefs.Save();                                                          // Сохраняем
    }                                                                                // Конец метода

    public static bool LoadCompleted(string presetName)                               // Загружаем факт прохождения
    {                                                                                 // Начало метода
        string key = KeyPrefix + "DONE_" + presetName;                                // Формируем ключ
        return PlayerPrefs.GetInt(key, 0) == 1;                                       // Возвращаем true если 1
    }                                                                                 // Конец метода
}                                                                                     // Конец класса