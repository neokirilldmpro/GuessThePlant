using UnityEngine;
/*Этот сервис отвечает за «Призрака» — твой собственный лучший результат в этом режиме.*/
public static class GhostRecordService
{
    // Ключи для сохранения в реестр PlayerPrefs
    private const string KeyScore = "GHOST_SCORE";
    private const string KeyTime = "GHOST_TIME";

    // Проверяем, играл ли пользователь раньше (есть ли сохраненный счет)
    public static bool HasGhost() => PlayerPrefs.HasKey(KeyScore);

    // Загружаем лучший счет "Призрака" (по умолчанию 0)
    public static int LoadScore() => PlayerPrefs.GetInt(KeyScore, 0);

    // Загружаем лучшее время "Призрака" (по умолчанию 30 сек)
    public static float LoadTime() => PlayerPrefs.GetFloat(KeyTime, 30f);

    // Метод попытки обновить рекорд
    public static void TrySaveNewGhost(int score, float time)
    {
        int curScore = LoadScore(); // Текущий лучший счет
        float curTime = LoadTime(); // Текущее лучшее время

        // Условие: новый результат лучше, если очков БОЛЬШЕ 
        // ИЛИ очков столько же, но время МЕНЬШЕ (быстрее)
        if (score > curScore || (score == curScore && time < curTime))
        {
            PlayerPrefs.SetInt(KeyScore, score);    // Сохраняем очки
            PlayerPrefs.SetFloat(KeyTime, time);    // Сохраняем время
            PlayerPrefs.Save();                     // Записываем на диск
        }
    }
}