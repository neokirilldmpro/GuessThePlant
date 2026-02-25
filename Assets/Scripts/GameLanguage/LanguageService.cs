using UnityEngine;

// Статический сервис для хранения и загрузки выбранного языка.
// Работает через PlayerPrefs и используется в меню и в игре.
public static class LanguageService
{
    private const string LanguageKey = "FLOWER_QUIZ_LANGUAGE"; // Ключ для PlayerPrefs

    // Текущий язык в памяти (кеш), чтобы не читать PlayerPrefs каждый раз
    private static bool _isLoaded = false;
    private static GameLanguage _currentLanguage = GameLanguage.Russian;

    // Получить текущий язык (с ленивой загрузкой из PlayerPrefs)
    public static GameLanguage CurrentLanguage
    {
        get
        {
            EnsureLoaded(); // Гарантируем, что язык загружен
            return _currentLanguage; // Возвращаем кеш
        }
    }

    // Удобное свойство: true если выбран английский
    public static bool UseEnglish
    {
        get
        {
            return CurrentLanguage == GameLanguage.English;
        }
    }

    // Установить язык и сохранить
    public static void SetLanguage(GameLanguage language)
    {
        _currentLanguage = language; // Обновляем кеш
        _isLoaded = true; // Помечаем как загруженное состояние

        PlayerPrefs.SetInt(LanguageKey, (int)language); // Сохраняем как int
        PlayerPrefs.Save(); // Принудительно сохраняем
    }

    // Переключить язык RU <-> EN
    public static void ToggleLanguage()
    {
        if (CurrentLanguage == GameLanguage.Russian)
        {
            SetLanguage(GameLanguage.English);
        }
        else
        {
            SetLanguage(GameLanguage.Russian);
        }
    }

    // Загрузить язык из PlayerPrefs (если ещё не загружен)
    private static void EnsureLoaded()
    {
        if (_isLoaded) return; // Если уже загружено — ничего не делаем

        int value = PlayerPrefs.GetInt(LanguageKey, (int)GameLanguage.Russian); // По умолчанию RU

        // Безопасная проверка диапазона
        if (value < (int)GameLanguage.Russian || value > (int)GameLanguage.English)
        {
            value = (int)GameLanguage.Russian;
        }

        _currentLanguage = (GameLanguage)value; // Применяем
        _isLoaded = true; // Отмечаем, что загрузили
    }
}